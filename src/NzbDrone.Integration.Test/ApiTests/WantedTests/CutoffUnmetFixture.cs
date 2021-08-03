using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Integration.Test.ApiTests.WantedTests
{
    [TestFixture]
    public class CutoffUnmetFixture : IntegrationTest
    {
        [Test]
        [Order(1)]
        public void cutoff_should_have_monitored_items()
        {
            EnsureProfileCutoff(1, Quality.HDTV720p);
            var series = EnsureSeries(266189, "The Blacklist", true);
            EnsureEpisodeFile(series, 1, 1, Quality.SDTV);

            var result = WantedCutoffUnmet.GetPaged(0, 15, "airDateUtc", "desc");

            result.Records.Should().NotBeEmpty();
        }

        [Test]
        [Order(1)]
        public void cutoff_should_not_have_unmonitored_items()
        {
            EnsureProfileCutoff(1, Quality.HDTV720p);
            var series = EnsureSeries(266189, "The Blacklist", false);
            EnsureEpisodeFile(series, 1, 1, Quality.SDTV);

            var result = WantedCutoffUnmet.GetPaged(0, 15, "airDateUtc", "desc");

            result.Records.Should().BeEmpty();
        }

        [Test]
        [Order(1)]
        public void cutoff_should_have_series()
        {
            EnsureProfileCutoff(1, Quality.HDTV720p);
            var series = EnsureSeries(266189, "The Blacklist", true);
            EnsureEpisodeFile(series, 1, 1, Quality.SDTV);

            var result = WantedCutoffUnmet.GetPaged(0, 15, "airDateUtc", "desc");

            result.Records.First().Series.Should().NotBeNull();
            result.Records.First().Series.Title.Should().Be("The Blacklist");
        }

        [Test]
        [Order(2)]
        public void cutoff_should_have_unmonitored_items()
        {
            EnsureProfileCutoff(1, Quality.HDTV720p);
            var series = EnsureSeries(266189, "The Blacklist", false);
            EnsureEpisodeFile(series, 1, 1, Quality.SDTV);

            var result = WantedCutoffUnmet.GetPaged(0, 15, "airDateUtc", "desc", "monitored", "false");

            result.Records.Should().NotBeEmpty();
        }
    }
}
