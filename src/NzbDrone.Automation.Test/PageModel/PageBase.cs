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


        public void WaitForNoSpinner(int timeout = 20)
        {
            //give the spinner some time to show up.
            Thread.Sleep(100);

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


        public IWebElement SeriesNavIcon
        {
            get
            {
                return Find(By.LinkText("Series"));
            }
        }

        public IWebElement CalendarNavIcon
        {
            get
            {
                return Find(By.LinkText("Calendar"));
            }
        }

        public IWebElement HistoryNavIcon
        {
            get
            {
                return Find(By.LinkText("History"));
            }
        }

        public IWebElement WantedNavIcon
        {
            get
            {
                return Find(By.LinkText("Wanted"));
            }
        }

        public IWebElement SettingNavIcon
        {
            get
            {
                return Find(By.LinkText("Settings"));
            }
        }

        public IWebElement SystemNavIcon
        {
            get
            {
                return Find(By.LinkText("System"));
            }
        }

    }
}