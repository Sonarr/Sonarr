using System;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Qualities;
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
        private EpisodeFile _episodeFile;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew()
                                        .With(s => s.Runtime = 30)
                                        .BuildNew();

            _series.Id = Db.Insert(_series).Id;

            _episode = Builder<Episode>.CreateNew()
                                          .With(e => e.EpisodeFileId = 0)
                                          .With(e => e.Monitored = false)
                                          .With(e => e.SeriesId = _series.Id)
                                          .With(e => e.AirDateUtc = DateTime.Today.AddDays(5))
                                          .BuildNew();

            _episodeFile = Builder<EpisodeFile>.CreateNew()
                                           .With(e => e.SeriesId = _series.Id)
                                           .With(e => e.Quality = new QualityModel(Quality.HDTV720p))
                                           .With(e => e.Language = Language.English)
                                           .BuildNew();
        }

        private void GivenEpisodeWithFile()
        {
            _episode.EpisodeFileId = 1;
        }

        private void GivenOldEpisode()
        {
            _episode.AirDateUtc = DateTime.Now.AddSeconds(-10);
        }

        private void GivenMonitoredEpisode()
        {
            _episode.Monitored = true;
        }

        private void GivenEpisode()
        {
            Db.Insert(_episode);
        }

        private void GivenEpisodeFile()
        {
            Db.Insert(_episodeFile);
        }

        [Test]
        public void should_get_stats_for_series()
        {
            GivenMonitoredEpisode();
            GivenEpisode();

            var stats = Subject.SeriesStatistics();

            stats.Should().HaveCount(1);
            stats.First().NextAiring.Should().Be(_episode.AirDateUtc);
            stats.First().PreviousAiring.Should().NotHaveValue();
        }

        [Test]
        public void should_not_have_next_airing_for_episode_with_file()
        {
            GivenEpisodeWithFile();
            GivenEpisode();

            var stats = Subject.SeriesStatistics();

            stats.Should().HaveCount(1);
            stats.First().NextAiring.Should().NotHaveValue();
        }

        [Test]
        public void should_have_previous_airing_for_old_episode_with_file()
        {
            GivenEpisodeWithFile();
            GivenOldEpisode();
            GivenEpisode();

            var stats = Subject.SeriesStatistics();

            stats.Should().HaveCount(1);
            stats.First().NextAiring.Should().NotHaveValue();
            stats.First().PreviousAiring.Should().Be(_episode.AirDateUtc);
        }

        [Test]
        public void should_have_previous_airing_for_old_episode_without_file_monitored()
        {
            GivenMonitoredEpisode();
            GivenOldEpisode();
            GivenEpisode();

            var stats = Subject.SeriesStatistics();

            stats.Should().HaveCount(1);
            stats.First().NextAiring.Should().NotHaveValue();
            stats.First().PreviousAiring.Should().Be(_episode.AirDateUtc);
        }

        [Test]
        public void should_not_have_previous_airing_for_old_episode_without_file_unmonitored()
        {
            GivenOldEpisode();
            GivenEpisode();

            var stats = Subject.SeriesStatistics();

            stats.Should().HaveCount(1);
            stats.First().NextAiring.Should().NotHaveValue();
            stats.First().PreviousAiring.Should().NotHaveValue();
        }

        [Test]
        public void should_not_include_unmonitored_episode_in_episode_count()
        {
            GivenEpisode();

            var stats = Subject.SeriesStatistics();

            stats.Should().HaveCount(1);
            stats.First().EpisodeCount.Should().Be(0);
        }

        [Test]
        public void should_include_unmonitored_episode_with_file_in_episode_count()
        {
            GivenEpisodeWithFile();
            GivenEpisode();

            var stats = Subject.SeriesStatistics();

            stats.Should().HaveCount(1);
            stats.First().EpisodeCount.Should().Be(1);
        }

        [Test]
        public void should_have_size_on_disk_of_zero_when_no_episode_file()
        {
            GivenEpisode();

            var stats = Subject.SeriesStatistics();

            stats.Should().HaveCount(1);
            stats.First().SizeOnDisk.Should().Be(0);
        }

        [Test]
        public void should_have_size_on_disk_when_episode_file_exists()
        {
            GivenEpisodeWithFile();
            GivenEpisode();
            GivenEpisodeFile();

            var stats = Subject.SeriesStatistics();

            stats.Should().HaveCount(1);
            stats.First().SizeOnDisk.Should().Be(_episodeFile.Size);
        }
    }
}
