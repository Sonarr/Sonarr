using System.Linq;
using OpenQA.Selenium.Remote;

namespace NzbDrone.Web.UI.Automation.Fluent
{
    public static class NavigationExtention
    {

        private const string baseUrl = "http://localhost:8989/";

        public static RemoteWebDriver GivenHomePage(this RemoteWebDriver driver)
        {
            driver.Navigate().GoToUrl(baseUrl);
            return driver;
        }

        public static RemoteWebDriver GivenSettingsPage(this RemoteWebDriver driver)
        {
            driver.Navigate().GoToUrl(baseUrl + "settings");
            return driver;
        }

        public static RemoteWebDriver GivenUpcomingPage(this RemoteWebDriver driver)
        {
            driver.Navigate().GoToUrl(baseUrl + "Upcoming");
            return driver;
        }

        public static RemoteWebDriver GivenHistoryPage(this RemoteWebDriver driver)
        {
            driver.Navigate().GoToUrl(baseUrl + "History");
            return driver;
        }

        public static RemoteWebDriver GivenMissingPage(this RemoteWebDriver driver)
        {
            driver.Navigate().GoToUrl(baseUrl + "Missing");
            return driver;
        }
    }
}
