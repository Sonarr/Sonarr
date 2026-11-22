using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NLog;
using NLog.Config;
using NLog.Targets;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.MediaFiles.Retention;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.MediaFiles.Retention
{
    [TestFixture]
    public class EpisodeRetentionServiceFixture : CoreTest<EpisodeRetentionService>
    {
        private Series _series;
        private List<Episode> _episodes;
        private List<EpisodeFile> _files;
        private LogFactory _logFactory;
        private MemoryTarget _logs;

        [SetUp]
        public void Setup()
        {
            _series = new Series { Id = 1, Title = "Retention Test", Path = "/tv/Retention Test", RetentionEnabled = true, RetentionKeepEpisodes = 2 };
            _episodes = new List<Episode>();
            _files = new List<EpisodeFile>();
            Mocker.GetMock<ISeriesService>().Setup(s => s.GetSeries(1)).Returns(_series);
            Mocker.GetMock<IEpisodeService>().Setup(s => s.GetEpisodeBySeries(1)).Returns(_episodes);
            Mocker.GetMock<IMediaFileService>().Setup(s => s.GetFilesBySeries(1)).Returns(_files);
            Mocker.GetMock<IRootFolderService>().Setup(s => s.GetBestRootFolderPath(_series.Path)).Returns("/tv");
            Mocker.GetMock<IDiskProvider>().Setup(s => s.FolderExists(It.IsAny<string>())).Returns(true);
            Mocker.GetMock<IEpisodeService>().Setup(s => s.GetEpisode(It.IsAny<int>()))
                  .Returns<int>(id => _episodes.SingleOrDefault(e => e.Id == id) ?? throw new ModelNotFoundException(typeof(Episode), id));
            Mocker.GetMock<IMediaFileService>().Setup(s => s.Get(It.IsAny<int>()))
                  .Returns<int>(id => _files.SingleOrDefault(f => f.Id == id) ?? throw new ModelNotFoundException(typeof(EpisodeFile), id));
            Mocker.GetMock<IEpisodeService>().Setup(s => s.SetMonitored(It.IsAny<IEnumerable<int>>(), It.IsAny<bool>()))
                  .Callback<IEnumerable<int>, bool>((ids, monitored) =>
                  {
                      foreach (var episode in _episodes.Where(e => ids.Contains(e.Id)))
                      {
                          episode.Monitored = monitored;
                      }
                  });
            Mocker.GetMock<IDeleteMediaFiles>().Setup(s => s.DeleteEpisodeFile(It.IsAny<Series>(), It.IsAny<EpisodeFile>(), It.IsAny<DeleteMediaFileReason>()))
                  .Callback<Series, EpisodeFile, DeleteMediaFileReason>((series, file, reason) =>
                  {
                      foreach (var episode in _episodes.Where(e => e.EpisodeFileId == file.Id))
                      {
                          episode.EpisodeFileId = 0;
                      }
                  });
            _logs = new MemoryTarget { Layout = "${level}|${message}" };
            var loggingConfiguration = new LoggingConfiguration();
            loggingConfiguration.AddRule(LogLevel.Debug, LogLevel.Fatal, _logs);
            _logFactory = new LogFactory { Configuration = loggingConfiguration };
            Mocker.SetConstant(_logFactory.GetLogger(nameof(EpisodeRetentionService)));
        }

        [TearDown]
        public void TearDown()
        {
            _logFactory.Dispose();
        }

        private Episode AddEpisode(int id, int season, int number, int fileId)
        {
            var episode = new Episode { Id = id, SeriesId = 1, SeasonNumber = season, EpisodeNumber = number, EpisodeFileId = fileId, Monitored = true, AirDate = new DateTime(2020, 1, 1).AddDays((season * 100) + number).ToString(Episode.AIR_DATE_FORMAT) };
            _episodes.Add(episode);
            if (fileId > 0 && _files.All(f => f.Id != fileId))
            {
                _files.Add(new EpisodeFile { Id = fileId, SeriesId = 1, RelativePath = fileId + ".mkv" });
            }

            return episode;
        }

        [Test]
        public void should_keep_newest_downloaded_episodes_across_seasons()
        {
            AddEpisode(1, 1, 20, 1);
            AddEpisode(2, 2, 1, 2);
            AddEpisode(3, 2, 2, 3);
            AddEpisode(4, 2, 3, 0);

            var plan = Subject.Preview(1);

            Assert.That(plan.FilesToDelete.Select(f => f.Id), Is.EqualTo(new[] { 1 }));
            Assert.That(plan.RetainedEpisodes.Select(e => e.Id), Is.EquivalentTo(new[] { 2, 3 }));
            Assert.That(plan.EpisodesToUnmonitor.Select(e => e.Id), Is.EqualTo(new[] { 1 }));
            Mocker.GetMock<IDeleteMediaFiles>().VerifyNoOtherCalls();
            Mocker.GetMock<IEpisodeService>().Verify(s => s.SetMonitored(It.IsAny<IEnumerable<int>>(), It.IsAny<bool>()), Times.Never());
            Assert.That(_logs.Logs, Is.Empty);
        }

        [Test]
        public void should_preserve_shared_file_at_retention_boundary()
        {
            AddEpisode(1, 1, 1, 1);
            AddEpisode(2, 1, 2, 2);
            AddEpisode(3, 1, 3, 2);
            AddEpisode(4, 1, 4, 3);

            var plan = Subject.Preview(1);

            Assert.That(plan.FilesToDelete.Select(f => f.Id), Is.EqualTo(new[] { 1 }));
            Assert.That(plan.RetainedEpisodes.Select(e => e.Id), Is.EquivalentTo(new[] { 2, 3, 4 }));
        }

        [Test]
        public void should_delete_a_shared_file_once_and_unmonitor_all_linked_episodes()
        {
            AddEpisode(1, 1, 1, 1);
            AddEpisode(2, 1, 2, 1);
            AddEpisode(3, 1, 3, 2);
            AddEpisode(4, 1, 4, 3);

            Trim();

            Mocker.GetMock<IDeleteMediaFiles>().Verify(s => s.DeleteEpisodeFile(_series, _files[0], DeleteMediaFileReason.Retention), Times.Once());
            Mocker.GetMock<IEpisodeService>().Verify(s => s.SetMonitored(It.Is<IEnumerable<int>>(ids => ids.SequenceEqual(new[] { 1, 2 })), false), Times.Once());
        }

        [Test]
        public void should_not_delete_when_at_or_below_keep_count()
        {
            AddEpisode(1, 1, 1, 1);
            AddEpisode(2, 1, 2, 2);

            Assert.That(Trim().FilesToDelete, Is.Empty);
            Mocker.GetMock<IDeleteMediaFiles>().VerifyNoOtherCalls();
            Assert.That(_logs.Logs.Single(), Does.StartWith("Debug|Skipping episode retention").And.Contain("no changes needed"));
        }

        [Test]
        public void should_preserve_excluded_specials_and_their_shared_files()
        {
            AddEpisode(1, 0, 1, 1);
            AddEpisode(2, 1, 1, 1);
            AddEpisode(3, 1, 2, 2);
            AddEpisode(4, 1, 3, 3);

            Assert.That(Subject.Preview(1).FilesToDelete, Is.Empty);
            _series.RetentionIncludeSpecials = true;
            Assert.That(Subject.Preview(1).FilesToDelete.Select(f => f.Id), Is.EqualTo(new[] { 1 }));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void should_reject_invalid_keep_even_when_disabled(int keep)
        {
            _series.RetentionEnabled = false;
            _series.RetentionKeepEpisodes = keep;

            Assert.Throws<ArgumentOutOfRangeException>(() => Subject.Preview(1));
            Assert.Throws<ArgumentOutOfRangeException>(() => Trim());
        }

        [Test]
        public void should_not_trim_or_unmonitor_disabled_series()
        {
            _series.RetentionEnabled = false;
            AddEpisode(1, 1, 1, 1);
            AddEpisode(2, 1, 2, 2);
            AddEpisode(3, 1, 3, 3);
            AddEpisode(4, 1, 0, 0);

            var plan = Trim();

            Assert.That(plan.FilesToDelete, Is.Empty);
            Assert.That(plan.EpisodesToUnmonitor, Is.Empty);
            Mocker.GetMock<IDeleteMediaFiles>().VerifyNoOtherCalls();
            Mocker.GetMock<IEpisodeService>().Verify(s => s.SetMonitored(It.IsAny<IEnumerable<int>>(), It.IsAny<bool>()), Times.Never());
            Assert.That(_logs.Logs.Single(), Does.StartWith("Debug|Skipping episode retention").And.Contain("retention disabled"));
        }

        [Test]
        public void should_unmonitor_trimmed_episodes_after_successful_deletion()
        {
            AddEpisode(1, 1, 1, 1);
            AddEpisode(2, 1, 2, 2);
            AddEpisode(3, 1, 3, 3);
            var deleted = false;
            Mocker.GetMock<IDeleteMediaFiles>().Setup(s => s.DeleteEpisodeFile(_series, _files[0], DeleteMediaFileReason.Retention)).Callback(() => deleted = true);
            Mocker.GetMock<IEpisodeService>().Setup(s => s.SetMonitored(It.IsAny<IEnumerable<int>>(), false)).Callback(() => Assert.That(deleted, Is.True));

            Trim();

            Mocker.GetMock<IDeleteMediaFiles>().Verify(s => s.DeleteEpisodeFile(_series, _files[0], DeleteMediaFileReason.Retention), Times.Once());
            Mocker.GetMock<IEpisodeService>().Verify(s => s.SetMonitored(It.Is<IEnumerable<int>>(ids => ids.SequenceEqual(new[] { 1 })), false), Times.Once());
        }

        [Test]
        public void should_leave_monitoring_unchanged_when_deletion_fails()
        {
            AddEpisode(1, 1, 1, 1);
            AddEpisode(2, 1, 2, 2);
            AddEpisode(3, 1, 3, 3);
            AddEpisode(4, 1, 0, 0);
            Mocker.GetMock<IDeleteMediaFiles>().Setup(s => s.DeleteEpisodeFile(_series, _files[0], DeleteMediaFileReason.Retention)).Throws(new InvalidOperationException());

            Assert.Throws<InvalidOperationException>(() => Trim());

            Mocker.GetMock<IEpisodeService>().Verify(s => s.SetMonitored(It.IsAny<IEnumerable<int>>(), It.IsAny<bool>()), Times.Never());
            Assert.That(_logs.Logs.Last(), Is.EqualTo("Warn|Episode retention failed for 'Retention Test' (series 1): deleted 0/1 files, unmonitored 0/2 episodes (0 older missing)"));
            Assert.That(_logs.Logs, Has.None.Contains("Completed episode retention"));
        }

        [Test]
        public void should_log_completed_file_and_monitoring_counts()
        {
            AddEpisode(1, 1, 2, 1);
            AddEpisode(2, 1, 3, 1).Monitored = false;
            AddEpisode(3, 1, 4, 2);
            AddEpisode(4, 1, 5, 3);
            AddEpisode(5, 1, 1, 0);

            Trim();

            Assert.That(_logs.Logs, Is.EqualTo(new[]
            {
                "Info|Starting episode retention for 'Retention Test' (series 1): keep 2 episodes, delete 1 files, unmonitor 2 episodes",
                "Info|Completed episode retention for 'Retention Test' (series 1): keep 2 episodes, deleted 1 files, unmonitored 2 episodes (1 older missing)"
            }));
        }

        [Test]
        public void should_log_partial_completion_when_later_deletion_fails()
        {
            AddEpisode(1, 1, 1, 1);
            AddEpisode(2, 1, 2, 2);
            AddEpisode(3, 1, 3, 3);
            AddEpisode(4, 1, 4, 4);
            Mocker.GetMock<IDeleteMediaFiles>().Setup(s => s.DeleteEpisodeFile(_series, _files[1], DeleteMediaFileReason.Retention)).Throws(new InvalidOperationException());

            Assert.Throws<InvalidOperationException>(() => Trim());

            Assert.That(_logs.Logs.Last(), Is.EqualTo("Warn|Episode retention failed for 'Retention Test' (series 1): deleted 1/2 files, unmonitored 1/2 episodes (0 older missing)"));
            Assert.That(_logs.Logs, Has.None.Contains("Completed episode retention"));
        }

        [Test]
        public void should_count_deletion_when_unmonitoring_fails()
        {
            AddEpisode(1, 1, 1, 1);
            AddEpisode(2, 1, 2, 2);
            AddEpisode(3, 1, 3, 3);
            Mocker.GetMock<IEpisodeService>().Setup(s => s.SetMonitored(It.IsAny<IEnumerable<int>>(), false)).Throws(new InvalidOperationException());

            Assert.Throws<InvalidOperationException>(() => Trim());

            Assert.That(_logs.Logs.Last(), Is.EqualTo("Warn|Episode retention failed for 'Retention Test' (series 1): deleted 1/1 files, unmonitored 0/1 episodes (0 older missing)"));
            Assert.That(_logs.Logs, Has.None.Contains("Completed episode retention"));
        }

        [Test]
        public void should_unmonitor_missing_episodes_before_oldest_downloaded_episode_before_trimming()
        {
            AddEpisode(1, 1, 3, 1);
            AddEpisode(2, 1, 6, 2);
            AddEpisode(3, 1, 7, 3);
            AddEpisode(4, 1, 2, 0);
            AddEpisode(5, 1, 4, 0);

            var plan = Trim();

            Assert.That(plan.FilesToDelete.Select(f => f.Id), Is.EqualTo(new[] { 1 }));
            Assert.That(plan.OlderMissingEpisodesToUnmonitor.Select(e => e.Id), Is.EqualTo(new[] { 4 }));
            Assert.That(plan.EpisodesToUnmonitor.Select(e => e.Id), Is.EquivalentTo(new[] { 1, 4 }));
            Mocker.GetMock<IEpisodeService>().Verify(s => s.SetMonitored(It.Is<IEnumerable<int>>(ids => ids.SequenceEqual(new[] { 4 })), false), Times.Once());
            Mocker.GetMock<IEpisodeService>().Verify(s => s.SetMonitored(It.Is<IEnumerable<int>>(ids => ids.Contains(5)), false), Times.Never());
        }

        [Test]
        public void should_unmonitor_older_missing_episodes_when_all_downloads_are_kept()
        {
            AddEpisode(1, 1, 3, 1);
            AddEpisode(2, 1, 6, 2);
            AddEpisode(3, 1, 2, 0);
            AddEpisode(4, 1, 4, 0);

            var plan = Trim();

            Assert.That(plan.FilesToDelete, Is.Empty);
            Assert.That(plan.OlderMissingEpisodesToUnmonitor.Select(e => e.Id), Is.EqualTo(new[] { 3 }));
            Mocker.GetMock<IDeleteMediaFiles>().VerifyNoOtherCalls();
            Mocker.GetMock<IEpisodeService>().Verify(s => s.SetMonitored(It.Is<IEnumerable<int>>(ids => ids.SequenceEqual(new[] { 3 })), false), Times.Once());
        }

        [Test]
        public void should_use_unmonitored_download_as_missing_episode_boundary()
        {
            AddEpisode(1, 1, 3, 1).Monitored = false;
            AddEpisode(2, 1, 6, 2);
            AddEpisode(3, 1, 2, 0);
            AddEpisode(4, 1, 4, 0);
            AddEpisode(5, 1, 1, 0).Monitored = false;

            Assert.That(Subject.Preview(1).OlderMissingEpisodesToUnmonitor.Select(e => e.Id), Is.EqualTo(new[] { 3 }));
        }

        [Test]
        public void should_preserve_missing_episodes_at_shared_file_boundary()
        {
            AddEpisode(1, 1, 2, 1);
            AddEpisode(2, 1, 3, 1);
            AddEpisode(3, 1, 4, 2);
            AddEpisode(4, 1, 1, 0);
            AddEpisode(5, 1, 2, 0);
            AddEpisode(6, 1, 5, 0);

            var plan = Trim();

            Assert.That(plan.FilesToDelete, Is.Empty);
            Assert.That(plan.OlderMissingEpisodesToUnmonitor.Select(e => e.Id), Is.EqualTo(new[] { 4 }));
        }

        [Test]
        public void should_exclude_specials_from_missing_episode_boundary()
        {
            AddEpisode(1, 0, 2, 1);
            AddEpisode(2, 1, 3, 2);
            AddEpisode(3, 0, 1, 0);
            AddEpisode(4, 1, 2, 0);

            var plan = Subject.Preview(1);

            Assert.That(plan.OlderMissingEpisodesToUnmonitor.Select(e => e.Id), Is.EqualTo(new[] { 4 }));
            _series.RetentionIncludeSpecials = true;
            Assert.That(Subject.Preview(1).OlderMissingEpisodesToUnmonitor.Select(e => e.Id), Is.EqualTo(new[] { 3 }));
        }

        [Test]
        public void should_leave_missing_episodes_monitored_without_downloaded_boundary()
        {
            AddEpisode(1, 1, 1, 0);

            Assert.That(Trim().EpisodesToUnmonitor, Is.Empty);
            Mocker.GetMock<IEpisodeService>().Verify(s => s.SetMonitored(It.IsAny<IEnumerable<int>>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void should_leave_missing_episodes_monitored_when_only_excluded_specials_are_downloaded()
        {
            AddEpisode(1, 0, 2, 1);
            AddEpisode(2, 0, 1, 0);
            AddEpisode(3, 1, 1, 0);

            Assert.That(Trim().EpisodesToUnmonitor, Is.Empty);
            Mocker.GetMock<IEpisodeService>().Verify(s => s.SetMonitored(It.IsAny<IEnumerable<int>>(), It.IsAny<bool>()), Times.Never());
        }

        private EpisodeRetentionPlan Trim()
        {
            var plan = Subject.Preview(1);
            Subject.Execute(new EpisodeRetentionCommand { SeriesId = 1 });
            return plan;
        }

        private void ExecuteSelection(List<int> files, List<int> episodeIds)
        {
            Subject.Execute(new EpisodeRetentionCommand
            {
                SeriesId = 1,
                Files = files,
                EpisodeIds = episodeIds,
                Trigger = CommandTrigger.Manual
            });
        }

        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        public void should_require_both_selections_for_manual_commands(bool filesProvided, bool episodesProvided)
        {
            Assert.Throws<NzbDroneClientException>(() => ExecuteSelection(filesProvided ? new List<int>() : null, episodesProvided ? new List<int>() : null));
            Mocker.GetMock<IDeleteMediaFiles>().VerifyNoOtherCalls();
        }

        [Test]
        public void should_allow_explicitly_empty_selections_without_expanding_them()
        {
            AddEpisode(1, 1, 1, 1);
            AddEpisode(2, 1, 2, 2);
            AddEpisode(3, 1, 3, 3);
            AddEpisode(4, 1, 0, 0);

            ExecuteSelection(new List<int>(), new List<int>());

            Mocker.GetMock<IDeleteMediaFiles>().VerifyNoOtherCalls();
            Mocker.GetMock<IEpisodeService>().Verify(s => s.SetMonitored(It.IsAny<IEnumerable<int>>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void should_only_trim_selected_files_and_missing_episodes()
        {
            AddEpisode(1, 1, 3, 1);
            AddEpisode(2, 1, 4, 2);
            AddEpisode(3, 1, 5, 3);
            AddEpisode(4, 1, 6, 4);
            AddEpisode(5, 1, 1, 0);
            AddEpisode(6, 1, 2, 0);

            ExecuteSelection(new List<int> { 1, 1 }, new List<int> { 5, 5 });

            Mocker.GetMock<IDeleteMediaFiles>().Verify(s => s.DeleteEpisodeFile(_series, _files[0], DeleteMediaFileReason.Retention), Times.Once());
            Mocker.GetMock<IDeleteMediaFiles>().VerifyNoOtherCalls();
            Assert.That(_episodes.Where(e => !e.Monitored).Select(e => e.Id), Is.EquivalentTo(new[] { 1, 5 }));
        }

        [Test]
        public void should_reject_foreign_files_before_any_mutation()
        {
            AddEpisode(1, 1, 1, 1);
            AddEpisode(2, 1, 2, 2);
            AddEpisode(3, 1, 3, 3);
            Mocker.GetMock<IMediaFileService>().Setup(s => s.Get(99)).Returns(new EpisodeFile { Id = 99, SeriesId = 2 });

            Assert.Throws<NzbDroneClientException>(() => ExecuteSelection(new List<int> { 1, 99 }, new List<int>()));

            Mocker.GetMock<IDeleteMediaFiles>().VerifyNoOtherCalls();
            Mocker.GetMock<IEpisodeService>().Verify(s => s.SetMonitored(It.IsAny<IEnumerable<int>>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void should_reject_foreign_episodes_before_any_mutation()
        {
            AddEpisode(1, 1, 1, 1);
            AddEpisode(2, 1, 2, 2);
            AddEpisode(3, 1, 3, 3);
            Mocker.GetMock<IEpisodeService>().Setup(s => s.GetEpisode(99)).Returns(new Episode { Id = 99, SeriesId = 2 });

            Assert.Throws<NzbDroneClientException>(() => ExecuteSelection(new List<int> { 1 }, new List<int> { 99 }));

            Mocker.GetMock<IDeleteMediaFiles>().VerifyNoOtherCalls();
            Mocker.GetMock<IEpisodeService>().Verify(s => s.SetMonitored(It.IsAny<IEnumerable<int>>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void should_skip_missing_selections_without_substituting_replacement_files()
        {
            AddEpisode(1, 1, 1, 123);
            AddEpisode(2, 1, 2, 2);
            AddEpisode(3, 1, 3, 3);

            ExecuteSelection(new List<int> { 1 }, new List<int> { 99 });

            Mocker.GetMock<IDeleteMediaFiles>().VerifyNoOtherCalls();
            Mocker.GetMock<IEpisodeService>().Verify(s => s.SetMonitored(It.IsAny<IEnumerable<int>>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void should_skip_selections_that_are_no_longer_eligible()
        {
            AddEpisode(1, 1, 1, 1);
            AddEpisode(2, 1, 2, 2);
            AddEpisode(3, 1, 3, 3);
            AddEpisode(4, 1, 0, 4);
            _series.RetentionKeepEpisodes = 4;

            ExecuteSelection(new List<int> { 1 }, new List<int> { 4 });

            Mocker.GetMock<IDeleteMediaFiles>().VerifyNoOtherCalls();
            Mocker.GetMock<IEpisodeService>().Verify(s => s.SetMonitored(It.IsAny<IEnumerable<int>>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void should_load_current_plan_data_once_and_avoid_individual_owned_selection_queries()
        {
            AddEpisode(1, 1, 2, 1);
            AddEpisode(2, 1, 3, 2);
            AddEpisode(3, 1, 4, 3);
            AddEpisode(4, 1, 1, 0);

            ExecuteSelection(new List<int> { 1 }, new List<int> { 4 });

            Mocker.GetMock<IMediaFileService>().Verify(s => s.GetFilesBySeries(1), Times.Once());
            Mocker.GetMock<IEpisodeService>().Verify(s => s.GetEpisodeBySeries(1), Times.Once());
            Mocker.GetMock<IMediaFileService>().Verify(s => s.Get(It.IsAny<int>()), Times.Never());
        }

        [Test]
        public void should_keep_new_specials_by_air_date()
        {
            AddEpisode(1, 1, 1, 1).AirDate = "2020-01-01";
            AddEpisode(2, 1, 2, 2).AirDate = "2020-02-01";
            AddEpisode(3, 0, 1, 3).AirDate = "2020-03-01";
            _series.RetentionIncludeSpecials = true;

            var plan = Subject.Preview(1);

            Assert.That(plan.FilesToDelete.Select(f => f.Id), Is.EqualTo(new[] { 1 }));
            Assert.That(plan.RetainedEpisodes.Select(e => e.Id), Is.EqualTo(new[] { 3, 2 }));
        }

        [Test]
        public void should_use_air_date_instead_of_season_order()
        {
            AddEpisode(1, 2, 1, 1).AirDate = "2020-01-01";
            AddEpisode(2, 1, 1, 2).AirDate = "2020-02-01";
            AddEpisode(3, 1, 2, 3).AirDate = "2020-03-01";

            Assert.That(Subject.Preview(1).FilesToDelete.Select(f => f.Id), Is.EqualTo(new[] { 1 }));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("unknown")]
        [TestCase("2020-02-30")]
        public void should_protect_unknown_dates_and_exclude_them_from_keep_count(string airDate)
        {
            AddEpisode(1, 1, 1, 1);
            AddEpisode(2, 1, 2, 2);
            AddEpisode(3, 1, 3, 3);
            AddEpisode(4, 2, 1, 4).AirDate = airDate;
            AddEpisode(5, 1, 0, 0).AirDate = airDate;

            var plan = Subject.Preview(1);

            Assert.That(plan.FilesToDelete.Select(f => f.Id), Is.EqualTo(new[] { 1 }));
            Assert.That(plan.EpisodesToUnmonitor.Select(e => e.Id), Is.EqualTo(new[] { 1 }));
        }

        [Test]
        public void should_protect_shared_files_linked_to_undated_episodes()
        {
            AddEpisode(1, 1, 1, 1);
            AddEpisode(2, 1, 2, 2);
            AddEpisode(3, 1, 3, 3);
            AddEpisode(4, 2, 1, 1).AirDate = null;

            Assert.That(Subject.Preview(1).FilesToDelete, Is.Empty);
        }

        [Test]
        public void should_compare_missing_episode_boundary_by_air_date()
        {
            AddEpisode(1, 1, 1, 1).AirDate = "2020-02-01";
            AddEpisode(2, 1, 2, 2).AirDate = "2020-03-01";
            AddEpisode(3, 9, 1, 0).AirDate = "2020-01-01";
            AddEpisode(4, 0, 1, 0).AirDate = "2020-02-01";
            AddEpisode(5, 1, 0, 0).AirDate = "2020-04-01";
            _series.RetentionIncludeSpecials = true;

            Assert.That(Subject.Preview(1).OlderMissingEpisodesToUnmonitor.Select(e => e.Id), Is.EqualTo(new[] { 3 }));
        }

        [TestCase(false, false, true, false)]
        [TestCase(true, true, true, false)]
        [TestCase(true, false, false, false)]
        [TestCase(false, false, true, true)]
        [TestCase(true, true, true, true)]
        [TestCase(true, false, false, true)]
        public void should_check_disk_availability_before_any_mutation(bool rootExists, bool rootEmpty, bool seriesExists, bool missingOnly)
        {
            AddEpisode(1, 1, 2, 1);
            AddEpisode(2, 1, 3, 2);
            AddEpisode(3, 1, 1, 0);
            if (!missingOnly)
            {
                AddEpisode(4, 1, 4, 3);
            }

            Mocker.GetMock<IDiskProvider>().Setup(s => s.FolderExists("/tv")).Returns(rootExists);
            Mocker.GetMock<IDiskProvider>().Setup(s => s.FolderEmpty("/tv")).Returns(rootEmpty);
            Mocker.GetMock<IDiskProvider>().Setup(s => s.FolderExists(_series.Path)).Returns(seriesExists);

            Assert.Throws<NzbDroneClientException>(() => Subject.Execute(new EpisodeRetentionCommand { SeriesId = 1 }));

            Mocker.GetMock<IDeleteMediaFiles>().VerifyNoOtherCalls();
            Mocker.GetMock<IEpisodeService>().Verify(s => s.SetMonitored(It.IsAny<IEnumerable<int>>(), It.IsAny<bool>()), Times.Never());
            Mocker.GetMock<IEventAggregator>().VerifyNoOtherCalls();
        }

        [Test]
        public void should_publish_current_episode_models_and_one_current_series_model()
        {
            AddEpisode(1, 1, 2, 1);
            AddEpisode(2, 1, 3, 2);
            AddEpisode(3, 1, 4, 3);
            AddEpisode(4, 1, 1, 0);
            var currentSeries = new Series { Id = 1, Title = "Current Title" };
            Mocker.GetMock<ISeriesService>().SetupSequence(s => s.GetSeries(1)).Returns(_series).Returns(currentSeries);

            Subject.Execute(new EpisodeRetentionCommand { SeriesId = 1 });

            Mocker.GetMock<IEventAggregator>().Verify(s => s.PublishEvent(It.Is<ModelEvent<Episode>>(e => e.ModelId == 1 && !e.Model.Monitored && !e.Model.HasFile && e.Action == ModelAction.Updated)), Times.Once());
            Mocker.GetMock<IEventAggregator>().Verify(s => s.PublishEvent(It.Is<ModelEvent<Episode>>(e => e.ModelId == 4 && !e.Model.Monitored && e.Action == ModelAction.Updated)), Times.Once());
            Mocker.GetMock<IEventAggregator>().Verify(s => s.PublishEvent(It.Is<ModelEvent<Series>>(e => ReferenceEquals(e.Model, currentSeries) && e.Action == ModelAction.Updated)), Times.Once());
            Mocker.GetMock<IEventAggregator>().VerifyNoOtherCalls();
        }

        [Test]
        public void should_publish_series_update_after_partial_failure()
        {
            AddEpisode(1, 1, 1, 1);
            AddEpisode(2, 1, 2, 2);
            AddEpisode(3, 1, 3, 3);
            AddEpisode(4, 1, 4, 4);
            Mocker.GetMock<IDeleteMediaFiles>().Setup(s => s.DeleteEpisodeFile(_series, _files[1], DeleteMediaFileReason.Retention)).Throws(new InvalidOperationException());

            Assert.Throws<InvalidOperationException>(() => Subject.Execute(new EpisodeRetentionCommand { SeriesId = 1 }));

            Mocker.GetMock<IEventAggregator>().Verify(s => s.PublishEvent(It.Is<ModelEvent<Episode>>(e => e.ModelId == 1)), Times.Once());
            Mocker.GetMock<IEventAggregator>().Verify(s => s.PublishEvent(It.IsAny<ModelEvent<Series>>()), Times.Once());
            Mocker.GetMock<IEventAggregator>().VerifyNoOtherCalls();
        }

        [Test]
        public void should_not_publish_model_updates_when_nothing_changed()
        {
            AddEpisode(1, 1, 1, 1);

            Subject.Execute(new EpisodeRetentionCommand { SeriesId = 1 });

            Mocker.GetMock<IEventAggregator>().VerifyNoOtherCalls();
        }

        [Test]
        public void should_queue_cleanup_after_scan_without_deleting_inline()
        {
            Subject.Handle(new SeriesScannedEvent(_series, new List<string>()));

            Mocker.GetMock<IManageCommandQueue>().Verify(s => s.Push(It.Is<EpisodeRetentionCommand>(c => c.SeriesId == 1 && c.RequiresDiskAccess && c.IsExclusive && c.SendUpdatesToClient && c.SuppressMessages && c.Files == null && c.EpisodeIds == null), CommandPriority.Normal, CommandTrigger.Unspecified), Times.Once());
            Mocker.GetMock<IDeleteMediaFiles>().VerifyNoOtherCalls();
        }

        [TestCase(SeriesScanSkippedReason.NeverRescanAfterRefresh, true)]
        [TestCase(SeriesScanSkippedReason.RescanAfterManualRefreshOnly, true)]
        [TestCase(SeriesScanSkippedReason.RootFolderDoesNotExist, false)]
        [TestCase(SeriesScanSkippedReason.RootFolderIsEmpty, false)]
        [TestCase((SeriesScanSkippedReason)99, false)]
        public void should_queue_only_intentionally_skipped_scans(SeriesScanSkippedReason reason, bool queued)
        {
            Subject.Handle(new SeriesScanSkippedEvent(_series, reason));

            Mocker.GetMock<IManageCommandQueue>().Verify(s => s.Push(It.IsAny<EpisodeRetentionCommand>(), It.IsAny<CommandPriority>(), It.IsAny<CommandTrigger>()), queued ? Times.Once() : Times.Never());
            Mocker.GetMock<IDeleteMediaFiles>().VerifyNoOtherCalls();
        }

        [Test]
        public void should_not_queue_disabled_policy()
        {
            _series.RetentionEnabled = false;

            Subject.Handle(new SeriesScannedEvent(_series, new List<string>()));
            Subject.Handle(new SeriesScanSkippedEvent(_series, SeriesScanSkippedReason.NeverRescanAfterRefresh));

            Mocker.GetMock<IManageCommandQueue>().Verify(s => s.Push(It.IsAny<EpisodeRetentionCommand>(), It.IsAny<CommandPriority>(), It.IsAny<CommandTrigger>()), Times.Never());
        }
    }
}
