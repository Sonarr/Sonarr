using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DataAugmentation;
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

        [Test]
        public void should_return_empty_when_series_is_not_found()
        {
            Subject.GetSceneTvdbMappings(12345).Should().BeEmpty();
        }


        [Test]
        public void should_get_mapping()
        {
            var result = Subject.GetSceneTvdbMappings(82807);

            result.Should().NotBeEmpty();
            result.Should().OnlyContain(c => c.Scene != null);
            result.Should().OnlyContain(c => c.Tvdb != null);
        }
    }
}