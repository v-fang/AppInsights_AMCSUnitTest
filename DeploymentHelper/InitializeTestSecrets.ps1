[CmdletBinding()]
Param(
    [Parameter(Mandatory=$false)][string]$AADApplicationId,
    [Parameter(Mandatory=$false)][string]$AADApplicationKey)


function Import-Certificate([string]$CertificateBase64) {
    $CertificateBytes = [Convert]::FromBase64String($CertificateBase64)

    [System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]$StorageFlags = [System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]`
        ([int][System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]::PersistKeySet `
        + [int][System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]::MachineKeySet `
        + [int][System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]::Exportable)
    $CertificateX509 = New-Object -TypeName System.Security.Cryptography.X509Certificates.X509Certificate2 @(
        $CertificateBytes, 
        $null, 
        $StorageFlags)

    [Console]::WriteLine('VERBOSE: Certificate thumbprint: ' + $CertificateX509.Thumbprint)
    if (Test-Path -Path Cert:\CurrentUser\My\$CertificateX509.Thumbprint) {
        [Console]::WriteLine('Cleaning up existing certificate')
        Remove-Item -Path Cert:\CurrentUser\My\$CertificateX509.Thumbprint -Force
    }
    [Console]::WriteLine('Installing certificate with thumbprint ' + $CertificateX509.Thumbprint)
    $CurrentUserMyStore = New-Object -TypeName System.Security.Cryptography.X509Certificates.X509Store @([System.Security.Cryptography.X509Certificates.StoreName]::My, [System.Security.Cryptography.X509Certificates.StoreLocation]::CurrentUser)
    $CurrentUserMyStore.Open([System.Security.Cryptography.X509Certificates.OpenFlags]::ReadWrite)
    $CurrentUserMyStore.Add($CertificateX509)
    $CurrentUserMyStore.Close()
    [Console]::WriteLine('Done importing certificate with thumbprint ' + $CertificateX509.Thumbprint + ' into CURRENTUSER\My certificate store!')

    return $CertificateX509
}

function Convert-Base64ToHex([string]$Base64String) {
    $Bytes = [Convert]::FromBase64String($Base64String)
    $BytesParts = @($Bytes | % { return [string]::Format('{0:X2}', $_) })
    $HexString = [string]::Join(@(''), $BytesParts)
   
    return $HexString
}

$MicrosoftComAADTenantId = [Guid]::Parse('72f988bf-86f1-41af-91ab-2d7cd011db47')

if (-not [string]::IsNullOrEmpty($AADApplicationId) -and -not [string]::IsNullOrEmpty($AADApplicationKey)) {
    [Console]::WriteLine('Logging in with specified AAD Application credentials...')
    $AADApplicationKeySecure = ConvertTo-SecureString $AADApplicationKey -AsPlainText -Force
    $AzureRmCredential = New-Object System.Management.Automation.PSCredential ($AADApplicationId, $AADApplicationKeySecure)
    Login-AzureRmAccount -Credential $AzureRmCredential -ServicePrincipal -TenantId $MicrosoftComAADTenantId
    [Console]::WriteLine('Done logging in with specified AAD Application credentials!')
} else {
    $ProfileCachePath = [IO.Path]::Combine($PSScriptRoot, [string]::Format("{0}.out.json", @([IO.Path]::GetFileNameWithoutExtension($MyInvocation.MyCommand.Definition))))
    [bool]$loginSuccessful = $false
    [Console]::WriteLine("Checking for Azure profile cache at " + $ProfileCachePath + "...")
    if (Test-Path -Path $ProfileCachePath) {
        [Console]::WriteLine("Attempting to read cached Azure profile from " + $ProfileCachePath)
        Import-AzureRmContext -Path $ProfileCachePath
        try {
            $azureSubscriptions = @(Get-AzureRmSubscription)
            if ($azureSubscriptions.Length -gt 0) {
                [Console]::WriteLine("Cached Azure profile loaded successfully!")
                $loginSuccessful = $true
            } else {
                [Console]::WriteLine("WARNING: Cached Azure profile couldn't be loaded")
            }
        } catch {
            [Console]::WriteLine("WARNING: Cached Azure profile couldn't be loaded")
            [Console]::WriteLine("EXCEPTION: " + $_.Exception.Message)
        }
    } else {
        [Console]::WriteLine("Azure profile not found at " + $ProfileCachePath)
    }

    if (-not $loginSuccessful) {
        [Console]::WriteLine('Logging in interactively...')
        Login-AzureRmAccount
        [Console]::WriteLine("Saving azure profile to " + $ProfileCachePath)
        Save-AzureRmContext -Path $ProfileCachePath
        [Console]::WriteLine('Done logging in interactively!')
    }
}

$TokenMatches = @{}
$CertificatesCache = @()
$SecretsCache = @()

$KeyVaultSecretRegex = New-Object -TypeName System.Text.RegularExpressions.Regex @('\$\$__(?<SecretKind>[0-9A-Za-z-]+)__https://(?<VaultName>[0-9A-Za-z-]+).vault.azure.net:443/[\w]+/(?<SecretName>[0-9A-Za-z-]+)__(?:(?<SecretProperty>[0-9A-Za-z-]+)__)?\$\$')
$TestSecretPathsFile = "$PSScriptRoot\TestSecretPaths.txt"
$TestSecretPaths = [IO.File]::ReadAllLines($TestSecretPathsFile)

$KeyVaultTokenizedTemplates = @()
ForEach ($TestSecretPath in $TestSecretPaths) {
    $KeyVaultTokenizedTemplates += @(Get-ChildItem -Path $PSScriptRoot\$TestSecretPath\* -Include *.KeyVaultTemplate.* -Recurse)
}

ForEach ($TemplateFileInfo in $KeyVaultTokenizedTemplates) {
    $TemplateFilename = $TemplateFileInfo.FullName
    $TemplateText = [System.IO.File]::ReadAllText($TemplateFilename)
    $ResolvedText = $TemplateText
    $Matches = $KeyVaultSecretRegex.Matches($TemplateText)
    ForEach ($Match in $Matches) {
        if (-not $TokenMatches.ContainsKey($Match.ToString())) {
            $VaultName = $Match.Groups['VaultName'].ToString()
            $SecretKind = $Match.Groups['SecretKind'].ToString()
            $SecretName = $Match.Groups['SecretName'].ToString()
            $SecretProperty = $Match.Groups['SecretProperty'].ToString()

            [Console]::WriteLine('Resolving secret ' + $Match + '...')
            $Secret = Get-AzureKeyVaultSecret -VaultName $VaultName -Name $SecretName
            if ($SecretKind -eq 'Certificate') {
                $CertificateX509 = Import-Certificate($Secret.SecretValueText)
                if ($SecretProperty -eq 'Thumbprint') {
                    $TokenMatches.Add($Match.ToString(), $CertificateX509.Thumbprint)
                } else {
                    throw 'Unknown SecretProperty: ' + $SecretProperty
                }
            } else {
                if ($SecretProperty -eq 'SecretValueText') {
                    $TokenMatches.Add($Match.ToString(), $Secret.SecretValueText)
                } elseif ($SecretProperty -eq 'SecretValueTextHex') {
                    $HexValueText = Convert-Base64ToHex($Secret.SecretValueText)
                    $TokenMatches.Add($Match.ToString(), $HexValueText)
                } else {
                   throw 'Unknown SecretProperty: ' + $SecretProperty
                }
            }
            [Console]::WriteLine('Done resolving secret ' + $Match + '!')
        }

        $ResolvedText = $ResolvedText.Replace($Match.ToString(), $TokenMatches[$Match.ToString()])
    }

    $OutFilename = $TemplateFilename.Replace('KeyVaultTemplate', 'out')
    [System.IO.File]::WriteAllText($OutFilename, $ResolvedText)
}

