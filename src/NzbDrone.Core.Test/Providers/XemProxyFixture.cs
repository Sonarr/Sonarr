using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DataAugmentation.Xem;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common.Categories;

namespace NzbDrone.Core.Test.Providers
{
    [TestFixture]
    [IntegrationTest]
    public class XemProxyFixture : CoreTest<XemProxy>
    {
        [SetUp]
        public void Setup()
        {
            UseRealHttp();
        }

        [Test]
        public void get_series_ids()
        {
            Subject.GetXemSeriesIds().Should().NotBeEmpty();
        }


        [Test]
        [Ignore("XEM's data is not clean")]
        public void get_mapping_for_all_series()
        {
            var ids = Subject.GetXemSeriesIds();

            var randomIds = ids.OrderBy(x => Guid.NewGuid()).Take(5);

            foreach (var randomId in randomIds)
            {
                Subject.GetSceneTvdbMappings(randomId).Should().NotBeEmpty();
            }
        }

        [TestCase(12345, Description = "invalid id")]
        [TestCase(267440, Description = "no single connection")]
        public void should_return_empty_when_known_error(int id)
        {
            Subject.GetSceneTvdbMappings(id).Should().BeEmpty();
        }

        [TestCase(82807)]
        public void should_get_mapping(int seriesId)
        {
            var result = Subject.GetSceneTvdbMappings(seriesId);

            result.Should().NotBeEmpty();
            result.Should().OnlyContain(c => c.Scene != null);
            result.Should().OnlyContain(c => c.Tvdb != null);
        }


        [TestCase(78916)]
        public void should_filter_out_episodes_without_scene_mapping(int seriesId)
        {
            var result = Subject.GetSceneTvdbMappings(seriesId);

            result.Should().NotContain(c => c.Tvdb == null);
        }
    }
}