using System.Net;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Api.Series;
using System.Linq;
using NzbDrone.Test.Common;

namespace NzbDrone.Integration.Test
{
    [TestFixture]
    public class SeriesIntegrationTest : IntegrationTest
    {
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

        [Test]
        public void should_be_able_to_add_and_delete_series()
        {
            var series = Series.Lookup("archer").First();

            series.QualityProfileId = 1;
            series.Path = @"C:\Test\Archer".AsOsAgnostic();

            series = Series.Post(series);

            Series.All().Should().HaveCount(1);

            Series.Get(series.Id).Should().NotBeNull();

            Series.Delete(series.Id);

            Series.All().Should().BeEmpty();
        }

        [Test]
        public void should_be_able_to_find_series_by_id()
        {
            var series = Series.Lookup("90210").First();

            series.QualityProfileId = 1;
            series.Path = @"C:\Test\90210".AsOsAgnostic();

            series = Series.Post(series);

            Series.All().Should().HaveCount(1);

            Series.Get(series.Id).Should().NotBeNull();
        }

        [Test]
        public void invalid_id_should_return_404()
        {
            Series.Get(99, HttpStatusCode.NotFound);
        }
    }
}