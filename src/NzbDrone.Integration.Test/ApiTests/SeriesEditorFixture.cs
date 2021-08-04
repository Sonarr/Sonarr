using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Test.Common;
using Sonarr.Api.V3.Series;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class SeriesEditorFixture : IntegrationTest
    {
        private void GivenExistingSeries()
        {
            WaitForCompletion(() => Profiles.All().Count > 0);

            foreach (var title in new[] { "90210", "Dexter" })
            {
                var newSeries = Series.Lookup(title).First();

                newSeries.QualityProfileId = 1;
                newSeries.LanguageProfileId = 1;
                newSeries.Path = string.Format(@"C:\Test\{0}", title).AsOsAgnostic();

                Series.Post(newSeries);
            }
        }

        [Test]
        public void should_be_able_to_update_multiple_series()
        {
            GivenExistingSeries();

            var series = Series.All();

            var seriesEditor = new SeriesEditorResource
            {
                QualityProfileId = 2,
                SeriesIds = series.Select(s => s.Id).ToList()
            };

            var result = Series.Editor(seriesEditor);

            result.Should().HaveCount(2);
            result.TrueForAll(s => s.QualityProfileId == 2).Should().BeTrue();
        }
    }
}
