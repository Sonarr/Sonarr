using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Automation.Test.PageModel;
using OpenQA.Selenium;

namespace NzbDrone.Automation.Test
{
    [TestFixture]
    public class WantedPageTest : AutomationTest
    {
        private PageBase page;

        [SetUp]
        public void Setup()
        {
            page = new PageBase(driver);
        }

        [Test]
        public void hide_drone_update_by_default()
        {
            page.WantedNavIcon.Click();
            page.WaitForNoSpinner();

            Assert.Throws<NoSuchElementException>(() => page.Displayed(By.ClassName("icon-sonarr-refresh")));
        }

        [Test]
        public void show_drone_update_if_drone_folder_set()
        {
            page.SettingNavIcon.Click();
            page.WaitForNoSpinner();

            page.FindByClass("x-download-client-tab").Click();
            page.FindByClass("x-advanced-settings").Click();
            page.Find(By.Name("downloadedEpisodesFolder")).SendKeys("C:\\");
            page.FindByClass("x-save-settings").Click();

            page.WantedNavIcon.Click();
            page.WaitForNoSpinner();

            page.FindByClass("icon-sonarr-refresh").Should().NotBeNull();
        }
    }
}
