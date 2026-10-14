using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NLog;
using NLog.Config;
using NLog.Targets;
using NUnit.Framework;
using NzbDrone.Core.Download;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.MediaFiles.Retention;
using NzbDrone.Core.Messaging.Commands;
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
            _series = new Series { Id = 1, Title = "Retention Test", RetentionEnabled = true, RetentionKeepEpisodes = 2 };
            _episodes = new List<Episode>();
            _files = new List<EpisodeFile>();
            Mocker.GetMock<ISeriesService>().Setup(s => s.GetSeries(1)).Returns(_series);
            Mocker.GetMock<IEpisodeService>().Setup(s => s.GetEpisodeBySeries(1)).Returns(_episodes);
            Mocker.GetMock<IMediaFileService>().Setup(s => s.GetFilesBySeries(1)).Returns(_files);
            Mocker.GetMock<IManageCommandQueue>().Setup(s => s.GetStarted()).Returns(new List<CommandModel>());
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
            var episode = new Episode { Id = id, SeriesId = 1, SeasonNumber = season, EpisodeNumber = number, EpisodeFileId = fileId, Monitored = true };
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

            Subject.Trim(1);

            Mocker.GetMock<IDeleteMediaFiles>().Verify(s => s.DeleteEpisodeFile(_series, _files[0], DeleteMediaFileReason.Retention), Times.Once());
            Mocker.GetMock<IEpisodeService>().Verify(s => s.SetMonitored(It.Is<IEnumerable<int>>(ids => ids.SequenceEqual(new[] { 1, 2 })), false), Times.Once());
        }

        [Test]
        public void should_not_delete_when_at_or_below_keep_count()
        {
            AddEpisode(1, 1, 1, 1);
            AddEpisode(2, 1, 2, 2);

            Assert.That(Subject.Trim(1).FilesToDelete, Is.Empty);
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
            Assert.Throws<ArgumentOutOfRangeException>(() => Subject.Trim(1));
        }

        [Test]
        public void should_not_trim_or_unmonitor_disabled_series()
        {
            _series.RetentionEnabled = false;
            AddEpisode(1, 1, 1, 1);
            AddEpisode(2, 1, 2, 2);
            AddEpisode(3, 1, 3, 3);
            AddEpisode(4, 1, 0, 0);

            var plan = Subject.Trim(1);

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

            Subject.Trim(1);

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

            Assert.Throws<InvalidOperationException>(() => Subject.Trim(1));

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

            Subject.Trim(1);

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

            Assert.Throws<InvalidOperationException>(() => Subject.Trim(1));

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

            Assert.Throws<InvalidOperationException>(() => Subject.Trim(1));

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

            var plan = Subject.Trim(1);

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

            var plan = Subject.Trim(1);

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

            var plan = Subject.Trim(1);

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

            Assert.That(Subject.Trim(1).EpisodesToUnmonitor, Is.Empty);
            Mocker.GetMock<IEpisodeService>().Verify(s => s.SetMonitored(It.IsAny<IEnumerable<int>>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void should_leave_missing_episodes_monitored_when_only_excluded_specials_are_downloaded()
        {
            AddEpisode(1, 0, 2, 1);
            AddEpisode(2, 0, 1, 0);
            AddEpisode(3, 1, 1, 0);

            Assert.That(Subject.Trim(1).EpisodesToUnmonitor, Is.Empty);
            Mocker.GetMock<IEpisodeService>().Verify(s => s.SetMonitored(It.IsAny<IEnumerable<int>>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void should_reject_changed_preview_before_any_mutation()
        {
            AddEpisode(1, 1, 1, 1);
            AddEpisode(2, 1, 2, 2);
            AddEpisode(3, 1, 3, 3);
            var token = Subject.Preview(1).PreviewToken;
            _files[0].RelativePath = "replacement.mkv";

            Assert.Throws<NzbDroneClientException>(() => Subject.Trim(1, token));
            Mocker.GetMock<IDeleteMediaFiles>().VerifyNoOtherCalls();
            Mocker.GetMock<IEpisodeService>().Verify(s => s.SetMonitored(It.IsAny<IEnumerable<int>>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void should_accept_unchanged_preview()
        {
            AddEpisode(1, 1, 1, 1);

            var token = Subject.Preview(1).PreviewToken;

            Assert.That(Subject.Trim(1, token).PreviewToken, Is.EqualTo(token));
        }

        [Test]
        public void should_check_preview_token_when_executing_queued_command()
        {
            var token = Subject.Preview(1).PreviewToken;
            _series.RetentionKeepEpisodes = 3;

            Assert.Throws<NzbDroneClientException>(() => Subject.Execute(new EpisodeRetentionCommand { SeriesId = 1, PreviewToken = token }));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void should_require_preview_for_manual_command(string token)
        {
            Assert.Throws<NzbDroneClientException>(() => Subject.Execute(new EpisodeRetentionCommand { SeriesId = 1, Trigger = CommandTrigger.Manual, PreviewToken = token }));

            Mocker.GetMock<IDeleteMediaFiles>().VerifyNoOtherCalls();
            Mocker.GetMock<IEpisodeService>().Verify(s => s.SetMonitored(It.IsAny<IEnumerable<int>>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void should_reject_trim_while_import_command_is_running()
        {
            Mocker.GetMock<IManageCommandQueue>().Setup(s => s.GetStarted()).Returns(new List<CommandModel>
            {
                new CommandModel { Body = new ProcessMonitoredDownloadsCommand() }
            });

            Assert.Throws<NzbDroneClientException>(() => Subject.Trim(1));
            Mocker.GetMock<IDeleteMediaFiles>().VerifyNoOtherCalls();
        }

        [Test]
        public void should_queue_cleanup_after_download_completion_without_deleting_inline()
        {
            Subject.Handle(new DownloadCompletedEvent(null, 1, new List<EpisodeFile>(), null));

            Mocker.GetMock<IManageCommandQueue>().Verify(s => s.Push(It.Is<EpisodeRetentionCommand>(c => c.SeriesId == 1 && c.RequiresDiskAccess && c.SendUpdatesToClient && c.SuppressMessages), CommandPriority.Normal, CommandTrigger.Unspecified), Times.Once());
            Mocker.GetMock<IDeleteMediaFiles>().VerifyNoOtherCalls();
        }

        [TestCase(true, true, true, true)]
        [TestCase(false, true, true, false)]
        [TestCase(true, false, true, false)]
        [TestCase(true, true, false, false)]
        public void should_queue_new_folder_imports_without_deleting_inline(bool newDownload, bool folderScan, bool enabled, bool queued)
        {
            _series.RetentionEnabled = enabled;
            if (folderScan)
            {
                Mocker.GetMock<IManageCommandQueue>().Setup(s => s.GetStarted()).Returns(new List<CommandModel>
                {
                    new CommandModel { Body = new DownloadedEpisodesScanCommand() }
                });
            }

            Subject.Handle(new EpisodeImportedEvent(null, new EpisodeFile { SeriesId = 1 }, new List<DeletedEpisodeFile>(), newDownload, null));

            Mocker.GetMock<IManageCommandQueue>().Verify(s => s.Push(It.Is<EpisodeRetentionCommand>(c => c.SeriesId == 1 && c.RequiresDiskAccess && c.SendUpdatesToClient && c.SuppressMessages), CommandPriority.Normal, CommandTrigger.Unspecified), queued ? Times.Once() : Times.Never());
            Mocker.GetMock<IDeleteMediaFiles>().VerifyNoOtherCalls();
            Mocker.GetMock<IEpisodeService>().Verify(s => s.SetMonitored(It.IsAny<IEnumerable<int>>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void should_wait_for_download_completion_for_tracked_folder_imports()
        {
            Mocker.GetMock<IManageCommandQueue>().Setup(s => s.GetStarted()).Returns(new List<CommandModel>
            {
                new CommandModel { Body = new DownloadedEpisodesScanCommand() }
            });

            Subject.Handle(new EpisodeImportedEvent(null, new EpisodeFile { SeriesId = 1 }, new List<DeletedEpisodeFile>(), true, new DownloadClientItem { DownloadId = "tracked-download" }));

            Mocker.GetMock<IManageCommandQueue>().Verify(s => s.Push(It.IsAny<EpisodeRetentionCommand>(), It.IsAny<CommandPriority>(), It.IsAny<CommandTrigger>()), Times.Never());
            Mocker.GetMock<IDeleteMediaFiles>().VerifyNoOtherCalls();
        }

        [Test]
        public void should_not_queue_disabled_policy()
        {
            _series.RetentionEnabled = false;

            Subject.Handle(new DownloadCompletedEvent(null, 1, new List<EpisodeFile>(), null));

            Mocker.GetMock<IManageCommandQueue>().Verify(s => s.Push(It.IsAny<EpisodeRetentionCommand>(), It.IsAny<CommandPriority>(), It.IsAny<CommandTrigger>()), Times.Never());
        }
    }
}
