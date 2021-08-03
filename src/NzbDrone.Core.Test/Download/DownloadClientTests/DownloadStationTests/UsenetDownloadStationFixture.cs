using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Http;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients.DownloadStation;
using NzbDrone.Core.Download.Clients.DownloadStation.Proxies;
using NzbDrone.Core.MediaFiles.TorrentInfo;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Download.DownloadClientTests.DownloadStationTests
{
    [TestFixture]
    public class UsenetDownloadStationFixture : DownloadClientFixtureBase<UsenetDownloadStation>
    {
        protected DownloadStationSettings _settings;

        protected DownloadStationTask _queued;
        protected DownloadStationTask _downloading;
        protected DownloadStationTask _failed;
        protected DownloadStationTask _completed;
        protected DownloadStationTask _seeding;

        protected string _serialNumber = "SERIALNUMBER";
        protected string _category = "sonarr";
        protected string _tvDirectory = @"video/Series";
        protected string _defaultDestination = "somepath";
        protected OsPath _physicalPath = new OsPath("/mnt/sdb1/mydata");

        protected RemoteEpisode _remoteEpisode;

        protected Dictionary<string, object> _downloadStationConfigItems;

        [SetUp]
        public void Setup()
        {
            _remoteEpisode = CreateRemoteEpisode();

            _settings = new DownloadStationSettings()
            {
                Host = "127.0.0.1",
                Port = 5000,
                Username = "admin",
                Password = "pass"
            };

            Subject.Definition = new DownloadClientDefinition();
            Subject.Definition.Settings = _settings;

            _queued = new DownloadStationTask()
            {
                Id = "id1",
                Size = 1000,
                Status = DownloadStationTaskStatus.Waiting,
                Type = DownloadStationTaskType.NZB.ToString(),
                Username = "admin",
                Title = "title",
                Additional = new DownloadStationTaskAdditional
                {
                    Detail = new Dictionary<string, string>
                    {
                        { "destination", "shared/folder" },
                        { "uri", FileNameBuilder.CleanFileName(_remoteEpisode.Release.Title) + ".nzb" }
                    },
                    Transfer = new Dictionary<string, string>
                    {
                        { "size_downloaded", "0" },
                        { "speed_download", "0" }
                    }
                }
            };

            _completed = new DownloadStationTask()
            {
                Id = "id2",
                Size = 1000,
                Status = DownloadStationTaskStatus.Finished,
                Type = DownloadStationTaskType.NZB.ToString(),
                Username = "admin",
                Title = "title",
                Additional = new DownloadStationTaskAdditional
                {
                    Detail = new Dictionary<string, string>
                    {
                        { "destination", "shared/folder" },
                        { "uri", FileNameBuilder.CleanFileName(_remoteEpisode.Release.Title) + ".nzb" }
                    },
                    Transfer = new Dictionary<string, string>
                    {
                        { "size_downloaded", "1000" },
                        { "speed_download", "0" }
                    },
                }
            };

            _seeding = new DownloadStationTask()
            {
                Id = "id2",
                Size = 1000,
                Status = DownloadStationTaskStatus.Seeding,
                Type = DownloadStationTaskType.NZB.ToString(),
                Username = "admin",
                Title = "title",
                Additional = new DownloadStationTaskAdditional
                {
                    Detail = new Dictionary<string, string>
                    {
                        { "destination", "shared/folder" },
                        { "uri", FileNameBuilder.CleanFileName(_remoteEpisode.Release.Title) + ".nzb" }
                    },
                    Transfer = new Dictionary<string, string>
                    {
                        { "size_downloaded", "1000" },
                        { "speed_download", "0" }
                    }
                }
            };

            _downloading = new DownloadStationTask()
            {
                Id = "id3",
                Size = 1000,
                Status = DownloadStationTaskStatus.Downloading,
                Type = DownloadStationTaskType.NZB.ToString(),
                Username = "admin",
                Title = "title",
                Additional = new DownloadStationTaskAdditional
                {
                    Detail = new Dictionary<string, string>
                    {
                        { "destination", "shared/folder" },
                        { "uri", FileNameBuilder.CleanFileName(_remoteEpisode.Release.Title) + ".nzb" }
                    },
                    Transfer = new Dictionary<string, string>
                    {
                        { "size_downloaded", "100" },
                        { "speed_download", "50" }
                    }
                }
            };

            _failed = new DownloadStationTask()
            {
                Id = "id4",
                Size = 1000,
                Status = DownloadStationTaskStatus.Error,
                Type = DownloadStationTaskType.NZB.ToString(),
                Username = "admin",
                Title = "title",
                Additional = new DownloadStationTaskAdditional
                {
                    Detail = new Dictionary<string, string>
                    {
                        { "destination", "shared/folder" },
                        { "uri", FileNameBuilder.CleanFileName(_remoteEpisode.Release.Title) + ".nzb" }
                    },
                    Transfer = new Dictionary<string, string>
                    {
                        { "size_downloaded", "10" },
                        { "speed_download", "0" }
                    }
                }
            };

            Mocker.GetMock<IHttpClient>()
                  .Setup(s => s.Get(It.IsAny<HttpRequest>()))
                  .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), new byte[0]));

            _downloadStationConfigItems = new Dictionary<string, object>
            {
                { "default_destination", _defaultDestination },
            };

            Mocker.GetMock<IDownloadStationInfoProxy>()
              .Setup(v => v.GetConfig(It.IsAny<DownloadStationSettings>()))
              .Returns(_downloadStationConfigItems);

            Mocker.GetMock<IDownloadStationTaskProxySelector>()
                  .Setup(s => s.GetProxy(It.IsAny<DownloadStationSettings>()))
                  .Returns(Mocker.GetMock<IDownloadStationTaskProxy>().Object);
        }

        protected void GivenSharedFolder()
        {
            Mocker.GetMock<ISharedFolderResolver>()
                  .Setup(s => s.RemapToFullPath(It.IsAny<OsPath>(), It.IsAny<DownloadStationSettings>(), It.IsAny<string>()))
                  .Returns<OsPath, DownloadStationSettings, string>((path, setttings, serial) => _physicalPath);
        }

        protected void GivenSerialNumber()
        {
            Mocker.GetMock<ISerialNumberProvider>()
                .Setup(s => s.GetSerialNumber(It.IsAny<DownloadStationSettings>()))
                .Returns(_serialNumber);
        }

        protected void GivenTvCategory()
        {
            _settings.TvCategory = _category;
        }

        protected void GivenTvDirectory()
        {
            _settings.TvDirectory = _tvDirectory;
        }

        protected virtual void GivenTasks(List<DownloadStationTask> nzbs)
        {
            if (nzbs == null)
            {
                nzbs = new List<DownloadStationTask>();
            }

            Mocker.GetMock<IDownloadStationTaskProxy>()
                  .Setup(s => s.GetTasks(It.IsAny<DownloadStationSettings>()))
                  .Returns(nzbs);
        }

        protected void PrepareClientToReturnQueuedItem()
        {
            GivenTasks(new List<DownloadStationTask>
            {
                _queued
            });
        }

        protected void GivenSuccessfulDownload()
        {/*
            Mocker.GetMock<IHttpClient>()
                  .Setup(s => s.Get(It.IsAny<HttpRequest>()))
                  .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), new byte[1000]));
            */
            Mocker.GetMock<IDownloadStationTaskProxy>()
                  .Setup(s => s.AddTaskFromData(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DownloadStationSettings>()))
                  .Callback(PrepareClientToReturnQueuedItem);
        }

        protected void GivenAllKindOfTasks()
        {
            var tasks = new List<DownloadStationTask>() { _queued, _completed, _failed, _downloading, _seeding };

            Mocker.GetMock<IDownloadStationTaskProxy>()
                  .Setup(d => d.GetTasks(_settings))
                  .Returns(tasks);
        }

        [Test]
        public void Download_with_TvDirectory_should_force_directory()
        {
            GivenSerialNumber();
            GivenTvDirectory();
            GivenSuccessfulDownload();

            var remoteEpisode = CreateRemoteEpisode();

            var id = Subject.Download(remoteEpisode);

            id.Should().NotBeNullOrEmpty();

            Mocker.GetMock<IDownloadStationTaskProxy>()
                  .Verify(v => v.AddTaskFromData(It.IsAny<byte[]>(), It.IsAny<string>(), _tvDirectory, It.IsAny<DownloadStationSettings>()), Times.Once());
        }

        [Test]
        public void Download_with_category_should_force_directory()
        {
            GivenSerialNumber();
            GivenTvCategory();
            GivenSuccessfulDownload();

            var remoteEpisode = CreateRemoteEpisode();

            var id = Subject.Download(remoteEpisode);

            id.Should().NotBeNullOrEmpty();

            Mocker.GetMock<IDownloadStationTaskProxy>()
                  .Verify(v => v.AddTaskFromData(It.IsAny<byte[]>(), It.IsAny<string>(), $"{_defaultDestination}/{_category}", It.IsAny<DownloadStationSettings>()), Times.Once());
        }

        [Test]
        public void Download_without_TvDirectory_and_Category_should_use_default()
        {
            GivenSerialNumber();
            GivenSuccessfulDownload();

            var remoteEpisode = CreateRemoteEpisode();

            var id = Subject.Download(remoteEpisode);

            id.Should().NotBeNullOrEmpty();

            Mocker.GetMock<IDownloadStationTaskProxy>()
                  .Verify(v => v.AddTaskFromData(It.IsAny<byte[]>(), It.IsAny<string>(), _defaultDestination, It.IsAny<DownloadStationSettings>()), Times.Once());
        }

        [Test]
        public void GetItems_should_return_empty_list_if_no_tasks_available()
        {
            GivenSerialNumber();
            GivenSharedFolder();
            GivenTasks(new List<DownloadStationTask>());

            Subject.GetItems().Should().BeEmpty();
        }

        [Test]
        public void GetItems_should_return_ignore_tasks_of_unknown_type()
        {
            GivenSerialNumber();
            GivenSharedFolder();
            GivenTasks(new List<DownloadStationTask> { _completed });

            _completed.Type = "ipfs";

            Subject.GetItems().Should().BeEmpty();
        }

        [Test]
        public void GetItems_should_ignore_downloads_in_wrong_folder()
        {
            _settings.TvDirectory = @"/shared/folder/sub";

            GivenSerialNumber();
            GivenSharedFolder();
            GivenTasks(new List<DownloadStationTask> { _completed });

            Subject.GetItems().Should().BeEmpty();
        }

        [Test]
        public void GetItems_should_throw_if_shared_folder_resolve_fails()
        {
            Mocker.GetMock<ISharedFolderResolver>()
                  .Setup(s => s.RemapToFullPath(It.IsAny<OsPath>(), It.IsAny<DownloadStationSettings>(), It.IsAny<string>()))
                  .Throws(new ApplicationException("Some unknown exception, HttpException or DownloadClientException"));

            GivenSerialNumber();
            GivenAllKindOfTasks();

            Assert.Throws(Is.InstanceOf<Exception>(), () => Subject.GetItems());
            ExceptionVerification.ExpectedErrors(0);
        }

        [Test]
        public void GetItems_should_throw_if_serial_number_unavailable()
        {
            Mocker.GetMock<ISerialNumberProvider>()
                  .Setup(s => s.GetSerialNumber(_settings))
                  .Throws(new ApplicationException("Some unknown exception, HttpException or DownloadClientException"));

            GivenSharedFolder();
            GivenAllKindOfTasks();

            Assert.Throws(Is.InstanceOf<Exception>(), () => Subject.GetItems());
            ExceptionVerification.ExpectedErrors(0);
        }

        [Test]
        public void Download_should_throw_and_not_add_task_if_cannot_get_serial_number()
        {
            var remoteEpisode = CreateRemoteEpisode();

            Mocker.GetMock<ISerialNumberProvider>()
                  .Setup(s => s.GetSerialNumber(_settings))
                  .Throws(new ApplicationException("Some unknown exception, HttpException or DownloadClientException"));

            Assert.Throws(Is.InstanceOf<Exception>(), () => Subject.Download(remoteEpisode));

            Mocker.GetMock<IDownloadStationTaskProxy>()
                  .Verify(v => v.AddTaskFromUrl(It.IsAny<string>(), null, _settings), Times.Never());
        }

        [Test]
        public void GetItems_should_not_map_outputpath_for_queued_or_downloading_tasks()
        {
            GivenSerialNumber();
            GivenSharedFolder();

            GivenTasks(new List<DownloadStationTask>
            {
                _queued, _downloading
            });

            var items = Subject.GetItems();

            items.Should().HaveCount(2);
            items.Should().OnlyContain(v => v.OutputPath.IsEmpty);
        }

        [Test]
        public void GetItems_should_map_outputpath_for_completed_or_failed_tasks()
        {
            GivenSerialNumber();
            GivenSharedFolder();

            GivenTasks(new List<DownloadStationTask>
            {
                _completed, _failed, _seeding
            });

            var items = Subject.GetItems();

            items.Should().HaveCount(3);
            items.Should().OnlyContain(v => !v.OutputPath.IsEmpty);
        }

        [TestCase(DownloadStationTaskStatus.Downloading, DownloadItemStatus.Downloading)]
        [TestCase(DownloadStationTaskStatus.Error, DownloadItemStatus.Failed)]
        [TestCase(DownloadStationTaskStatus.Extracting, DownloadItemStatus.Downloading)]
        [TestCase(DownloadStationTaskStatus.Finished, DownloadItemStatus.Completed)]
        [TestCase(DownloadStationTaskStatus.Finishing, DownloadItemStatus.Downloading)]
        [TestCase(DownloadStationTaskStatus.HashChecking, DownloadItemStatus.Downloading)]
        [TestCase(DownloadStationTaskStatus.CaptchaNeeded, DownloadItemStatus.Downloading)]
        [TestCase(DownloadStationTaskStatus.Paused, DownloadItemStatus.Paused)]
        [TestCase(DownloadStationTaskStatus.Seeding, DownloadItemStatus.Completed)]
        [TestCase(DownloadStationTaskStatus.FilehostingWaiting, DownloadItemStatus.Queued)]
        [TestCase(DownloadStationTaskStatus.Waiting, DownloadItemStatus.Queued)]
        [TestCase(DownloadStationTaskStatus.Unknown, DownloadItemStatus.Queued)]
        public void GetItems_should_return_item_as_downloadItemStatus(DownloadStationTaskStatus apiStatus, DownloadItemStatus expectedItemStatus)
        {
            GivenSerialNumber();
            GivenSharedFolder();

            _queued.Status = apiStatus;

            GivenTasks(new List<DownloadStationTask>() { _queued });

            var items = Subject.GetItems();
            items.Should().HaveCount(1);

            items.First().Status.Should().Be(expectedItemStatus);
        }
    }
}
