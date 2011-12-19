using System.Linq;
using NUnit.Framework;
using NzbDrone.Web.UI.Automation.Fluent;

namespace NzbDrone.Web.UI.Automation
{
    [TestFixture]
    public class BasicPageFixture : AutomationTestBase
    {

        [Test]
        public void HomePage()
        {
            Driver.GivenHomePage().Should().BeNzbDronePage();
            CaptureScreen();
        }

        [Test]
        public void HistoryPage()
        {
            Driver.GivenHistoryPage().Should().BeNzbDronePage();
            CaptureScreen();
        }

        [Test]
        public void MissingPage()
        {
            Driver.GivenMissingPage().Should().BeNzbDronePage();
            CaptureScreen();
        }

        [Test]
        public void SettingsPage()
        {
            Driver.GivenSettingsPage().Should().BeNzbDronePage();
            CaptureScreen();
        }

    }



}
