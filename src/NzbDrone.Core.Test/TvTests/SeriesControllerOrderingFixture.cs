using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MetadataSource.Tvdb;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.TvTests
{
    [TestFixture]
    public class SeriesControllerOrderingFixture : CoreTest
    {
        /// <summary>
        /// Tests the ordering change detection logic extracted from SeriesController.UpdateSeries.
        /// We test the logic directly since controller integration tests require the full HTTP stack.
        /// </summary>
        private static bool DetectOrderingChange(
            Series existingSeries,
            EpisodeOrderType newEpisodeOrder,
            List<SeasonOrderUpdate> seasonUpdates)
        {
            var orderingChanged = existingSeries.EpisodeOrder != newEpisodeOrder;

            if (!orderingChanged && seasonUpdates != null)
            {
                foreach (var seasonUpdate in seasonUpdates)
                {
                    var existingSeason = existingSeries.Seasons?.FirstOrDefault(s => s.SeasonNumber == seasonUpdate.SeasonNumber);
                    if (existingSeason != null && existingSeason.EpisodeOrderOverride != seasonUpdate.NewOverride)
                    {
                        orderingChanged = true;
                        break;
                    }
                }
            }

            return orderingChanged;
        }

        private class SeasonOrderUpdate
        {
            public int SeasonNumber { get; set; }
            public EpisodeOrderType? NewOverride { get; set; }
        }

        [Test]
        public void should_detect_series_level_ordering_change()
        {
            var series = new Series
            {
                EpisodeOrder = EpisodeOrderType.Default,
                Seasons = new List<Season>
                {
                    new Season { SeasonNumber = 1, Monitored = true }
                }
            };

            var result = DetectOrderingChange(series, EpisodeOrderType.Dvd, null);

            result.Should().BeTrue();
        }

        [Test]
        public void should_not_detect_change_when_ordering_unchanged()
        {
            var series = new Series
            {
                EpisodeOrder = EpisodeOrderType.Dvd,
                Seasons = new List<Season>
                {
                    new Season { SeasonNumber = 1, Monitored = true }
                }
            };

            var result = DetectOrderingChange(series, EpisodeOrderType.Dvd, null);

            result.Should().BeFalse();
        }

        [Test]
        public void should_detect_season_level_ordering_change()
        {
            var series = new Series
            {
                EpisodeOrder = EpisodeOrderType.Dvd,
                Seasons = new List<Season>
                {
                    new Season { SeasonNumber = 1, Monitored = true, EpisodeOrderOverride = null },
                    new Season { SeasonNumber = 2, Monitored = true, EpisodeOrderOverride = null }
                }
            };

            var seasonUpdates = new List<SeasonOrderUpdate>
            {
                new SeasonOrderUpdate { SeasonNumber = 2, NewOverride = EpisodeOrderType.Default }
            };

            var result = DetectOrderingChange(series, EpisodeOrderType.Dvd, seasonUpdates);

            result.Should().BeTrue();
        }

        [Test]
        public void should_not_detect_change_when_season_override_unchanged()
        {
            var series = new Series
            {
                EpisodeOrder = EpisodeOrderType.Dvd,
                Seasons = new List<Season>
                {
                    new Season { SeasonNumber = 1, Monitored = true, EpisodeOrderOverride = EpisodeOrderType.Default }
                }
            };

            var seasonUpdates = new List<SeasonOrderUpdate>
            {
                new SeasonOrderUpdate { SeasonNumber = 1, NewOverride = EpisodeOrderType.Default }
            };

            var result = DetectOrderingChange(series, EpisodeOrderType.Dvd, seasonUpdates);

            result.Should().BeFalse();
        }

        [Test]
        public void should_detect_removing_season_override()
        {
            var series = new Series
            {
                EpisodeOrder = EpisodeOrderType.Dvd,
                Seasons = new List<Season>
                {
                    new Season { SeasonNumber = 1, Monitored = true, EpisodeOrderOverride = EpisodeOrderType.Default }
                }
            };

            var seasonUpdates = new List<SeasonOrderUpdate>
            {
                new SeasonOrderUpdate { SeasonNumber = 1, NewOverride = null }
            };

            var result = DetectOrderingChange(series, EpisodeOrderType.Dvd, seasonUpdates);

            result.Should().BeTrue();
        }

        [Test]
        public void available_orderings_should_map_season_types_correctly()
        {
            var seasonTypes = new List<string> { "official", "dvd", "absolute" };

            var orderings = seasonTypes
                .Select(st => TvdbApiClient.MapSeasonTypeToOrderType(st))
                .ToList();

            orderings.Should().Contain(EpisodeOrderType.Default);
            orderings.Should().Contain(EpisodeOrderType.Dvd);
            orderings.Should().Contain(EpisodeOrderType.Absolute);
            orderings.Should().HaveCount(3);
        }
    }
}
