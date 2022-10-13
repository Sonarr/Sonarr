using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Http;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients;
using NzbDrone.Core.Download.Clients.FreeboxDownload;
using NzbDrone.Core.Download.Clients.FreeboxDownload.Responses;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Test.Download.DownloadClientTests.FreeboxDownloadTests
{
    [TestFixture]
    public class TorrentFreeboxDownloadFixture : DownloadClientFixtureBase<TorrentFreeboxDownload>
    {
        protected FreeboxDownloadSettings _settings;

        protected FreeboxDownloadConfiguration _downloadConfiguration;

        protected FreeboxDownloadTask _task;

        protected string _defaultDestination = @"/some/path";
        protected string _encodedDefaultDestination = "L3NvbWUvcGF0aA==";
        protected string _category = "somecat";
        protected string _encodedDefaultDestinationAndCategory = "L3NvbWUvcGF0aC9zb21lY2F0";
        protected string _destinationDirectory = @"/path/to/media";
        protected string _encodedDestinationDirectory = "L3BhdGgvdG8vbWVkaWE=";
        protected OsPath _physicalPath = new OsPath("/mnt/sdb1/mydata");
        protected string _downloadURL => "magnet:?xt=urn:btih:5dee65101db281ac9c46344cd6b175cdcad53426&dn=download";

        [SetUp]
        public void Setup()
        {
            Subject.Definition = new DownloadClientDefinition();

            _settings = new FreeboxDownloadSettings()
            {
                Host = "127.0.0.1",
                Port = 443,
                ApiUrl = "/api/v1/",
                AppId = "someid",
                AppToken = "S0mEv3RY1oN9T0k3n"
            };

            Subject.Definition.Settings = _settings;

            _downloadConfiguration = new FreeboxDownloadConfiguration()
            {
                DownloadDirectory = _encodedDefaultDestination
            };

            _task = new FreeboxDownloadTask()
            {
                Id = "id0",
                Name = "name",
                DownloadDirectory = "L3NvbWUvcGF0aA==",
                InfoHash = "HASH",
                QueuePosition = 1,
                Status = FreeboxDownloadTaskStatus.Unknown,
                Eta = 0,
                Error = "none",
                Type = FreeboxDownloadTaskType.Bt.ToString(),
                IoPriority = FreeboxDownloadTaskIoPriority.Normal.ToString(),
                StopRatio = 150,
                PieceLength = 125,
                CreatedTimestamp = 1665261599,
                Size = 1000,
                ReceivedPrct = 0,
                ReceivedBytes = 0,
                ReceivedRate = 0,
                TransmittedPrct = 0,
                TransmittedBytes = 0,
                TransmittedRate = 0,
            };

            Mocker.GetMock<IHttpClient>()
                  .Setup(s => s.Get(It.IsAny<HttpRequest>()))
                  .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), new byte[0]));
        }

        protected void GivenCategory()
        {
            _settings.Category = _category;
        }

        protected void GivenDestinationDirectory()
        {
            _settings.DestinationDirectory = _destinationDirectory;
        }

        protected virtual void GivenDownloadConfiguration()
        {
            Mocker.GetMock<IFreeboxDownloadProxy>()
                  .Setup(s => s.GetDownloadConfiguration(It.IsAny<FreeboxDownloadSettings>()))
                  .Returns(_downloadConfiguration);
        }

        protected virtual void GivenTasks(List<FreeboxDownloadTask> torrents)
        {
            if (torrents == null)
            {
                torrents = new List<FreeboxDownloadTask>();
            }

            Mocker.GetMock<IFreeboxDownloadProxy>()
                  .Setup(s => s.GetTasks(It.IsAny<FreeboxDownloadSettings>()))
                  .Returns(torrents);
        }

        protected void PrepareClientToReturnQueuedItem()
        {
            _task.Status = FreeboxDownloadTaskStatus.Queued;

            GivenTasks(new List<FreeboxDownloadTask>
            {
                _task
            });
        }

        protected void GivenSuccessfulDownload()
        {
            Mocker.GetMock<IHttpClient>()
                  .Setup(s => s.Get(It.IsAny<HttpRequest>()))
                  .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), new byte[1000]));

            Mocker.GetMock<IFreeboxDownloadProxy>()
                  .Setup(s => s.AddTaskFromUrl(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<double?>(), It.IsAny<FreeboxDownloadSettings>()))
                  .Callback(PrepareClientToReturnQueuedItem);

            Mocker.GetMock<IFreeboxDownloadProxy>()
                  .Setup(s => s.AddTaskFromFile(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<double?>(), It.IsAny<FreeboxDownloadSettings>()))
                  .Callback(PrepareClientToReturnQueuedItem);
        }

        protected override RemoteEpisode CreateRemoteEpisode()
        {
            var episode = base.CreateRemoteEpisode();

            episode.Release.DownloadUrl = _downloadURL;

            return episode;
        }

        [Test]
        public void Download_with_DestinationDirectory_should_force_directory()
        {
            GivenDestinationDirectory();
            GivenSuccessfulDownload();

            var remoteEpisode = CreateRemoteEpisode();

            Subject.Download(remoteEpisode);

            Mocker.GetMock<IFreeboxDownloadProxy>()
                  .Verify(v => v.AddTaskFromUrl(It.IsAny<string>(), _encodedDestinationDirectory, It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<double?>(), It.IsAny<FreeboxDownloadSettings>()), Times.Once());
        }

        [Test]
        public void Download_with_Category_should_force_directory()
        {
            GivenDownloadConfiguration();
            GivenCategory();
            GivenSuccessfulDownload();

            var remoteEpisode = CreateRemoteEpisode();

            Subject.Download(remoteEpisode);

            Mocker.GetMock<IFreeboxDownloadProxy>()
                  .Verify(v => v.AddTaskFromUrl(It.IsAny<string>(), _encodedDefaultDestinationAndCategory, It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<double?>(), It.IsAny<FreeboxDownloadSettings>()), Times.Once());
        }

        [Test]
        public void Download_without_DestinationDirectory_and_Category_should_use_default()
        {
            GivenDownloadConfiguration();
            GivenSuccessfulDownload();

            var remoteEpisode = CreateRemoteEpisode();

            Subject.Download(remoteEpisode);

            Mocker.GetMock<IFreeboxDownloadProxy>()
                  .Verify(v => v.AddTaskFromUrl(It.IsAny<string>(), _encodedDefaultDestination, It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<double?>(), It.IsAny<FreeboxDownloadSettings>()), Times.Once());
        }

        [TestCase(false, false)]
        [TestCase(true, true)]
        public void Download_should_pause_torrent_as_expected(bool addPausedSetting, bool toBePausedFlag)
        {
            _settings.AddPaused = addPausedSetting;

            GivenDownloadConfiguration();
            GivenSuccessfulDownload();

            var remoteEpisode = CreateRemoteEpisode();

            Subject.Download(remoteEpisode);

            Mocker.GetMock<IFreeboxDownloadProxy>()
                  .Verify(v => v.AddTaskFromUrl(It.IsAny<string>(), It.IsAny<string>(), toBePausedFlag, It.IsAny<bool>(), It.IsAny<double?>(), It.IsAny<FreeboxDownloadSettings>()), Times.Once());
        }

        [TestCase(0, (int)FreeboxDownloadPriority.First, (int)FreeboxDownloadPriority.First, true)]
        [TestCase(0, (int)FreeboxDownloadPriority.Last, (int)FreeboxDownloadPriority.First, true)]
        [TestCase(0, (int)FreeboxDownloadPriority.First, (int)FreeboxDownloadPriority.Last, false)]
        [TestCase(0, (int)FreeboxDownloadPriority.Last, (int)FreeboxDownloadPriority.Last, false)]
        [TestCase(15, (int)FreeboxDownloadPriority.First, (int)FreeboxDownloadPriority.First, true)]
        [TestCase(15, (int)FreeboxDownloadPriority.Last, (int)FreeboxDownloadPriority.First, false)]
        [TestCase(15, (int)FreeboxDownloadPriority.First, (int)FreeboxDownloadPriority.Last, true)]
        [TestCase(15, (int)FreeboxDownloadPriority.Last, (int)FreeboxDownloadPriority.Last, false)]
        public void Download_should_queue_torrent_first_as_expected(int ageDay, int olderPriority, int recentPriority, bool toBeQueuedFirstFlag)
        {
            _settings.OlderPriority = olderPriority;
            _settings.RecentPriority = recentPriority;

            GivenDownloadConfiguration();
            GivenSuccessfulDownload();

            var remoteEpisode = CreateRemoteEpisode();

            var episode = new Tv.Episode()
            {
                AirDateUtc = DateTime.UtcNow.Date.AddDays(-ageDay)
            };

            remoteEpisode.Episodes.Add(episode);

            Subject.Download(remoteEpisode);

            Mocker.GetMock<IFreeboxDownloadProxy>()
                  .Verify(v => v.AddTaskFromUrl(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), toBeQueuedFirstFlag, It.IsAny<double?>(), It.IsAny<FreeboxDownloadSettings>()), Times.Once());
        }

        [TestCase(0, 0)]
        [TestCase(1.5, 150)]
        public void Download_should_define_seed_ratio_as_expected(double? providerSeedRatio, double? expectedSeedRatio)
        {
            GivenDownloadConfiguration();
            GivenSuccessfulDownload();

            var remoteEpisode = CreateRemoteEpisode();

            remoteEpisode.SeedConfiguration = new TorrentSeedConfiguration();
            remoteEpisode.SeedConfiguration.Ratio = providerSeedRatio;

            Subject.Download(remoteEpisode);

            Mocker.GetMock<IFreeboxDownloadProxy>()
                  .Verify(v => v.AddTaskFromUrl(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), expectedSeedRatio, It.IsAny<FreeboxDownloadSettings>()), Times.Once());
        }

        [Test]
        public void GetItems_should_return_empty_list_if_no_tasks_available()
        {
            GivenTasks(new List<FreeboxDownloadTask>());

            Subject.GetItems().Should().BeEmpty();
        }

        [Test]
        public void GetItems_should_return_ignore_tasks_of_unknown_type()
        {
            _task.Status = FreeboxDownloadTaskStatus.Done;
            _task.Type = "toto";

            GivenTasks(new List<FreeboxDownloadTask> { _task });

            Subject.GetItems().Should().BeEmpty();
        }

        [Test]
        public void GetItems_when_destinationdirectory_is_set_should_ignore_downloads_in_wrong_folder()
        {
            _settings.DestinationDirectory = @"/some/path/that/will/not/match";

            _task.Status = FreeboxDownloadTaskStatus.Done;

            GivenTasks(new List<FreeboxDownloadTask> { _task });

            Subject.GetItems().Should().BeEmpty();
        }

        [Test]
        public void GetItems_when_category_is_set_should_ignore_downloads_in_wrong_folder()
        {
            _settings.Category = "somecategory";

            _task.Status = FreeboxDownloadTaskStatus.Done;

            GivenTasks(new List<FreeboxDownloadTask> { _task });

            Subject.GetItems().Should().BeEmpty();
        }

        [TestCase(FreeboxDownloadTaskStatus.Downloading, false, false)]
        [TestCase(FreeboxDownloadTaskStatus.Done, true, true)]
        [TestCase(FreeboxDownloadTaskStatus.Seeding, false, false)]
        [TestCase(FreeboxDownloadTaskStatus.Stopped, false, false)]
        public void GetItems_should_return_canBeMoved_and_canBeDeleted_as_expected(FreeboxDownloadTaskStatus apiStatus, bool canMoveFilesExpected, bool canBeRemovedExpected)
        {
            _task.Status = apiStatus;

            GivenTasks(new List<FreeboxDownloadTask>() { _task });

            var items = Subject.GetItems();

            items.Should().HaveCount(1);
            items.First().CanBeRemoved.Should().Be(canBeRemovedExpected);
            items.First().CanMoveFiles.Should().Be(canMoveFilesExpected);
        }

        [TestCase(FreeboxDownloadTaskStatus.Stopped, DownloadItemStatus.Paused)]
        [TestCase(FreeboxDownloadTaskStatus.Stopping, DownloadItemStatus.Paused)]
        [TestCase(FreeboxDownloadTaskStatus.Queued, DownloadItemStatus.Queued)]
        [TestCase(FreeboxDownloadTaskStatus.Starting, DownloadItemStatus.Downloading)]
        [TestCase(FreeboxDownloadTaskStatus.Downloading, DownloadItemStatus.Downloading)]
        [TestCase(FreeboxDownloadTaskStatus.Retry, DownloadItemStatus.Downloading)]
        [TestCase(FreeboxDownloadTaskStatus.Checking, DownloadItemStatus.Downloading)]
        [TestCase(FreeboxDownloadTaskStatus.Error, DownloadItemStatus.Warning)]
        [TestCase(FreeboxDownloadTaskStatus.Seeding, DownloadItemStatus.Completed)]
        [TestCase(FreeboxDownloadTaskStatus.Done, DownloadItemStatus.Completed)]
        [TestCase(FreeboxDownloadTaskStatus.Unknown, DownloadItemStatus.Downloading)]
        public void GetItems_should_return_item_as_downloadItemStatus(FreeboxDownloadTaskStatus apiStatus, DownloadItemStatus expectedItemStatus)
        {
            _task.Status = apiStatus;

            GivenTasks(new List<FreeboxDownloadTask>() { _task });

            var items = Subject.GetItems();

            items.Should().HaveCount(1);
            items.First().Status.Should().Be(expectedItemStatus);
        }

        [Test]
        public void GetItems_should_return_decoded_destination_directory()
        {
            var decodedDownloadDirectory = "/that/the/path";

            _task.Status = FreeboxDownloadTaskStatus.Done;
            _task.DownloadDirectory = "L3RoYXQvdGhlL3BhdGg=";

            GivenTasks(new List<FreeboxDownloadTask> { _task });

            var items = Subject.GetItems();

            items.Should().HaveCount(1);
            items.First().OutputPath.Should().Be(decodedDownloadDirectory);
        }

        [Test]
        public void GetItems_should_return_message_if_tasks_in_error()
        {
            _task.Status = FreeboxDownloadTaskStatus.Error;
            _task.Error = "internal";

            GivenTasks(new List<FreeboxDownloadTask> { _task });

            var items = Subject.GetItems();

            items.Should().HaveCount(1);
            items.First().Message.Should().Be("Internal error.");
            items.First().Status.Should().Be(DownloadItemStatus.Warning);
        }
    }
}
