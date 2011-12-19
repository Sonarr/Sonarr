using System.Linq;
using OpenQA.Selenium.Remote;

namespace NzbDrone.Web.UI.Automation.Fluent
{
    public static class AssertionExtention
    {
        public static DriverAssertion Should(this RemoteWebDriver actualValue)
        {
            return new DriverAssertion(actualValue);
        }
    }
}