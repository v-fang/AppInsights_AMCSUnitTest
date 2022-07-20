// Microsoft.Portal.TestFramework.Core.Authentication.PortalAuthentication
#define TRACE
using Microsoft.Portal.TestFramework.Core.Authentication;
using Microsoft.Portal.TestFramework.Core.Shared;
using Microsoft.Portal.TestFramework.Core.Shell;
using Microsoft.Selenium.Utilities;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Web;
using System.Xml;

public class PortalAuthentication
{
    private const string AccountDisambiguationPicker = "login_splitter_control";

    private const string AccountDisambiguationPickerNew = "login-idp-disambiguation-view";

    private const string DisabledExtensionsToEnableSetting = "TestFramework.Portal.DisabledExtensionsToEnable";

    private const string DisambiguationContinueButton = "cred_continue_button";

    private const string PortalLanguageEnvVar = "PortalLanguage";

    private const string LiveIdAccountDisambiguationButton = "mso_account_tile";

    private const string LiveIdAccountDisambiguationButtonNew = "msaTile";

    private const string LiveIdLoginTextBoxCssSelector = "#i0116";

    private const string LiveIdPasswordTextBoxCssSelector = "#i0118";

    private const string LiveIdSignInButtonCssSelector = "#idSIButton9";

    private const string LiveIdUri = "https://login.live.com";

    private const string OrgIdAccountDisambiguationButton = "aad_account_tile";

    private const string OrgIdAccountDisambiguationButtonNew = "aadTile";

    private const string OrgIdLoginTextBoxCssSelector = "#cred_userid_inputtext";

    private const string OrgIdLoginTextBoxId = "cred_userid_inputtext";

    private const string OrgIdPasswordTextBoxCssSelector = "#cred_password_inputtext";

    private const string OrgIdSignInButtonCssSelector = "#cred_sign_in_button";

    private const string KeepMeSignedInNoCssSelector = "#decline_kmsi,#idBtn_Back";

    private const string KeepMeSignedInPageIdContent = "KmsiInterrupt";

    private const string LoginPageQueryParamAppSetting = "TestFramework.Portal.appendQueryParamsToLoginPage";

    private const string LoginPageIsMSAIntAppSetting = "TestFramework.Portal.isMSAInt";

    private const string MsaIntQueryParam = "?msaoauth2=true";

    private IWebDriver webDriver;

    private Uri portalUri;

    private PortalAuthentication()
    {
    }

    public PortalAuthentication(IWebDriver webDriver, Uri portalUri)
    {
        this.webDriver = webDriver;
        this.portalUri = portalUri;
    }

    private IWebElement getNextOrSignInButton()
    {
        return webDriver.WaitUntil(() => ((ISearchContext)webDriver).FindElement(By.CssSelector("#idSIButton9,#cred_sign_in_button")), "Could not find Sign In/Next/Yes button in the sign in page");
    }

    private static bool ElementIsDisplayed(IWebElement element)
    {
        string cssValue = element.GetCssValue("display");
        string cssValue2 = element.GetCssValue("opacity");
        switch (cssValue)
        {
            case "block":
            case "inline-block":
            case "inline":
                if (cssValue2 != "0")
                {
                    return element.Size.Height > 0;
                }
                break;
        }
        return false;
    }

    private void SetUserName(string userName)
    {
        IWebElement obj = webDriver.WaitUntil(() => ((ISearchContext)webDriver).FindElement(By.CssSelector("#i0116,#cred_userid_inputtext")), "Could not find UserName textbox in LiveId sign in page");
        obj.SendKeys(Keys.Control + "a");
        obj.SendKeys(userName + Keys.Tab);
    }

    private void SetPassword(string password, AuthenticationMode? authMode = AuthenticationMode.OrgId)
    {
        AuthenticationMode? disambiguateUserSelection = (!authMode.HasValue) ? new AuthenticationMode?(AuthenticationMode.LiveId) : authMode;
        webDriver.WaitUntil((Func<IWebElement>)delegate
        {
            IWebElement val = ((ISearchContext)(object)webDriver).FindElementOrDefault(By.ClassName("login_splitter_control"));
            if (val == null)
            {
                val = ((ISearchContext)(object)webDriver).FindElementOrDefault(By.CssSelector(string.Format("[data-bind*=\"'{0}'\"]", "login-idp-disambiguation-view")));
                if (val != null && ElementIsDisplayed(val))
                {
                    DisambiguateUser(authMode, val, (disambiguateUserSelection == AuthenticationMode.LiveId) ? "msaTile" : "aadTile");
                }
            }
            else if (ElementIsDisplayed(val))
            {
                DisambiguateUser(authMode, val, (disambiguateUserSelection == AuthenticationMode.LiveId) ? "mso_account_tile" : "aad_account_tile");
            }
            if (authMode == AuthenticationMode.LiveId && !webDriver.Url.StartsWith("https://login.live.com", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            IWebElement val2 = ((ISearchContext)(object)webDriver).FindElementOrDefault(By.CssSelector("#i0118,#cred_password_inputtext"));
            return (val2 != null && ElementIsDisplayed(val2)) ? val2 : null;
        }, "Expected to find the password textbox in the sign in page.").SendKeys(password);
    }

    private void DisambiguateUser(AuthenticationMode? authMode, IWebElement accountPicker, string buttonId)
    {
        ((ISearchContext)webDriver).FindElement(By.Id(buttonId)).Click();
        if (authMode == AuthenticationMode.LiveId)
        {
            webDriver.WaitUntil(() => webDriver.Url.StartsWith("https://login.live.com", StringComparison.OrdinalIgnoreCase), "Browser did not redirect to the LiveID page");
        }
    }

    private void ClickNextButton()
    {
        //IL_0013: Unknown result type (might be due to invalid IL or missing references)
        IWebElement nextOrSignInButton = getNextOrSignInButton();
        nextOrSignInButton.Click();
    }

    private void ClickSigninButton()
    {
        IWebElement nextOrSignInButton = getNextOrSignInButton();
        ClickAndWaitForRedirect(nextOrSignInButton);
    }

    private void ClickAndWaitForRedirect(IWebElement element)
    {
        //IL_0018: Unknown result type (might be due to invalid IL or missing references)
        string url = webDriver.Url;
        int num = 30;
        while (num > 0)
        {
            num--;
            try
            {
                if (webDriver.Url != url)
                {
                    return;
                }
                element.Click();
            }
            catch (Exception)
            {
            }
            Thread.Sleep(2000);
        }
        throw new WebDriverException("Failed to click button and redirect");
    }

    private void SetUserNameAndPasswordAndClickOk(string userName, string password, AuthenticationMode? authMode, bool signedInOnce = false, bool skipPortalLoadValidation = false)
    {
        if (signedInOnce)
        {
            try
            {
                webDriver.WaitUntil(() => ((ISearchContext)webDriver).FindElement(By.Id("otherTile")), "Could not find 'Use another account' link").Click();
            }
            catch (WebDriverException)
            {
                Trace.TraceWarning("Expected to see the 'Use another account'. Continuing to login.");
            }
        }
        bool num = webDriver.WaitUntil(() => ((ISearchContext)webDriver).FindElement(By.CssSelector("#i0116,#cred_userid_inputtext")), "Could not find UserName textbox in LiveId sign in page").GetAttribute("id").Equals("cred_userid_inputtext");
        SetUserName(userName);
        if (num)
        {
            if (authMode == AuthenticationMode.LiveId)
            {
                webDriver.WaitUntil(() => webDriver.Url.StartsWith("https://login.live.com", StringComparison.OrdinalIgnoreCase), string.Format("Expected browser to be redirected to {0}. Current url: {1}", "https://login.live.com", webDriver.Url));
            }
            else if (!authMode.HasValue)
            {
                Thread.Sleep(3000);
            }
        }
        if (!num)
        {
            ClickNextButton();
        }
    }

    private void PrepareWebDriverForSignIn(string path, string deepLink, string extensionsToEnable, Dictionary<string, string> featuresToEnable)
    {
        UriBuilder uriBuilder = new UriBuilder(new Uri(portalUri.AbsoluteUri));
        if (!string.IsNullOrWhiteSpace(path))
        {
            uriBuilder.Path = path;
        }
        NameValueCollection nameValueCollection = HttpUtility.ParseQueryString(uriBuilder.Query);
        SetCommonValues(nameValueCollection);
        if (!string.IsNullOrWhiteSpace(extensionsToEnable))
        {
            Trace.TraceInformation("Enabling these disabled extensions: {0}...", extensionsToEnable);
            string[] array = extensionsToEnable.Split(',');
            foreach (string text in array)
            {
                nameValueCollection[text.Trim()] = "true";
            }
        }
        if (featuresToEnable != null)
        {
            foreach (KeyValuePair<string, string> item in featuresToEnable)
            {
                nameValueCollection[item.Key] = item.Value;
            }
        }
        uriBuilder.Query = nameValueCollection.ToString();
        webDriver.Url = string.IsNullOrWhiteSpace(deepLink) ? uriBuilder.Uri.AbsoluteUri : deepLink;
    }

    private void PrepareWebDriverForSignIn2(string path, string query, string fragment)
    {
        UriBuilder uriBuilder = new UriBuilder(new Uri(portalUri.AbsoluteUri));
        uriBuilder.Path = "signin/index/{0}".FormatInvariant(path ?? string.Empty);
        if (!string.IsNullOrWhiteSpace(fragment))
        {
            uriBuilder.Fragment = fragment;
        }
        NameValueCollection nameValueCollection = HttpUtility.ParseQueryString(uriBuilder.Query);
        SetCommonValues(nameValueCollection);
        NameValueCollection nameValueCollection2 = HttpUtility.ParseQueryString(query ?? string.Empty);
        foreach (string item in nameValueCollection2)
        {
            nameValueCollection[item] = nameValueCollection2[item];
        }
        uriBuilder.Query = nameValueCollection.ToString();
        webDriver.Navigate().GoToUrl(uriBuilder.Uri.AbsoluteUri);
        ReloadLoginPageWithAdditionalQueryParams();
    }

    private void ReloadLoginPageWithAdditionalQueryParams()
    {
        string text = AppendAdditionalLoginQueryParms(webDriver.Url);
        if (webDriver.Url != text)
        {
            webDriver.Navigate().GoToUrl(text);
        }
    }

    private string AppendAdditionalLoginQueryParms(string inputUrl)
    {
        string result = inputUrl;
        string text = ConfigurationManager.AppSettings["TestFramework.Portal.appendQueryParamsToLoginPage"] ?? string.Empty;
        bool result2 = false;
        bool.TryParse(ConfigurationManager.AppSettings["TestFramework.Portal.isMSAInt"], out result2);
        if (result2)
        {
            text = text.AppendQueryParam("?msaoauth2=true");
        }
        if (!string.IsNullOrEmpty(text))
        {
            UriBuilder uriBuilder = new UriBuilder(inputUrl);
            uriBuilder.Query = uriBuilder.Query.AppendQueryParam(text);
            result = uriBuilder.Uri.AbsoluteUri;
        }
        return result;
    }

    private void SetCommonValues(NameValueCollection query)
    {
        string environmentVariable = Environment.GetEnvironmentVariable("PortalLanguage");
        if (!string.IsNullOrWhiteSpace(environmentVariable))
        {
            query["l"] = environmentVariable;
        }
        query["sessionId"] = "TestTraffic";
        query["trace"] = "debugLog";
    }

    private Portal DoPostSignInSetup()
    {
        webDriver.WaitUntil(() => webDriver.Url.StartsWith(portalUri.AbsoluteUri, StringComparison.OrdinalIgnoreCase), $"Sign in failed. The browser's url did not start with the given url {portalUri.AbsoluteUri}.");
        Portal.EnableJavaScriptConsoleInterception(webDriver);
        Portal portal = Portal.FindPortal(webDriver, checkIfPartsExist: false);
        portal.EnableAnimations(enabled: false);
        Size size = webDriver.Manage().Window.Size;
        if (size.Height < 850)
        {
            webDriver.Manage().Window.Size = new Size(size.Width, 850);
        }
        return portal;
    }

    public Portal SignIn(string userName, string password, string tenantDomainName, AuthenticationMode mode, string deepLink = null, string extensionsToEnable = null, Dictionary<string, string> featuresToEnable = null)
    {
        try
        {
            webDriver.Url = "https://login.live.com";
            webDriver.Manage().Cookies.AddCookie(new Cookie("__Host-MSAAUTHP", GetCookie("AIVendorCookie")));
            PrepareWebDriverForSignIn(tenantDomainName, deepLink, extensionsToEnable, featuresToEnable);
            SetUserNameAndPasswordAndClickOk(userName, password, mode);
            return DoPostSignInSetup();
        }
        catch (UnhandledAlertException)
        {
            IAlert val = webDriver.SwitchTo().Alert();
            Trace.TraceInformation("Dismissing unexpected alert.  Alert text: {0}", val.Text);
            val.Dismiss();
            Trace.TraceInformation("Rethrow the initial exception as this means something has gone wrong either in a previous test, or a previous step of this test.");
            throw;
        }
    }
    public string GetCookie(string name)
    {
        XmlDocument doc = new XmlDocument();
        doc.Load(Path.Combine(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(this.GetType()).Location), "TestEnvironmentConfiguration.out.xml")));
        XmlNodeList settingNode = doc.SelectNodes(string.Format("//*[@name='{0}']", name));
        return settingNode[0].Attributes["value"].InnerText;
    }
    public Portal SignIn(string userName, string password, string tenantDomainName, string query = null, string fragment = null, bool signedInOnce = false)
    {
        SignInAndSkipPostValidation(userName, password, tenantDomainName, query, fragment, signedInOnce);
        return DoPostSignInSetup();
    }

    public void SignInAndSkipPostValidation(string userName, string password, string tenantDomainName, string query = null, string fragment = null, bool signedInOnce = false)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new ArgumentException("username was not specified");
        }
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("password was not specified");
        }
        try
        {
            PrepareWebDriverForSignIn2(tenantDomainName, query, fragment);
            SetUserNameAndPasswordAndClickOk(userName, password, null, signedInOnce, skipPortalLoadValidation: true);
        }
        catch (UnhandledAlertException)
        {
            IAlert val = webDriver.SwitchTo().Alert();
            Trace.TraceInformation("Dismissing unexpected alert.  Alert text: {0}", val.Text);
            val.Dismiss();
            Trace.TraceInformation("Rethrow the initial exception as this means something has gone wrong either in a previous test, or a previous step of this test.");
            throw;
        }
    }
}
