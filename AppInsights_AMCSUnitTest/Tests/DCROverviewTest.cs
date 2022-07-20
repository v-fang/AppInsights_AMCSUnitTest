using AppInsights_AMCSUnitTest.Framework;
using AppInsights_AMCSUnitTest.Framework.Elements;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppInsights_AMCSUnitTest.Tests
{
    [TestClass]
    public class DCROverviewTest : AMCSTestBase
    {
        private readonly string DCR_bladeTitle = "Monitor" + " | " + Constants.DCRFullName;
        private MonitorBlade overviewBlade;
        private MonitorDCRBlade monitorDCRBlade;

        [TestInitialize]
        public void OpenMonitorOverviewBlade()
        {
            LogInfo("1.Open Monitor | Data Collection Rules blade");
            overviewBlade = MonitorBlade.OpenMonitorOverviewBladeWithDeepLink(driver, portal);
            overviewBlade.OpenBladeFromSideBarByName(Constants.DCRFullName);
            monitorDCRBlade = MonitorDCRBlade.GetBladeAndValidateComponentsLoaded(DCR_bladeTitle);
        }

        [TestMethod]
        [TestArea(Areas.DCROverview)]
        [TestOwner(Owners.Fang)]
        [TestCategory(Constants.DCRValidation)]
        [Priority(1)]
        public void OverviewCorrectInformation()
        {
            LogInfo("2.Open DCR resource blade.");
            monitorDCRBlade.FindElement(By.LinkText(ClientResources.DCRResourceName)).Click();
            DCROverviewBlade dcrOverviewBlade = DCROverviewBlade.GetBladeAndValidateComponentsLoaded(ClientResources.DCRResourceName);
            Assert.AreEqual(TestConfig.ResourceGroup, dcrOverviewBlade.FindOverviewProperty(ClientResources.ResourceGroupLabel).ValueElement.Text);

            LogInfo("3.Verify Resource Group Label works properly.");
            dcrOverviewBlade.FindOverviewProperty(ClientResources.ResourceGroupLabel).ChangeButton.Click();
            MonitorBlade blade = MonitorBlade.GetBladeAndValidateComponentsLoaded(ClientResources.MoveResources);
            blade.Close();
            dcrOverviewBlade.FindOverviewProperty(ClientResources.ResourceGroupLabel).ValueElement.Click();
            blade = MonitorBlade.GetBladeAndValidateComponentsLoaded(TestConfig.ResourceGroup);
            blade.Close();

            LogInfo("4.Verify Subscription Label works properly.");
            Assert.AreEqual(TestConfig.Subscription, dcrOverviewBlade.FindOverviewProperty(ClientResources.SubscriptionLabel).ValueElement.Text);
            dcrOverviewBlade.FindOverviewProperty(ClientResources.SubscriptionLabel).ChangeButton.Click();
            blade = MonitorBlade.GetBladeAndValidateComponentsLoaded(ClientResources.MoveResources);
            blade.Close();
            dcrOverviewBlade.FindOverviewProperty(ClientResources.SubscriptionLabel).ValueElement.Click();
            blade = MonitorBlade.GetBladeAndValidateComponentsLoaded(TestConfig.Subscription);
            blade.Close();

            LogInfo("5.Verify Tags Label works properly.");
            Assert.AreEqual("Click here to add tags", dcrOverviewBlade.ClickHereToAddTags.Text);
            dcrOverviewBlade.FindOverviewProperty(ClientResources.TagsLabel).ChangeButton.Click();
            blade = MonitorBlade.GetBladeAndValidateComponentsLoaded(ClientResources.EditTags);
            blade.Close();
            dcrOverviewBlade.ClickHereToAddTags.Click();
            blade = MonitorBlade.GetBladeAndValidateComponentsLoaded(ClientResources.EditTags);
            blade.Close();
        }

        [TestCleanup]
        public void Cleanup()
        {
            LogInfo("Clean up.");
            this.CaptureLogsAndScreenshot();
        }
    }
    
}
