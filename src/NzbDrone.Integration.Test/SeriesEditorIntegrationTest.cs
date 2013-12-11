using System;
using System.Net;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Api.Series;
using System.Linq;
using NzbDrone.Test.Common;

namespace NzbDrone.Integration.Test
{
    [TestFixture]
    public class SeriesEditorIntegrationTest : IntegrationTest
    {
        private void GivenExistingSeries()
        {
            foreach (var title in new[] { "90210", "Dexter" })
            {
                var newSeries = Series.Lookup(title).First();

                newSeries.QualityProfileId = 1;
                newSeries.Path = String.Format(@"C:\Test\{0}", title).AsOsAgnostic();

                Series.Post(newSeries);
            }
        }

        [Test]
        public void should_be_able_to_update_multiple_series()
        {
            GivenExistingSeries();

            var series = Series.All();

            foreach (var s in series)
            {
                s.QualityProfileId = 2;
            }

            var result = Series.Editor(series);

            result.Should().HaveCount(2);
            result.TrueForAll(s => s.QualityProfileId == 2).Should().BeTrue();
        }
    }
}