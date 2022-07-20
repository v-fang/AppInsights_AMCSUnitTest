using System.Configuration;

namespace AppInsights_AMCSUnitTest.Framework
{
    /// <summary>
    /// This class representing appSettings sections in app.config.
    /// </summary>
    public class ConfigurationSettings
    {
        public string Environment { get; private set; }
        public string TenantId { get; private set; }
        public string PortalUrl { get; private set; }
        public string TestAccount { get; private set; }
        public string Subscription { get; private set; }
        public string SubscriptionId { get; private set; }
        public string ResourceGroup { get; private set; }

        public ConfigurationSettings()
        {
            Environment = GetConfigurationOrDefault("Environment", "MPAC");
            TenantId = GetConfigurationOrDefault("TenantId", "72f988bf-86f1-41af-91ab-2d7cd011db47");
            PortalUrl = GetConfigurationOrDefault("TestFramework.Portal.Uri", "https://ms.portal.azure.com/");
            TestAccount = GetConfigurationOrDefault("TestAccount", "AIvendor@outlook.com");
            Subscription = GetConfigurationOrDefault("Subscription", "AME_TestVendor_TestAutomation_RnD");
            SubscriptionId = GetConfigurationOrDefault("SubscriptionId", "29f76b8c-2c76-45eb-b392-cde6dc538b31");
            ResourceGroup = GetConfigurationOrDefault("ResourceGroup", "AI_v-fanfa_04082020_AIUIAutomation");
        }

        private static string GetConfigurationOrDefault(string key, string defaultValue)
        {
            string config = ConfigurationManager.AppSettings[key];
            if (config == null || config.Trim() == string.Empty)
            {
                return defaultValue;
            }
            else
            {
                return config;
            }
        }
    }
}
