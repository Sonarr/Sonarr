using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NLog;
using NLog.Config;
using NLog.Targets;
using NUnit.Framework;
using NzbDrone.Automation.Test.PageModel;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Test.Common;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;

namespace NzbDrone.Automation.Test
{
    [TestFixture]
    [AutomationTest]
    public abstract class AutomationTest
    {
        private NzbDroneRunner _runner;
        protected RemoteWebDriver driver;

        public AutomationTest()
        {
            new StartupContext();

            LogManager.Configuration = new LoggingConfiguration();
            var consoleTarget = new ConsoleTarget { Layout = "${level}: ${message} ${exception}" };
            LogManager.Configuration.AddTarget(consoleTarget.GetType().Name, consoleTarget);
            LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", NLog.LogLevel.Trace, consoleTarget));
        }

        [OneTimeSetUp]
        public void SmokeTestSetup()
        {
            var options = new ChromeOptions();
            options.AddArguments("--headless");
            var service = ChromeDriverService.CreateDefaultService();

            // Timeout as windows automation tests seem to take alot longer to get going
            driver = new ChromeDriver(service, options, new TimeSpan(0, 3, 0));

            driver.Manage().Window.Size = new System.Drawing.Size(1920, 1080);

            _runner = new NzbDroneRunner(LogManager.GetCurrentClassLogger());
            _runner.KillAll();
            _runner.Start();

            driver.Url = "http://localhost:8989";

            var page = new PageBase(driver);
            page.WaitForNoSpinner();

            driver.ExecuteScript("window.Sonarr.NameViews = true;");

            GetPageErrors().Should().BeEmpty();
        }

        protected IEnumerable<string> GetPageErrors()
        {
            return driver.FindElements(By.CssSelector("#errors div"))
                .Select(e => e.Text);
        }

        protected void TakeScreenshot(string name)
        {
            try
            {
                Screenshot image = ((ITakesScreenshot)driver).GetScreenshot();
                image.SaveAsFile($"./{name}_test_screenshot.png", ScreenshotImageFormat.Png);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save screenshot {name}, {ex.Message}");
            }
        }

        [OneTimeTearDown]
        public void SmokeTestTearDown()
        {
            _runner.KillAll();
            driver.Quit();
        }

        [TearDown]
        public void AutomationTearDown()
        {
            GetPageErrors().Should().BeEmpty();
        }
    }
}
