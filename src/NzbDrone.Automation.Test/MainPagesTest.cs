using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Automation.Test.PageModel;
using OpenQA.Selenium;

namespace NzbDrone.Automation.Test
{
    [TestFixture]
    public class MainPagesTest : AutomationTest
    {
        private PageBase page;

        [SetUp]
        public void Setup()
        {
            page = new PageBase(driver);
        }

        [Test]
        public void series_page()
        {
            page.SeriesNavIcon.Click();
            page.WaitForNoSpinner();

            page.Find(By.CssSelector("div[class*='SeriesIndex']")).Should().NotBeNull();
        }

        [Test]
        public void calendar_page()
        {
            page.CalendarNavIcon.Click();
            page.WaitForNoSpinner();

            page.Find(By.CssSelector("div[class*='CalendarPage']")).Should().NotBeNull();
        }

        [Test]
        public void activity_page()
        {
            page.ActivityNavIcon.Click();
            page.WaitForNoSpinner();

            page.Find(By.LinkText("Queue")).Should().NotBeNull();
            page.Find(By.LinkText("History")).Should().NotBeNull();
            page.Find(By.LinkText("Blacklist")).Should().NotBeNull();
        }

        [Test]
        public void wanted_page()
        {
            page.WantedNavIcon.Click();
            page.WaitForNoSpinner();

            page.Find(By.LinkText("Missing")).Should().NotBeNull();
            page.Find(By.LinkText("Cutoff Unmet")).Should().NotBeNull();
        }

        [Test]
        public void system_page()
        {
            page.SystemNavIcon.Click();
            page.WaitForNoSpinner();

            page.Find(By.CssSelector("div[class*='Health']")).Should().NotBeNull();
        }

        [Test]
        public void add_series_page()
        {
            page.SeriesNavIcon.Click();
            page.WaitForNoSpinner();

            page.Find(By.LinkText("Add New")).Click();

            page.WaitForNoSpinner();

            page.Find(By.CssSelector("input[class*='AddNewSeries/searchInput']")).Should().NotBeNull();
        }
    }
}