using System;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.SeriesStats;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.SeriesStatsTests
{
    [TestFixture]
    public class SeriesStatisticsFixture : DbTest<SeriesStatisticsRepository, Series>
    {
        private Series _series;
        private Episode _episode;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew()
                                        .With(s => s.Id = 0)
                                        .With(s => s.Runtime = 30)
                                        .Build();

            _series.Id = Db.Insert(_series).Id;

            _episode = Builder<Episode>.CreateNew()
                                          .With(e => e.Id = 0)
                                          .With(e => e.EpisodeFileId = 0)
                                          .With(e => e.Monitored = false)
                                          .With(e => e.SeriesId = _series.Id)
                                          .With(e => e.AirDateUtc = DateTime.Today.AddDays(5))
                                          .Build();
        }

        private void GivenEpisodeWithFile()
        {
            _episode.EpisodeFileId = 1;
        }

        private void GivenMonitoredEpisode()
        {
            _episode.Monitored = true;
        }

        private void GivenFile()
        {
            Db.Insert(_episode);
        }

        [Test]
        public void should_get_stats_for_series()
        {
            GivenMonitoredEpisode();
            GivenFile();

            var stats = Subject.SeriesStatistics();

            stats.Should().HaveCount(1);
            stats.First().NextAiring.Should().Be(_episode.AirDateUtc);
        }

        [Test]
        public void should_not_have_next_airing_for_episode_with_file()
        {
            GivenEpisodeWithFile();
            GivenFile();

            var stats = Subject.SeriesStatistics();

            stats.Should().HaveCount(1);
            stats.First().NextAiring.Should().NotHaveValue();
        }

        [Test]
        public void should_not_include_unmonitored_episode_in_episode_count()
        {
            GivenFile();

            var stats = Subject.SeriesStatistics();

            stats.Should().HaveCount(1);
            stats.First().EpisodeCount.Should().Be(0);
        }

        [Test]
        public void should_include_unmonitored_episode_with_file_in_episode_count()
        {
            GivenEpisodeWithFile();
            GivenFile();

            var stats = Subject.SeriesStatistics();

            stats.Should().HaveCount(1);
            stats.First().EpisodeCount.Should().Be(1);
        }
    }
}
