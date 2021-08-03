using System;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;

namespace NzbDrone.Automation.Test.PageModel
{
    public class PageBase
    {
        private readonly RemoteWebDriver _driver;

        public PageBase(RemoteWebDriver driver)
        {
            _driver = driver;
            driver.Manage().Window.Maximize();
        }

        public IWebElement FindByClass(string className, int timeout = 5)
        {
            return Find(By.ClassName(className), timeout);
        }

        public IWebElement Find(By by, int timeout = 5)
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeout));
            return wait.Until(d => d.FindElement(by));
        }

        public void WaitForNoSpinner(int timeout = 30)
        {
            //give the spinner some time to show up.
            Thread.Sleep(200);

            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeout));
            wait.Until(d =>
            {
                try
                {
                    IWebElement element = d.FindElement(By.Id("followingBalls"));
                    return !element.Displayed;
                }
                catch (NoSuchElementException)
                {
                    return true;
                }
            });
        }

        public IWebElement SeriesNavIcon => Find(By.LinkText("Series"));

        public IWebElement CalendarNavIcon => Find(By.LinkText("Calendar"));

        public IWebElement ActivityNavIcon => Find(By.LinkText("Activity"));

        public IWebElement WantedNavIcon => Find(By.LinkText("Wanted"));

        public IWebElement SettingNavIcon => Find(By.LinkText("Settings"));

        public IWebElement SystemNavIcon => Find(By.PartialLinkText("System"));
    }
}
