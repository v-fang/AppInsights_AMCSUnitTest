using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using Microsoft.Portal.TestFramework.Core.Shell;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Selenium.Utilities;
using Microsoft.Portal.TestFramework.Core.Authentication;
using System.Xml;
using System.IO;
using System.Threading;
using System.Diagnostics;

[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]
namespace AppInsights_AMCSUnitTest.Framework
{
    [TestClass]
    public class AMCSTestBase : TestBase
    {
        public static IWebDriverFactory factory;
        public static ThreadLocal<RemoteWebDriver> Driver = new ThreadLocal<RemoteWebDriver>(() => factory.Create());
        public static ThreadLocal<Portal> testPortal = new ThreadLocal<Portal>();
        public static List<RemoteWebDriver> webDrivers = new List<RemoteWebDriver>();
        public static RemoteWebDriver driver
        {
            get
            {
                return Driver.Value;
            }
        }
        public static Portal portal
        {
            get
            {
                return testPortal.Value;
            }
            set
            {
                testPortal.Value = value;
            }
        }

        [TestInitialize]
        public void AppInsightsTestBaseInitialize()
        {
            //Only need to sign in once
            if (AMCSTestBase.portal == null)
            {
                AMCSTestBase.factory = new ChromeDriverFactory();
                this.SignIn(this.TestConfig.TestAccount);
                webDrivers.Add(driver);
            }
            else
            {
                try
                {
                    IAlert alert = Driver.Value.SwitchTo().Alert();
                    alert.Accept();
                }
                catch (NoAlertPresentException) { }
                finally
                {
                    //portal.OpenDashboard(driver);
                }
            }
        }

        [TestCleanup]
        public void AMCSTestBaseCleanup()
        {
            CaptureLogsAndScreenshot();
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            factory.CleanAllDriver(webDrivers);
        }

        /// <summary>
        /// Signs account in.
        /// </summary>
        /// <param name="email"></param>
        public void SignIn(string email)
        {
            var liveIdDomains = new string[] { "live.com", "outlook.com", "hotmail.com" };
            var password = GetPassword();

            if (!string.IsNullOrEmpty(password))
            {
                var authMode = liveIdDomains.Any(domain => email.Contains(domain)) ? AuthenticationMode.LiveId : AuthenticationMode.OrgId;
                PortalAuthentication auth = new PortalAuthentication(driver, new Uri(this.TestConfig.PortalUrl));

                portal = driver.WaitUntil<Portal>(() =>
                {
                    Portal p1 = null;
                    try
                    {
                        p1 = auth.SignIn(email, password, TestConfig.TenantId, authMode, TestConfig.PortalUrl + TestConfig.TenantId);
                    }
                    catch (Exception e)
                    {
                        // re-try
                        TestContext.WriteLine("Sign in throw exception: {0}, \r\n stack:\r\n {1}\r\n InnerException: {2}\r\n", e.Message, e.StackTrace, e.InnerException);
                    }

                    return p1;

                }, "Failed to sign in.", Constants.DefaultTimeSpan);
            }
            else
            {
                throw new Exception("Failed to get password due to Key vault issue");
            }

        }

        public string GetPassword()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(Path.Combine(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(this.GetType()).Location), "TestEnvironmentConfiguration.out.xml")));
            XmlNodeList settingNode = doc.GetElementsByTagName("Setting");
            return settingNode[0].Attributes["value"].InnerText;
        }

        public void CaptureLogsAndScreenshot()
        {
            try
            {
                if (TestContext.CurrentTestOutcome != UnitTestOutcome.Passed && driver != null)
                {
                    ITakesScreenshot screenshotDriver = driver;
                    Screenshot screenshot = screenshotDriver.GetScreenshot();
                    var fileName = TestContext.TestRunResultsDirectory + "/" + TestContext.TestName + "-" + $"{DateTime.UtcNow:yyyyMMddTHHmmssZ}" + ".png";
                    screenshot.SaveAsFile(fileName, ScreenshotImageFormat.Png);
                    TestContext.AddResultFile(fileName);
                }
                var logs = Portal.GetJavaScriptConsoleLogs(driver).ToList();
                if (logs.Any())
                {
                    TestContext.WriteLine(Environment.NewLine + "JavaScript Console Logs:");
                    logs.ForEach(log => TestContext.WriteLine(log.Replace("{", "{{").Replace("}", "}}")));
                }
            }
            catch
            {
                TestContext.WriteLine("Could not capture logs/screenshots");
            }
        }
    }
    public interface IWebDriverFactory
    {
        RemoteWebDriver Create();
        void CleanAllDriver(List<RemoteWebDriver> webDrivers);
    }

    public class ChromeDriverFactory : IWebDriverFactory
    {
        private readonly ChromeOptions options;

        public ChromeDriverFactory()
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments(new string[]{
                    "--window-size=1920,1080",
                    "--headless",
                    //"start-maximized",
                    "--no-sandbox",
                    "--disable-gpu",
                    "--disable-extensions"
                    //"user-data-dir=" + System.Environment.GetEnvironmentVariable("LOCALAPPDATA") + "/Google/Chrome/User Data"
            });

            this.options = chromeOptions;
        }

        public RemoteWebDriver Create()
        {
            return new ChromeDriver(options);
        }

        public void CleanAllDriver(List<RemoteWebDriver> webDrivers)
        {
            foreach (RemoteWebDriver driver in webDrivers)
            {
                driver.Quit();
                driver.Dispose();
            }
        }
    }
}
