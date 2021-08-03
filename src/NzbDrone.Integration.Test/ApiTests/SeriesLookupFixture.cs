using FluentAssertions;
using NUnit.Framework;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class SeriesLookupFixture : IntegrationTest
    {
        [TestCase("archer", "Archer (2009)")]
        [TestCase("90210", "90210")]
        public void lookup_new_series_by_title(string term, string title)
        {
            var series = Series.Lookup(term);

            series.Should().NotBeEmpty();
            series.Should().Contain(c => c.Title == title);
        }

        [Test]
        public void lookup_new_series_by_tvdbid()
        {
            var series = Series.Lookup("tvdb:266189");

            series.Should().NotBeEmpty();
            series.Should().Contain(c => c.Title == "The Blacklist");
        }

        [Test]
        [Ignore("Unreliable")]
        public void lookup_random_series_using_asterix()
        {
            var series = Series.Lookup("*");

            series.Should().NotBeEmpty();
        }
    }
}
