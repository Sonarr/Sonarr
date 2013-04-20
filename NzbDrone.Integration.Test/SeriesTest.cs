using System.Net;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Api.Series;

namespace NzbDrone.Integration.Test
{
    [TestFixture]
    public class SeriesTest : SmokeTestBase
    {
        [Test]
        public void should_have_no_series_on_start_application()
        {
            Series.GetAll().Should().BeEmpty();
        }

        [Test]
        public void series_lookup_on_trakt()
        {
            var series = Series.Lookup("archer");

            series.Should().NotBeEmpty();
            series.Should().Contain(c => c.Title == "Archer (2009)");
        }

        [Test]
        public void add_series_without_required_fields_should_return_badrequest()
        {
            var errors = Series.InvalidPost(new SeriesResource());
            errors.Should().NotBeEmpty();
        }

    }
}