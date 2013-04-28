using System.IO;
using System.Net;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Api.RootFolders;
using NzbDrone.Api.Series;
using System.Linq;

namespace NzbDrone.Integration.Test
{
    [TestFixture]
    public class SeriesIntegrationTest : IntegrationTest
    {
        [Test]
        public void should_have_no_series_on_start_application()
        {
            Series.All().Should().BeEmpty();
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

        [Test]
        public void should_be_able_to_add_and_delete_series()
        {
            var series = Series.Lookup("archer").First();

            var rootFolder = RootFolders.Post(new RootFolderResource { Path = Directory.GetCurrentDirectory() });

            series.RootFolderId = rootFolder.Id;
            series.QualityProfileId = 1;

            series = Series.Post(series);

            Series.All().Should().HaveCount(1);

            Series.Delete(series.Id);

            Series.All().Should().BeEmpty();
        }
    }
}