using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Notifications;
using NzbDrone.Core.Notifications.Plex.PlexTv;
using NzbDrone.Core.Notifications.Plex.Server;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Events;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.NotificationTests
{
    [TestFixture]
    public class SeriesFolderRenameFixture : CoreTest<NotificationService>
    {
        private SeriesMovedEvent _moved;
        private Mock<INotification> _notification;
        private NotificationDefinition _definition;

        [SetUp]
        public void Setup()
        {
            var source = @"C:\Test\TV\Old Title".AsOsAgnostic();
            var destination = @"C:\Test\TV\New Title".AsOsAgnostic();
            var series = new Series
            {
                Id = 1,
                Title = "Series",
                Path = destination,
                Tags = new HashSet<int> { 1 }
            };
            _moved = new SeriesMovedEvent(series, source, destination);

            _definition = new NotificationDefinition { Id = 1, Name = "Recorder", OnRename = true };
            _notification = new Mock<INotification>();
            _notification.SetupGet(n => n.Definition).Returns(_definition);
            Mocker.GetMock<INotificationFactory>()
                .Setup(f => f.OnRenameEnabled(true))
                .Returns(new List<INotification> { _notification.Object });
            Mocker.GetMock<IRootFolderService>()
                .Setup(s => s.GetBestRootFolderPath(It.IsAny<string>()))
                .Returns(@"C:\Test\TV".AsOsAgnostic());
        }

        private void RenameFolder()
        {
            Subject.Should().BeAssignableTo<IHandle<SeriesMovedEvent>>();
            ((IHandle<SeriesMovedEvent>)(object)Subject).Handle(_moved);
        }

        [Test]
        public void should_notify_with_destination_path_and_no_renamed_episode_files()
        {
            RenameFolder();

            _notification.Verify(n => n.OnRename(
                It.Is<Series>(s => s.Path == _moved.DestinationPath),
                It.Is<List<RenamedEpisodeFile>>(files => files.Count == 0)),
                Times.Once());
            _notification.Verify(n => n.ProcessQueue(), Times.Once());
        }

        [Test]
        public void should_include_subfolder_changes_within_the_same_root()
        {
            _moved.SourcePath = @"C:\Test\TV\O\Old Title".AsOsAgnostic();
            _moved.DestinationPath = @"C:\Test\TV\N\New Title".AsOsAgnostic();
            _moved.Series.Path = _moved.DestinationPath;

            RenameFolder();

            _notification.Verify(n => n.OnRename(_moved.Series, It.IsAny<List<RenamedEpisodeFile>>()), Times.Once());
        }

        [Test]
        public void should_ignore_cross_root_moves()
        {
            Mocker.GetMock<IRootFolderService>()
                .Setup(s => s.GetBestRootFolderPath(_moved.DestinationPath))
                .Returns(@"C:\Test\OtherTV".AsOsAgnostic());

            RenameFolder();

            _notification.Verify(n => n.OnRename(It.IsAny<Series>(), It.IsAny<List<RenamedEpisodeFile>>()), Times.Never());
            _notification.Verify(n => n.ProcessQueue(), Times.Never());
        }

        [Test]
        public void should_ignore_unchanged_paths()
        {
            _moved.SourcePath = _moved.DestinationPath;

            RenameFolder();

            _notification.Verify(n => n.OnRename(It.IsAny<Series>(), It.IsAny<List<RenamedEpisodeFile>>()), Times.Never());
            _notification.Verify(n => n.ProcessQueue(), Times.Never());
        }

        [TestCase(1, true)]
        [TestCase(2, false)]
        public void should_respect_notification_tags(int tag, bool expected)
        {
            _definition.Tags = new HashSet<int> { tag };

            RenameFolder();

            var times = expected ? Times.Once() : Times.Never();
            _notification.Verify(n => n.OnRename(It.IsAny<Series>(), It.IsAny<List<RenamedEpisodeFile>>()), times);
            _notification.Verify(n => n.ProcessQueue(), times);
        }

        [Test]
        public void should_not_notify_or_flush_connections_excluded_by_the_factory()
        {
            Mocker.GetMock<INotificationFactory>()
                .Setup(f => f.OnRenameEnabled(true)).Returns(new List<INotification>());

            RenameFolder();

            _notification.Verify(n => n.OnRename(It.IsAny<Series>(), It.IsAny<List<RenamedEpisodeFile>>()), Times.Never());
            _notification.Verify(n => n.ProcessQueue(), Times.Never());
        }

        [Test]
        public void should_preserve_episode_rename_files_and_defer_its_queue_processing()
        {
            var files = new List<RenamedEpisodeFile> { new RenamedEpisodeFile() };

            Subject.Handle(new SeriesRenamedEvent(_moved.Series, files));

            _notification.Verify(n => n.OnRename(_moved.Series, files), Times.Once());
            _notification.Verify(n => n.ProcessQueue(), Times.Never());
        }

        [Test]
        public void should_continue_to_other_connections_when_queue_processing_fails()
        {
            _notification.Setup(n => n.ProcessQueue()).Throws(new Exception("Synthetic queue failure"));
            var second = new Mock<INotification>();
            second.SetupGet(n => n.Definition).Returns(new NotificationDefinition { Id = 2, Name = "Second" });
            Mocker.GetMock<INotificationFactory>().Setup(f => f.OnRenameEnabled(true))
                .Returns(new List<INotification> { _notification.Object, second.Object });

            RenameFolder();

            ExceptionVerification.ExpectedWarns(1);
            Mocker.GetMock<INotificationStatusService>().Verify(s => s.RecordFailure(1, It.IsAny<TimeSpan>()), Times.Once());
            second.Verify(n => n.OnRename(_moved.Series, It.IsAny<List<RenamedEpisodeFile>>()), Times.Once());
            second.Verify(n => n.ProcessQueue(), Times.Once());
        }

        [TestCase(true, false)]
        [TestCase(false, false)]
        [TestCase(true, true)]
        [TestCase(false, true)]
        public void should_process_the_real_plex_queue_with_the_new_path(bool updateLibrary, bool alreadyQueued)
        {
            var settings = new PlexServerSettings { Host = "plex.invalid", AuthToken = "synthetic", UpdateLibrary = updateLibrary };
            var plex = Mocker.Resolve<PlexServer>();
            plex.Definition = new NotificationDefinition { Id = 3, Name = "Mocked Plex", Settings = settings, OnRename = true };
            Mocker.GetMock<INotificationFactory>().Setup(f => f.OnRenameEnabled(true))
                .Returns(new List<INotification> { plex });

            if (alreadyQueued)
            {
                plex.OnDownload(new DownloadMessage
                {
                    Series = new Series { Id = _moved.Series.Id, Path = _moved.SourcePath }
                });
            }

            RenameFolder();

            Mocker.GetMock<IPlexTvService>().Verify(s => s.Ping("synthetic"), Times.Exactly(alreadyQueued ? 2 : 1));
            Mocker.GetMock<IPlexServerService>().Verify(s => s.UpdateLibrary(
                It.Is<IEnumerable<Series>>(series => series.Count() == 1 && series.Single().Path == _moved.DestinationPath),
                settings),
                updateLibrary ? Times.Once() : Times.Never());

            // A later queue flush must not send the same rename a second time.
            plex.ProcessQueue();
            Mocker.GetMock<IPlexServerService>().Verify(s => s.UpdateLibrary(
                It.IsAny<IEnumerable<Series>>(), settings),
                updateLibrary ? Times.Once() : Times.Never());
        }
    }
}
