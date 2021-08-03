using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Automation.Test.PageModel;
using OpenQA.Selenium;

namespace NzbDrone.Automation.Test
{
    [TestFixture]
    public class MainPagesTest : AutomationTest
    {
        private PageBase _page;

        [SetUp]
        public void Setup()
        {
            _page = new PageBase(driver);
        }

        [Test]
        public void series_page()
        {
            _page.SeriesNavIcon.Click();
            _page.WaitForNoSpinner();

            var imageName = MethodBase.GetCurrentMethod().Name;
            TakeScreenshot(imageName);

            _page.Find(By.CssSelector("div[class*='SeriesIndex']")).Should().NotBeNull();
        }

        [Test]
        public void calendar_page()
        {
            _page.CalendarNavIcon.Click();
            _page.WaitForNoSpinner();

            var imageName = MethodBase.GetCurrentMethod().Name;
            TakeScreenshot(imageName);

            _page.Find(By.CssSelector("div[class*='CalendarPage']")).Should().NotBeNull();
        }

        [Test]
        public void activity_page()
        {
            _page.ActivityNavIcon.Click();
            _page.WaitForNoSpinner();

            var imageName = MethodBase.GetCurrentMethod().Name;
            TakeScreenshot(imageName);

            _page.Find(By.LinkText("Queue")).Should().NotBeNull();
            _page.Find(By.LinkText("History")).Should().NotBeNull();
            _page.Find(By.LinkText("Blocklist")).Should().NotBeNull();
        }

        [Test]
        public void wanted_page()
        {
            _page.WantedNavIcon.Click();
            _page.WaitForNoSpinner();

            _page.Find(By.LinkText("Missing")).Should().NotBeNull();
            _page.Find(By.LinkText("Cutoff Unmet")).Should().NotBeNull();
        }

        [Test]
        public void system_page()
        {
            _page.SystemNavIcon.Click();
            _page.WaitForNoSpinner();

            var imageName = MethodBase.GetCurrentMethod().Name;
            TakeScreenshot(imageName);

            _page.Find(By.CssSelector("div[class*='Health']")).Should().NotBeNull();
        }

        [Test]
        public void add_series_page()
        {
            _page.SeriesNavIcon.Click();
            _page.WaitForNoSpinner();

            _page.Find(By.LinkText("Add New")).Click();

            var imageName = MethodBase.GetCurrentMethod().Name;
            TakeScreenshot(imageName);

            _page.Find(By.CssSelector("input[class*='AddNewSeries-searchInput']")).Should().NotBeNull();
        }
    }
}
