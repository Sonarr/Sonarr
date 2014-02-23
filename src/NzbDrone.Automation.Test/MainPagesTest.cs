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
            page.FindByClass("iv-series-index-seriesindexlayout").Should().NotBeNull();
        }

        [Test]
        public void calendar_page()
        {
            page.CalendarNavIcon.Click();
            page.WaitForNoSpinner();

            page.FindByClass("iv-calendar-calendarlayout").Should().NotBeNull();
        }

        [Test]
        public void history_page()
        {
            page.HistoryNavIcon.Click();
            page.WaitForNoSpinner();

            page.FindByClass("iv-history-historylayout").Should().NotBeNull();
        }

        [Test]
        public void wanted_page()
        {
            page.WantedNavIcon.Click();
            page.WaitForNoSpinner();

            page.FindByClass("iv-wanted-missing-missinglayout").Should().NotBeNull();
        }

        [Test]
        public void system_page()
        {
            page.SystemNavIcon.Click();
            page.WaitForNoSpinner();

            page.FindByClass("iv-system-systemlayout").Should().NotBeNull();
        }


        [Test]
        public void add_series_page()
        {
            page.SeriesNavIcon.Click();
            page.WaitForNoSpinner();

            page.Find(By.LinkText("Add Series")).Click();

            page.WaitForNoSpinner();

            page.FindByClass("iv-addseries-addserieslayout").Should().NotBeNull();
        }


    }
}