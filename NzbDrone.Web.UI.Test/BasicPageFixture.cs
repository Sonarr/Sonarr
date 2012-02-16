using System.Linq;
using NUnit.Framework;
using NzbDrone.Web.UI.Automation.Fluent;

namespace NzbDrone.Web.UI.Automation
{
    public class BasicPageFixture : AutomationTestBase
    {
        [Test]
        public void HomePage()
        {
            Driver.GivenHomePage();
            CaptureScreen();
            Driver.Should().BeNzbDronePage();
        }

        [Test]
        public void HistoryPage()
        {
            Driver.GivenHistoryPage();
            CaptureScreen();
            Driver.Should().BeNzbDronePage();
        }

        [Test]
        public void MissingPage()
        {
            Driver.GivenMissingPage();
            CaptureScreen();
            Driver.Should().BeNzbDronePage();
        }

        [Test]
        public void SettingsPage()
        {
            Driver.GivenSettingsPage();
            CaptureScreen();
            Driver.Should().BeNzbDronePage();
        }

    }
}
