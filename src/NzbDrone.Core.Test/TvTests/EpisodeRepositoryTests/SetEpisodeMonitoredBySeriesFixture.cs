using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.TvTests.EpisodeRepositoryTests
{
    [TestFixture]
    public class SetEpisodeMonitoredBySeriesFixture : DbTest<EpisodeRepository, Episode>
    {
        private Series _series;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew()
                                     .With(s => s.Id = 0)
                                     .With(s => s.Runtime = 30)
                                     .With(s => s.Monitored = true)
                                     .Build();

            _series.Id = Db.Insert(_series).Id;
        }

        private Episode GivenEpisode(int seasonNumber, int episodeNumber, bool hasFile, DateTime? airDateUtc, bool monitored = false)
        {
            var episode = Builder<Episode>.CreateNew()
                                          .With(e => e.Id = 0)
                                          .With(e => e.SeriesId = _series.Id)
                                          .With(e => e.SeasonNumber = seasonNumber)
                                          .With(e => e.EpisodeNumber = episodeNumber)
                                          .With(e => e.EpisodeFileId = hasFile ? 1 : 0)
                                          .With(e => e.AirDateUtc = airDateUtc)
                                          .With(e => e.Monitored = monitored)
                                          .Build();

            return Db.Insert(episode);
        }

        private Dictionary<int, bool> MonitoredByEpisodeId()
        {
            return Subject.GetEpisodes(_series.Id).ToDictionary(e => e.Id, e => e.Monitored);
        }

        [Test]
        public void should_monitor_all_regular_episodes_and_unmonitor_specials_for_all()
        {
            var special = GivenEpisode(0, 1, false, DateTime.UtcNow.AddDays(-1), monitored: true);
            var aired = GivenEpisode(1, 1, false, DateTime.UtcNow.AddDays(-1));
            var withFile = GivenEpisode(2, 1, true, DateTime.UtcNow.AddDays(-1));

            Subject.SetMonitored(_series.Id, MonitorTypes.All, 1, 2);

            var monitored = MonitoredByEpisodeId();
            monitored[special.Id].Should().BeFalse();
            monitored[aired.Id].Should().BeTrue();
            monitored[withFile.Id].Should().BeTrue();
        }

        [Test]
        public void should_unmonitor_everything_for_none()
        {
            var special = GivenEpisode(0, 1, false, DateTime.UtcNow.AddDays(-1), monitored: true);
            var regular = GivenEpisode(1, 1, true, DateTime.UtcNow.AddDays(-1), monitored: true);

            Subject.SetMonitored(_series.Id, MonitorTypes.None, 1, 2);

            var monitored = MonitoredByEpisodeId();
            monitored[special.Id].Should().BeFalse();
            monitored[regular.Id].Should().BeFalse();
        }

        [Test]
        public void should_monitor_only_episodes_without_files_for_missing()
        {
            var missing = GivenEpisode(1, 1, false, DateTime.UtcNow.AddDays(-1));
            var existing = GivenEpisode(1, 2, true, DateTime.UtcNow.AddDays(-1), monitored: true);

            Subject.SetMonitored(_series.Id, MonitorTypes.Missing, 1, 1);

            var monitored = MonitoredByEpisodeId();
            monitored[missing.Id].Should().BeTrue();
            monitored[existing.Id].Should().BeFalse();
        }

        [Test]
        public void should_monitor_only_episodes_with_files_for_existing()
        {
            var missing = GivenEpisode(1, 1, false, DateTime.UtcNow.AddDays(-1), monitored: true);
            var existing = GivenEpisode(1, 2, true, DateTime.UtcNow.AddDays(-1));

            Subject.SetMonitored(_series.Id, MonitorTypes.Existing, 1, 1);

            var monitored = MonitoredByEpisodeId();
            monitored[missing.Id].Should().BeFalse();
            monitored[existing.Id].Should().BeTrue();
        }

        [Test]
        public void should_monitor_future_and_tba_episodes_for_future()
        {
            var aired = GivenEpisode(1, 1, false, DateTime.UtcNow.AddDays(-1), monitored: true);
            var future = GivenEpisode(1, 2, false, DateTime.UtcNow.AddDays(7));
            var tba = GivenEpisode(1, 3, false, null);

            Subject.SetMonitored(_series.Id, MonitorTypes.Future, 1, 1);

            var monitored = MonitoredByEpisodeId();
            monitored[aired.Id].Should().BeFalse();
            monitored[future.Id].Should().BeTrue();
            monitored[tba.Id].Should().BeTrue();
        }

        [Test]
        public void should_monitor_episodes_within_90_days_and_future_for_recent()
        {
            var old = GivenEpisode(1, 1, false, DateTime.UtcNow.AddDays(-200), monitored: true);
            var recent = GivenEpisode(1, 2, false, DateTime.UtcNow.AddDays(-5));
            var future = GivenEpisode(1, 3, false, DateTime.UtcNow.AddDays(30));
            var tba = GivenEpisode(1, 4, false, null);

            Subject.SetMonitored(_series.Id, MonitorTypes.Recent, 1, 1);

            var monitored = MonitoredByEpisodeId();
            monitored[old.Id].Should().BeFalse();
            monitored[recent.Id].Should().BeTrue();
            monitored[future.Id].Should().BeTrue();
            monitored[tba.Id].Should().BeTrue();
        }

        [Test]
        public void should_monitor_only_pilot_for_pilot()
        {
            var pilot = GivenEpisode(1, 1, false, DateTime.UtcNow.AddDays(-1));
            var other = GivenEpisode(1, 2, false, DateTime.UtcNow.AddDays(-1), monitored: true);
            var laterSeason = GivenEpisode(2, 1, false, DateTime.UtcNow.AddDays(-1), monitored: true);

            Subject.SetMonitored(_series.Id, MonitorTypes.Pilot, 1, 2);

            var monitored = MonitoredByEpisodeId();
            monitored[pilot.Id].Should().BeTrue();
            monitored[other.Id].Should().BeFalse();
            monitored[laterSeason.Id].Should().BeFalse();
        }

        [Test]
        public void should_monitor_only_first_season_for_first_season()
        {
            var firstSeason = GivenEpisode(1, 1, false, DateTime.UtcNow.AddDays(-1));
            var secondSeason = GivenEpisode(2, 1, false, DateTime.UtcNow.AddDays(-1), monitored: true);

            Subject.SetMonitored(_series.Id, MonitorTypes.FirstSeason, 1, 2);

            var monitored = MonitoredByEpisodeId();
            monitored[firstSeason.Id].Should().BeTrue();
            monitored[secondSeason.Id].Should().BeFalse();
        }

        [Test]
        public void should_monitor_only_last_season_for_last_season()
        {
            var firstSeason = GivenEpisode(1, 1, false, DateTime.UtcNow.AddDays(-1), monitored: true);
            var lastSeason = GivenEpisode(2, 1, false, DateTime.UtcNow.AddDays(-1));

            Subject.SetMonitored(_series.Id, MonitorTypes.LastSeason, 1, 2);

            var monitored = MonitoredByEpisodeId();
            monitored[firstSeason.Id].Should().BeFalse();
            monitored[lastSeason.Id].Should().BeTrue();
        }

        [Test]
        public void should_only_change_specials_for_monitor_specials()
        {
            var special = GivenEpisode(0, 1, false, DateTime.UtcNow.AddDays(-1));
            var regularMonitored = GivenEpisode(1, 1, false, DateTime.UtcNow.AddDays(-1), monitored: true);
            var regularUnmonitored = GivenEpisode(1, 2, false, DateTime.UtcNow.AddDays(-1));

            Subject.SetMonitored(_series.Id, MonitorTypes.MonitorSpecials, 1, 1);

            var monitored = MonitoredByEpisodeId();
            monitored[special.Id].Should().BeTrue();

            // Regular seasons must be left untouched.
            monitored[regularMonitored.Id].Should().BeTrue();
            monitored[regularUnmonitored.Id].Should().BeFalse();
        }

        [Test]
        public void should_only_change_specials_for_unmonitor_specials()
        {
            var special = GivenEpisode(0, 1, false, DateTime.UtcNow.AddDays(-1), monitored: true);
            var regularMonitored = GivenEpisode(1, 1, false, DateTime.UtcNow.AddDays(-1), monitored: true);
            var regularUnmonitored = GivenEpisode(1, 2, false, DateTime.UtcNow.AddDays(-1));

            Subject.SetMonitored(_series.Id, MonitorTypes.UnmonitorSpecials, 1, 1);

            var monitored = MonitoredByEpisodeId();
            monitored[special.Id].Should().BeFalse();

            // Regular seasons must be left untouched.
            monitored[regularMonitored.Id].Should().BeTrue();
            monitored[regularUnmonitored.Id].Should().BeFalse();
        }

        [Test]
        public void should_return_distinct_season_numbers_with_monitored_episodes()
        {
            GivenEpisode(1, 1, false, DateTime.UtcNow.AddDays(-1));
            GivenEpisode(1, 2, true, DateTime.UtcNow.AddDays(-1));
            GivenEpisode(2, 1, false, DateTime.UtcNow.AddDays(-1));
            GivenEpisode(3, 1, true, DateTime.UtcNow.AddDays(-1));

            var monitoredSeasons = Subject.SetMonitored(_series.Id, MonitorTypes.Missing, 1, 3);

            // Only seasons 1 and 2 have a missing (and now monitored) episode.
            monitoredSeasons.Should().BeEquivalentTo(new[] { 1, 2 });
        }
    }
}
