using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Portal.TestFramework.Core.Controls;
using Microsoft.Portal.TestFramework.Core.Shell;
using Microsoft.Selenium.Utilities;
using OpenQA.Selenium;

namespace AppInsights_AMCSUnitTest.Framework.Elements
{
    public class DCROverviewBlade : MonitorBlade
    {
        /// <summary>
        /// Get and load the blade with the blade title
        /// </summary>
        /// <param name="bladeTitle"></param>
        /// <returns></returns>
        public static new DCROverviewBlade GetBladeAndValidateComponentsLoaded(string bladeTitle)
        {
            DCROverviewBlade blade = AMCSTestBase.portal.FindSingleBladeByTitle<DCROverviewBlade>(bladeTitle);
            blade.ValidateBlade();
            return blade;
        }

        public OverviewProperty FindOverviewProperty(string label)
        {
            return WebDriver.FindElements<OverviewProperty>().First(t => t.PropertyName == label);
        }

        public IWebElement ClickHereToAddTags
        {
            get
            {
                return AMCSTestBase.driver.WaitUntil(() =>
                {
                    return FindOverviewProperty(ClientResources.TagsLabel).FindElement(By.CssSelector(".fxc-essentials-notags-button"));
                }, "Cannot find 'Click here to add tags' text.", Constants.DefaultTimeSpan);
            }
        }
    }

    public class OverviewProperty : BaseElement
    {
        public override By Locator
        {
            get
            {
                return By.CssSelector(".fxc-essentials-item");
            }
        }

        public string PropertyName
        {
            get
            {
                return FindElement(By.TagName("label")).Text;
            }
        }
        public IWebElement ChangeButton
        {
            get
            {
                return FindElement(By.CssSelector("button[role='link']"));
            }
        }

        public IWebElement ValueElement
        {
            get
            {
                return FindElement(By.CssSelector(".fxc-essentials-value"));
            }
        }
    }
}
