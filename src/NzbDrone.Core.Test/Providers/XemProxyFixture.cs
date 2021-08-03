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
            var ids = Subject.GetXemSeriesIds();

            ids.Should().NotBeEmpty();
            ids.Should().Contain(i => i == 73141);
        }

        [TestCase(12345, Description = "invalid id")]
        [TestCase(279042, Description = "no single connection")]
        public void should_return_empty_when_known_error(int id)
        {
            Subject.GetSceneTvdbMappings(id).Should().BeEmpty();
        }

        [TestCase(82807)]
        [TestCase(73141, Description = "American Dad!")]
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
