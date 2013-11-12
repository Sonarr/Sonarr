using FluentAssertions;
using NUnit.Framework;

namespace NzbDrone.Automation.Test
{
    [TestFixture]
    public class MainPagesTest : AutomationTest
    {
        [Test]
        public void series_page()
        {
            driver.FindElementByLinkText("Series").Click();
            driver.FindElementByClassName("iv-series-index-seriesindexlayout").Should().NotBeNull();
        }

        [Test]
        public void calendar_page()
        {
            driver.FindElementByLinkText("Calendar").Click();
            driver.FindElementByClassName("iv-calendar-calendarlayout").Should().NotBeNull();
        }

        [Test]
        public void history_page()
        {
            driver.FindElementByLinkText("History").Click();
            driver.FindElementByClassName("iv-history-historylayout").Should().NotBeNull();
        }

        [Test]
        public void missing_page()
        {
            driver.FindElementByLinkText("Settings").Click();
          
        }

        [Test]
        public void system_page()
        {
            driver.FindElementByLinkText("System").Click();
            driver.FindElementByClassName("iv-system-systemlayout").Should().NotBeNull();
        }


    }
}