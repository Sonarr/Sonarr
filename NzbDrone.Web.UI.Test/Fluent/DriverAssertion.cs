using System.Linq;
using FluentAssertions;
using OpenQA.Selenium.Remote;

namespace NzbDrone.Web.UI.Automation.Fluent
{
    public class DriverAssertion
    {
        private readonly RemoteWebDriver _driver;

        public DriverAssertion(RemoteWebDriver driver)
        {
            _driver = driver;
        }

        public void BeNzbDronePage()
        {
            _driver.Title.Should().EndWith("NzbDrone");
        }
    }
}
