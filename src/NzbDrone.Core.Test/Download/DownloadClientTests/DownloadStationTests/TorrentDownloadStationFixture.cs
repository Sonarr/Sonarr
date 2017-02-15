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
using NzbDrone.Core.Parser.Model;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Download.DownloadClientTests.DownloadStationTests
{
    [TestFixture]
    public class TorrentDownloadStationFixture : DownloadClientFixtureBase<TorrentDownloadStation>
    {
        protected DownloadStationSettings _settings;

        protected DownloadStationTorrent _queued;
        protected DownloadStationTorrent _downloading;
        protected DownloadStationTorrent _failed;
        protected DownloadStationTorrent _completed;
        protected DownloadStationTorrent _seeding;
        protected DownloadStationTorrent _magnet;
        protected DownloadStationTorrent _singleFile;
        protected DownloadStationTorrent _multipleFiles;
        protected DownloadStationTorrent _singleFileCompleted;
        protected DownloadStationTorrent _multipleFilesCompleted;

        protected string _serialNumber = "SERIALNUMBER";
        protected string _category = "sonarr";
        protected string _tvDirectory = @"video/Series";
        protected string _defaultDestination = "somepath";
        protected OsPath _physicalPath = new OsPath("/mnt/sdb1/mydata");

        protected Dictionary<string, object> _downloadStationConfigItems;

        protected string DownloadURL => "magnet:?xt=urn:btih:5dee65101db281ac9c46344cd6b175cdcad53426&dn=download";

        [SetUp]
        public void Setup()
        {
            _settings = new DownloadStationSettings()
            {
                Host = "127.0.0.1",
                Port = 5000,
                Username = "admin",
                Password = "pass"
            };

            Subject.Definition = new DownloadClientDefinition();
            Subject.Definition.Settings = _settings;

            _queued = new DownloadStationTorrent()
            {
                Id = "id1",
                Size = 1000,
                Status = DownloadStationTaskStatus.Waiting,
                Type = DownloadStationTaskType.BT,
                Username = "admin",
                Title = "title",
                Additional = new DownloadStationTorrentAdditional
                {
                    Detail = new Dictionary<string, string>
                    {
                        { "destination","shared/folder" },
                        { "uri", DownloadURL }
                    },
                    Transfer = new Dictionary<string, string>
                    {
                        { "size_downloaded", "0"},
                        { "speed_download", "0" }
                    }
                }
            };

            _completed = new DownloadStationTorrent()
            {
                Id = "id2",
                Size = 1000,
                Status = DownloadStationTaskStatus.Finished,
                Type = DownloadStationTaskType.BT,
                Username = "admin",
                Title = "title",
                Additional = new DownloadStationTorrentAdditional
                {
                    Detail = new Dictionary<string, string>
                    {
                        { "destination","shared/folder" },
                        { "uri", DownloadURL }
                    },
                    Transfer = new Dictionary<string, string>
                    {
                        { "size_downloaded", "1000"},
                        { "speed_download", "0" }
                    },
                }
            };

            _seeding = new DownloadStationTorrent()
            {
                Id = "id2",
                Size = 1000,
                Status = DownloadStationTaskStatus.Seeding,
                Type = DownloadStationTaskType.BT,
                Username = "admin",
                Title = "title",
                Additional = new DownloadStationTorrentAdditional
                {
                    Detail = new Dictionary<string, string>
                    {
                        { "destination","shared/folder" },
                        { "uri", DownloadURL }
                    },
                    Transfer = new Dictionary<string, string>
                    {
                        { "size_downloaded", "1000"},
                        { "speed_download", "0" }
                    }
                }
            };

            _downloading = new DownloadStationTorrent()
            {
                Id = "id3",
                Size = 1000,
                Status = DownloadStationTaskStatus.Downloading,
                Type = DownloadStationTaskType.BT,
                Username = "admin",
                Title = "title",
                Additional = new DownloadStationTorrentAdditional
                {
                    Detail = new Dictionary<string, string>
                    {
                        { "destination","shared/folder" },
                        { "uri", DownloadURL }
                    },
                    Transfer = new Dictionary<string, string>
                    {
                        { "size_downloaded", "100"},
                        { "speed_download", "50" }
                    }
                }
            };

            _failed = new DownloadStationTorrent()
            {
                Id = "id4",
                Size = 1000,
                Status = DownloadStationTaskStatus.Error,
                Type = DownloadStationTaskType.BT,
                Username = "admin",
                Title = "title",
                Additional = new DownloadStationTorrentAdditional
                {
                    Detail = new Dictionary<string, string>
                    {
                        { "destination","shared/folder" },
                        { "uri", DownloadURL }
                    },
                    Transfer = new Dictionary<string, string>
                    {
                        { "size_downloaded", "10"},
                        { "speed_download", "0" }
                    }
                }
            };

            _singleFile = new DownloadStationTorrent()
            {
                Id = "id5",
                Size = 1000,
                Status = DownloadStationTaskStatus.Seeding,
                Type = DownloadStationTaskType.BT,
                Username = "admin",
                Title = "a.mkv",
                Additional = new DownloadStationTorrentAdditional
                {
                    Detail = new Dictionary<string, string>
                    {
                        { "destination","shared/folder" },
                        { "uri", DownloadURL }
                    },
                    Transfer = new Dictionary<string, string>
                    {
                        { "size_downloaded", "1000"},
                        { "speed_download", "0" }
                    }
                }
            };

            _multipleFiles = new DownloadStationTorrent()
            {
                Id = "id6",
                Size = 1000,
                Status = DownloadStationTaskStatus.Seeding,
                Type = DownloadStationTaskType.BT,
                Username = "admin",
                Title = "title",
                Additional = new DownloadStationTorrentAdditional
                {
                    Detail = new Dictionary<string, string>
                    {
                        { "destination","shared/folder" },
                        { "uri", DownloadURL }
                    },
                    Transfer = new Dictionary<string, string>
                    {
                        { "size_downloaded", "1000"},
                        { "speed_download", "0" }
                    }
                }
            };

            _singleFileCompleted = new DownloadStationTorrent()
            {
                Id = "id6",
                Size = 1000,
                Status = DownloadStationTaskStatus.Finished,
                Type = DownloadStationTaskType.BT,
                Username = "admin",
                Title = "a.mkv",
                Additional = new DownloadStationTorrentAdditional
                {
                    Detail = new Dictionary<string, string>
                    {
                        { "destination","shared/folder" },
                        { "uri", DownloadURL }
                    },
                    Transfer = new Dictionary<string, string>
                    {
                        { "size_downloaded", "1000"},
                        { "speed_download", "0" }
                    }
                }
            };

            _multipleFilesCompleted = new DownloadStationTorrent()
            {
                Id = "id6",
                Size = 1000,
                Status = DownloadStationTaskStatus.Finished,
                Type = DownloadStationTaskType.BT,
                Username = "admin",
                Title = "title",
                Additional = new DownloadStationTorrentAdditional
                {
                    Detail = new Dictionary<string, string>
                    {
                        { "destination","shared/folder" },
                        { "uri", DownloadURL }
                    },
                    Transfer = new Dictionary<string, string>
                    {
                        { "size_downloaded", "1000"},
                        { "speed_download", "0" }
                    }
                }
            };

            Mocker.GetMock<ITorrentFileInfoReader>()
                  .Setup(s => s.GetHashFromTorrentFile(It.IsAny<byte[]>()))
                  .Returns("CBC2F069FE8BB2F544EAE707D75BCD3DE9DCF951");

            Mocker.GetMock<IHttpClient>()
                  .Setup(s => s.Get(It.IsAny<HttpRequest>()))
                  .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), new byte[0]));

            _downloadStationConfigItems = new Dictionary<string, object>
            {
                { "default_destination", _defaultDestination },
            };

            Mocker.GetMock<IDownloadStationProxy>()
              .Setup(v => v.GetConfig(It.IsAny<DownloadStationSettings>()))
              .Returns(_downloadStationConfigItems);
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

        protected virtual void GivenTorrents(List<DownloadStationTorrent> torrents)
        {
            if (torrents == null)
            {
                torrents = new List<DownloadStationTorrent>();
            }

            Mocker.GetMock<IDownloadStationProxy>()
                  .Setup(s => s.GetTorrents(It.IsAny<DownloadStationSettings>()))
                  .Returns(torrents);
        }

        protected void PrepareClientToReturnQueuedItem()
        {
            GivenTorrents(new List<DownloadStationTorrent>
            {
                _queued
            });
        }

        protected void GivenSuccessfulDownload()
        {
            Mocker.GetMock<IHttpClient>()
                  .Setup(s => s.Get(It.IsAny<HttpRequest>()))
                  .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), new byte[1000]));

            Mocker.GetMock<IDownloadStationProxy>()
                  .Setup(s => s.AddTorrentFromUrl(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DownloadStationSettings>()))
                  .Callback(PrepareClientToReturnQueuedItem);

            Mocker.GetMock<IDownloadStationProxy>()
                  .Setup(s => s.AddTorrentFromData(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DownloadStationSettings>()))
                  .Callback(PrepareClientToReturnQueuedItem);
        }

        protected override RemoteEpisode CreateRemoteEpisode()
        {
            var episode = base.CreateRemoteEpisode();

            episode.Release.DownloadUrl = DownloadURL;

            return episode;
        }

        protected int GivenAllKindOfTasks()
        {
            var tasks = new List<DownloadStationTorrent>() { _queued, _completed, _failed, _downloading, _seeding };

            Mocker.GetMock<IDownloadStationProxy>()
                  .Setup(d => d.GetTorrents(_settings))
                  .Returns(tasks);

            return tasks.Count;
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

            Mocker.GetMock<IDownloadStationProxy>()
                  .Verify(v => v.AddTorrentFromUrl(It.IsAny<string>(), _tvDirectory, It.IsAny<DownloadStationSettings>()), Times.Once());
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

            Mocker.GetMock<IDownloadStationProxy>()
                  .Verify(v => v.AddTorrentFromUrl(It.IsAny<string>(), $"{_defaultDestination}/{_category}", It.IsAny<DownloadStationSettings>()), Times.Once());
        }

        [Test]
        public void Download_without_TvDirectory_and_Category_should_use_default()
        {
            GivenSerialNumber();
            GivenSuccessfulDownload();

            var remoteEpisode = CreateRemoteEpisode();

            var id = Subject.Download(remoteEpisode);

            id.Should().NotBeNullOrEmpty();

            Mocker.GetMock<IDownloadStationProxy>()
                  .Verify(v => v.AddTorrentFromUrl(It.IsAny<string>(), null, It.IsAny<DownloadStationSettings>()), Times.Once());
        }

        [Test]
        public void GetItems_should_ignore_downloads_in_wrong_folder()
        {
            _settings.TvDirectory = @"/shared/folder/sub";

            GivenSerialNumber();
            GivenSharedFolder();
            GivenTorrents(new List<DownloadStationTorrent> { _completed });

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
        public void Download_should_throw_and_not_add_torrent_if_cannot_get_serial_number()
        {
            var remoteEpisode = CreateRemoteEpisode();

            Mocker.GetMock<ISerialNumberProvider>()
                  .Setup(s => s.GetSerialNumber(_settings))
                  .Throws(new ApplicationException("Some unknown exception, HttpException or DownloadClientException"));

            Assert.Throws(Is.InstanceOf<Exception>(), () => Subject.Download(remoteEpisode));

            Mocker.GetMock<IDownloadStationProxy>()
                  .Verify(v => v.AddTorrentFromUrl(It.IsAny<string>(), null, _settings), Times.Never());
        }

        [Test]
        public void GetItems_should_set_outputPath_to_base_folder_when_single_file_non_finished_torrent()
        {
            GivenSerialNumber();
            GivenSharedFolder();

            GivenTorrents(new List<DownloadStationTorrent>() { _singleFile });

            var items = Subject.GetItems();

            items.Should().HaveCount(1);
            items.First().OutputPath.Should().Be(_physicalPath + _singleFile.Title);
        }

        [Test]
        public void GetItems_should_set_outputPath_to_torrent_folder_when_multiple_files_non_finished_torrent()
        {
            GivenSerialNumber();
            GivenSharedFolder();

            GivenTorrents(new List<DownloadStationTorrent>() { _multipleFiles });

            var items = Subject.GetItems();

            items.Should().HaveCount(1);
            items.First().OutputPath.Should().Be(_physicalPath + _multipleFiles.Title);
        }

        [Test]
        public void GetItems_should_set_outputPath_to_base_folder_when_single_file_finished_torrent()
        {
            GivenSerialNumber();
            GivenSharedFolder();

            GivenTorrents(new List<DownloadStationTorrent>() { _singleFileCompleted });

            var items = Subject.GetItems();

            items.Should().HaveCount(1);
            items.First().OutputPath.Should().Be(_physicalPath + _singleFileCompleted.Title);
        }

        [Test]
        public void GetItems_should_set_outputPath_to_torrent_folder_when_multiple_files_finished_torrent()
        {
            GivenSerialNumber();
            GivenSharedFolder();

            GivenTorrents(new List<DownloadStationTorrent>() { _multipleFilesCompleted });

            var items = Subject.GetItems();

            items.Should().HaveCount(1);
            items.First().OutputPath.Should().Be($"{_physicalPath}/{_multipleFiles.Title}");
        }

        [Test]
        public void GetItems_should_not_map_outputpath_for_queued_or_downloading_torrents()
        {
            GivenSerialNumber();
            GivenSharedFolder();

            GivenTorrents(new List<DownloadStationTorrent>
            {
                _queued, _downloading
            });

            var items = Subject.GetItems();

            items.Should().HaveCount(2);
            items.Should().OnlyContain(v => v.OutputPath.IsEmpty);
        }

        [Test]
        public void GetItems_should_map_outputpath_for_completed_or_failed_torrents()
        {
            GivenSerialNumber();
            GivenSharedFolder();

            GivenTorrents(new List<DownloadStationTorrent>
            {
                _completed, _failed, _seeding
            });

            var items = Subject.GetItems();

            items.Should().HaveCount(3);
            items.Should().OnlyContain(v => !v.OutputPath.IsEmpty);
        }

        [TestCase(DownloadStationTaskStatus.Downloading, DownloadItemStatus.Downloading, true)]
        [TestCase(DownloadStationTaskStatus.Finished, DownloadItemStatus.Completed, false)]
        [TestCase(DownloadStationTaskStatus.Seeding, DownloadItemStatus.Completed, true)]
        [TestCase(DownloadStationTaskStatus.Waiting, DownloadItemStatus.Queued, true)]
        public void GetItems_should_return_readonly_expected(DownloadStationTaskStatus apiStatus, DownloadItemStatus expectedItemStatus, bool readOnlyExpected)
        {
            GivenSerialNumber();
            GivenSharedFolder();

            _queued.Status = apiStatus;

            GivenTorrents(new List<DownloadStationTorrent>() { _queued });

            var items = Subject.GetItems();

            items.Should().HaveCount(1);
            items.First().IsReadOnly.Should().Be(readOnlyExpected);
        }

        [TestCase(DownloadStationTaskStatus.Downloading, DownloadItemStatus.Downloading)]
        [TestCase(DownloadStationTaskStatus.Error, DownloadItemStatus.Failed)]
        [TestCase(DownloadStationTaskStatus.Extracting, DownloadItemStatus.Downloading)]
        [TestCase(DownloadStationTaskStatus.Finished, DownloadItemStatus.Completed)]
        [TestCase(DownloadStationTaskStatus.Finishing, DownloadItemStatus.Downloading)]
        [TestCase(DownloadStationTaskStatus.HashChecking, DownloadItemStatus.Downloading)]
        [TestCase(DownloadStationTaskStatus.Paused, DownloadItemStatus.Paused)]
        [TestCase(DownloadStationTaskStatus.Seeding, DownloadItemStatus.Completed)]
        [TestCase(DownloadStationTaskStatus.Waiting, DownloadItemStatus.Queued)]
        public void GetItems_should_return_item_as_downloadItemStatus(DownloadStationTaskStatus apiStatus, DownloadItemStatus expectedItemStatus)
        {
            GivenSerialNumber();
            GivenSharedFolder();

            _queued.Status = apiStatus;

            GivenTorrents(new List<DownloadStationTorrent>() { _queued });

            var items = Subject.GetItems();
            items.Should().HaveCount(1);

            items.First().Status.Should().Be(expectedItemStatus);
        }
    }
}
