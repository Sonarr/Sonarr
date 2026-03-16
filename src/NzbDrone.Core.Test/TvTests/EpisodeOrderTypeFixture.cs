using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MetadataSource.Tvdb;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.TvTests
{
    [TestFixture]
    public class EpisodeOrderTypeFixture
    {
        [Test]
        public void default_should_be_zero()
        {
            ((int)EpisodeOrderType.Default).Should().Be(0);
        }

        [Test]
        public void should_have_all_expected_values()
        {
            var values = Enum.GetValues(typeof(EpisodeOrderType)).Cast<EpisodeOrderType>().ToList();
            values.Should().Contain(EpisodeOrderType.Default);
            values.Should().Contain(EpisodeOrderType.Dvd);
            values.Should().Contain(EpisodeOrderType.Absolute);
            values.Should().Contain(EpisodeOrderType.Alternate);
            values.Should().Contain(EpisodeOrderType.AltDvd);
            values.Should().Contain(EpisodeOrderType.Regional);
            values.Should().HaveCount(6);
        }

        [TestCase(EpisodeOrderType.Default, "official")]
        [TestCase(EpisodeOrderType.Dvd, "dvd")]
        [TestCase(EpisodeOrderType.Absolute, "absolute")]
        [TestCase(EpisodeOrderType.Alternate, "alternate")]
        [TestCase(EpisodeOrderType.AltDvd, "altdvd")]
        [TestCase(EpisodeOrderType.Regional, "regional")]
        public void should_map_order_type_to_tvdb_season_type(EpisodeOrderType orderType, string expectedSeasonType)
        {
            TvdbApiClient.MapOrderTypeToSeasonType(orderType).Should().Be(expectedSeasonType);
        }

        [TestCase("official", EpisodeOrderType.Default)]
        [TestCase("dvd", EpisodeOrderType.Dvd)]
        [TestCase("absolute", EpisodeOrderType.Absolute)]
        [TestCase("alternate", EpisodeOrderType.Alternate)]
        [TestCase("altdvd", EpisodeOrderType.AltDvd)]
        [TestCase("regional", EpisodeOrderType.Regional)]
        [TestCase("unknown_type", EpisodeOrderType.Default)]
        public void should_map_tvdb_season_type_to_order_type(string seasonType, EpisodeOrderType expectedOrderType)
        {
            TvdbApiClient.MapSeasonTypeToOrderType(seasonType).Should().Be(expectedOrderType);
        }
    }
}
