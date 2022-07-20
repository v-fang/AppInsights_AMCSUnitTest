using Microsoft.Portal.TestFramework.Core.Shell;
using Microsoft.Selenium.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AppInsights_AMCSUnitTest.Framework
{
    [TestClass]
    public abstract class TestBase
    {
        private Stopwatch testTimer = new Stopwatch();
        public TestContext TestContext { get; set; }
        public Owners TestOwner { get; set; }
        public Areas TestArea { get; set; }
        public String[] TestCategories { get; set; }
        public static String TestRunID { get; set; }
        public ConfigurationSettings TestConfig { get; private set; }

        [TestInitialize]
        public void TestBaseInitialize()
        {
            TestOwnerAttribute testOwner = GetTestMethodAttribute<TestOwnerAttribute>();
            if (testOwner == null)
            {
                throw new Exception("Please specify test owner: [TestOwner(Owners.xxx)].");
            }
            this.TestOwner = testOwner.Owner;

            TestAreaAttribute testArea = GetTestMethodAttribute<TestAreaAttribute>();
            if (testArea == null)
            {
                throw new Exception("Please specify test area:[TestArea(Areas.xxx)].");
            }
            this.TestArea = testArea.Area;

            List<TestCategoryAttribute> testCategories = GetTestMethodAttributes<TestCategoryAttribute>();
            if (testCategories == null || testCategories.Count() <= 0)
            {
                throw new Exception("Please specify test category:[TestCategory(Constants.DeploymentValidation)].");
            }

            List<string> categories = new List<string>();
            foreach (TestCategoryAttribute category in testCategories)
            {
                categories.AddRange(category.TestCategories);
            }
            // Sort the test categories that we won't get to many unique value for telemetry. 
            this.TestCategories = categories.OrderByDescending(a => a).ToArray();

            if (String.IsNullOrEmpty(TestBase.TestRunID))
            {
                TestBase.TestRunID = Guid.NewGuid().ToString();
            }

            if (this.TestConfig == null)
            {
                this.TestConfig = new ConfigurationSettings();
            }

            TestContext.WriteLine("Test Owner: " + this.TestOwner.ToString());
            TestContext.WriteLine("Test Area: " + this.TestArea.ToString());
            TestContext.WriteLine("Test Categories: " + string.Join(",", this.TestCategories));
            TestContext.WriteLine("Test Run ID: " + TestBase.TestRunID);
            TestContext.WriteLine("Test Environment: " + this.TestConfig.Environment);
            TestContext.WriteLine("Test PortalUrl: " + this.TestConfig.PortalUrl);
            TestContext.WriteLine("Test Account: " + this.TestConfig.TestAccount);
            testTimer.Start();
        }

        [TestCleanup]
        public void TestBaseCleanup()
        {
        }

        internal T GetTestMethodAttribute<T>() where T : Attribute
        {
            var attrs = this.GetTestMethodAttributes<T>();
            return attrs.Count == 0 ? null : attrs[0];
        }

        internal List<T> GetTestMethodAttributes<T>() where T : Attribute
        {
            List<T> attributes = new List<T>();

            Type attributeType = typeof(T);
            MethodInfo methodInfo = this.GetType().GetMethod(this.TestContext.TestName);
            attributes.AddRange(methodInfo.GetCustomAttributes(attributeType, true).OfType<T>());

            return attributes;
        }

        public static class BladeHelper
        {
            public static T GetBlade<T>(string bladeName) where T : Blade, new()
            {
                return AMCSTestBase.driver.WaitUntil<T>(() =>
                {
                    if (AMCSTestBase.portal.FindElements<T>().Where(t => (t.Title.Contains(bladeName)) && (t.IsLoading == false)).Count() > 0)
                    {
                        return AMCSTestBase.portal.FindElements<T>().Where(t => (t.Title.Contains(bladeName)) && (t.IsLoading == false)).First();
                    }
                    else
                    {
                        return null;
                    }
                }, String.Format("Can't get a blade named {0}", bladeName), TimeSpan.FromMinutes(1));
            }
        }

        public void LogInfo(string info, params object[] args)
        {
            TestContext.WriteLine(info, args);
        }
    }
}
