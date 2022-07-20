using Microsoft.Portal.TestFramework.Core.Controls;
using Microsoft.Portal.TestFramework.Core.Controls.Lists;
using Microsoft.Portal.TestFramework.Core.Shell;
using Microsoft.Selenium.Utilities;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace AppInsights_AMCSUnitTest.Framework.Elements
{
    public class MonitorBlade : Blade
    {
        public static MonitorBlade OpenMonitorOverviewBladeWithDeepLink(IWebDriver driver, Portal portal)
        {
            portal.StartBoard.WaitUntilAllPartsAreLoaded();
            string fragment = "blade/Microsoft_Azure_Monitoring/AzureMonitoringBrowseBlade/overview";
            portal.GoToDeepLink(driver, fragment);

            string OverviewbladeTitle = "Monitor | Overview";
            MonitorBlade overviewBlade = portal.FindSingleBladeByTitle<MonitorBlade>(OverviewbladeTitle);
            overviewBlade.WaitUntilAllPartsAreLoaded();
            return overviewBlade;
        }

        public void OpenBladeFromSideBarByName(string ItemName)
        {
            try
            {
                this.FindElement<ListView>().GetListViewItem(ItemName).Click();
            }
            catch
            {
                throw new NoSuchElementException(string.Format("Can't find an item in side bar named {0}", ItemName));
            }
        }

        public IEnumerable<Grid> Grids
        {
            get { return this.FindElements<Grid>(); }
        }

        /// <summary>
        ///     Validate that the grid and all of the rows loaded succefully within the blade  
        /// </summary>
        /// <param name="blade"> the parent blade of the grid</param>
        public void WaitForGridsToLoad()
        {
            Thread.Sleep(3000);
            WebDriver.WaitUntil(
                () => this.Grids != null && this.Grids.All((grid) => (grid.Rows.Length != 0 || grid.Text.IndexOf("No results") != -1 || grid.Text.IndexOf("No data") != -1 || grid.Text == "") || grid.Text.IndexOf("No results to display") != -1 || grid.Text.IndexOf("No data") != -1 || grid.Text.IndexOf("No alerts fired in the selected time period") != -1),
                "Not all " + this.Title + " blade grids loaded");
        }

        public static MonitorBlade GetBladeAndValidateComponentsLoaded(string bladeTitle)
        {
            MonitorBlade blade = AMCSTestBase.driver.FindElement<Portal>().FindSingleBladeByTitle<MonitorBlade>(bladeTitle);
            blade.ValidateBlade();
            return blade;
        }

        public void ValidateBlade()
        {
            this.WaitForGridsToLoad();
            this.WaitBladeNoProgress();
        }

        public void WaitBladeNoProgress()
        {
            IWebElement element = this.FindElement(By.CssSelector(".fxs-progress.fxs-blade-progress"));
            WebDriver.WaitUntil(() => element.HasClass("fxs-display-none"), "Blade also has progress.");
        }
    }
}
