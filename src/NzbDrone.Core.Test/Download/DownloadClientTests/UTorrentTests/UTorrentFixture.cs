using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients.UTorrent;
using NzbDrone.Core.MediaFiles.TorrentInfo;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Download.DownloadClientTests.UTorrentTests
{
    [TestFixture]
    public class UTorrentFixture : DownloadClientFixtureBase<UTorrent>
    {
        protected UTorrentTorrent _queued;
        protected UTorrentTorrent _downloading;
        protected UTorrentTorrent _failed;
        protected UTorrentTorrent _completed;

        [SetUp]
        public void Setup()
        {
            Subject.Definition = new DownloadClientDefinition();
            Subject.Definition.Settings = new UTorrentSettings
                                          {
                                              Host = "127.0.0.1",
                                              Port = 2222,
                                              Username = "admin",
                                              Password = "pass",
                                              TvCategory = "tv"
                                          };

            _queued = new UTorrentTorrent
                    {
                        Hash = "HASH",
                        Status = UTorrentTorrentStatus.Queued | UTorrentTorrentStatus.Loaded,
                        Name = _title,
                        Size = 1000,
                        Remaining = 1000,
                        Progress = 0,
                        Label = "tv",
                        DownloadUrl = _downloadUrl,
                        RootDownloadPath = "somepath"
                    };

            _downloading = new UTorrentTorrent
                    {
                        Hash = "HASH",
                        Status = UTorrentTorrentStatus.Started | UTorrentTorrentStatus.Loaded,
                        Name = _title,
                        Size = 1000,
                        Remaining = 100,
                        Progress = 0.9,
                        Label = "tv",
                        DownloadUrl = _downloadUrl,
                        RootDownloadPath = "somepath"
                    };

            _failed = new UTorrentTorrent
                    {
                        Hash = "HASH",
                        Status = UTorrentTorrentStatus.Error,
                        Name = _title,
                        Size = 1000,
                        Remaining = 100,
                        Progress = 0.9,
                        Label = "tv",
                        DownloadUrl = _downloadUrl,
                        RootDownloadPath = "somepath"
                    };

            _completed = new UTorrentTorrent
                    {
                        Hash = "HASH",
                        Status = UTorrentTorrentStatus.Checked | UTorrentTorrentStatus.Loaded,
                        Name = _title,
                        Size = 1000,
                        Remaining = 0,
                        Progress = 1.0,
                        Label = "tv",
                        DownloadUrl = _downloadUrl,
                        RootDownloadPath = "somepath"
                    };

            Mocker.GetMock<ITorrentFileInfoReader>()
                  .Setup(s => s.GetHashFromTorrentFile(It.IsAny<byte[]>()))
                  .Returns("CBC2F069FE8BB2F544EAE707D75BCD3DE9DCF951");

            Mocker.GetMock<IHttpClient>()
                  .Setup(s => s.Get(It.IsAny<HttpRequest>()))
                  .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), new byte[0]));
        }

        protected void GivenRedirectToMagnet()
        {
            var httpHeader = new HttpHeader();
            httpHeader["Location"] = "magnet:?xt=urn:btih:ZPBPA2P6ROZPKRHK44D5OW6NHXU5Z6KR&tr=udp";

            Mocker.GetMock<IHttpClient>()
                  .Setup(s => s.Get(It.IsAny<HttpRequest>()))
                  .Returns<HttpRequest>(r => new HttpResponse(r, httpHeader, new byte[0], System.Net.HttpStatusCode.SeeOther));
        }

        protected void GivenRedirectToTorrent()
        {
            var httpHeader = new HttpHeader();
            httpHeader["Location"] = "http://test.sonarr.tv/not-a-real-torrent.torrent";

            Mocker.GetMock<IHttpClient>()
                  .Setup(s => s.Get(It.Is<HttpRequest>(h => h.Url.ToString() == _downloadUrl)))
                  .Returns<HttpRequest>(r => new HttpResponse(r, httpHeader, new byte[0], System.Net.HttpStatusCode.Found));
        }

        protected void GivenFailedDownload()
        {
            Mocker.GetMock<IUTorrentProxy>()
                .Setup(s => s.AddTorrentFromUrl(It.IsAny<string>(), It.IsAny<UTorrentSettings>()))
                .Throws<InvalidOperationException>();
        }

        protected void GivenSuccessfulDownload()
        {
            Mocker.GetMock<IUTorrentProxy>()
                .Setup(s => s.AddTorrentFromUrl(It.IsAny<string>(), It.IsAny<UTorrentSettings>()))
                .Callback(() =>
                {
                    PrepareClientToReturnQueuedItem();
                });
        }

        protected virtual void GivenTorrents(List<UTorrentTorrent> torrents, string cacheNumber = null)
        {
            if (torrents == null)
            {
                torrents = new List<UTorrentTorrent>();
            }

            Mocker.GetMock<IUTorrentProxy>()
                .Setup(s => s.GetTorrents(It.IsAny<string>(), It.IsAny<UTorrentSettings>()))
                .Returns(new UTorrentResponse { Torrents = torrents, CacheNumber = cacheNumber });
        }

        protected virtual void GivenDifferentialTorrents(string oldCacheNumber, List<UTorrentTorrent> changed, List<string> removed, string cacheNumber)
        {
            if (changed == null)
            {
                changed = new List<UTorrentTorrent>();
            }

            if (removed == null)
            {
                removed = new List<string>();
            }

            Mocker.GetMock<IUTorrentProxy>()
                .Setup(s => s.GetTorrents(oldCacheNumber, It.IsAny<UTorrentSettings>()))
                .Returns(new UTorrentResponse { TorrentsChanged = changed, TorrentsRemoved = removed, CacheNumber = cacheNumber });
        }

        protected void PrepareClientToReturnQueuedItem()
        {
            GivenTorrents(new List<UTorrentTorrent>
                {
                    _queued
                });
        }

        protected void PrepareClientToReturnDownloadingItem()
        {
            GivenTorrents(new List<UTorrentTorrent>
                {
                    _downloading
                });
        }

        protected void PrepareClientToReturnFailedItem()
        {
            GivenTorrents(new List<UTorrentTorrent>
                {
                    _failed
                });
        }

        protected void PrepareClientToReturnCompletedItem()
        {
            GivenTorrents(new List<UTorrentTorrent>
                {
                    _completed
                });
        }

        [Test]
        public void queued_item_should_have_required_properties()
        {
            PrepareClientToReturnQueuedItem();
            var item = Subject.GetItems().Single();
            VerifyQueued(item);
        }

        [Test]
        public void downloading_item_should_have_required_properties()
        {
            PrepareClientToReturnDownloadingItem();
            var item = Subject.GetItems().Single();
            VerifyDownloading(item);
        }

        [Test]
        public void failed_item_should_have_required_properties()
        {
            PrepareClientToReturnFailedItem();
            var item = Subject.GetItems().Single();
            VerifyWarning(item);
        }

        [Test]
        public void completed_download_should_have_required_properties()
        {
            PrepareClientToReturnCompletedItem();
            var item = Subject.GetItems().Single();
            VerifyCompleted(item);

            item.CanBeRemoved.Should().BeTrue();
            item.CanMoveFiles.Should().BeTrue();
        }

        [Test]
        public void Download_should_return_unique_id()
        {
            GivenSuccessfulDownload();

            var remoteEpisode = CreateRemoteEpisode();

            var id = Subject.Download(remoteEpisode);

            id.Should().NotBeNullOrEmpty();
        }

        [Test]
        public void GetItems_should_ignore_downloads_from_other_categories()
        {
            _completed.Label = "myowncat";
            PrepareClientToReturnCompletedItem();

            var items = Subject.GetItems();

            items.Should().BeEmpty();
        }

        // Proxy.GetTorrents does not return original url. So item has to be found via magnet url.
        [TestCase("magnet:?xt=urn:btih:ZPBPA2P6ROZPKRHK44D5OW6NHXU5Z6KR&tr=udp", "CBC2F069FE8BB2F544EAE707D75BCD3DE9DCF951")]
        public void Download_should_get_hash_from_magnet_url(string magnetUrl, string expectedHash)
        {
            GivenSuccessfulDownload();

            var remoteEpisode = CreateRemoteEpisode();
            remoteEpisode.Release.DownloadUrl = magnetUrl;

            var id = Subject.Download(remoteEpisode);

            id.Should().Be(expectedHash);
        }

        [TestCase(UTorrentTorrentStatus.Loaded, DownloadItemStatus.Queued)]
        [TestCase(UTorrentTorrentStatus.Loaded | UTorrentTorrentStatus.Checking, DownloadItemStatus.Queued)]
        [TestCase(UTorrentTorrentStatus.Loaded | UTorrentTorrentStatus.Queued, DownloadItemStatus.Queued)]
        [TestCase(UTorrentTorrentStatus.Loaded | UTorrentTorrentStatus.Started, DownloadItemStatus.Downloading)]
        [TestCase(UTorrentTorrentStatus.Loaded | UTorrentTorrentStatus.Queued | UTorrentTorrentStatus.Started, DownloadItemStatus.Downloading)]
        public void GetItems_should_return_queued_item_as_downloadItemStatus(UTorrentTorrentStatus apiStatus, DownloadItemStatus expectedItemStatus)
        {
            _queued.Status = apiStatus;

            PrepareClientToReturnQueuedItem();

            var item = Subject.GetItems().Single();

            item.Status.Should().Be(expectedItemStatus);
        }

        [TestCase(UTorrentTorrentStatus.Loaded | UTorrentTorrentStatus.Checking, DownloadItemStatus.Queued)]
        [TestCase(UTorrentTorrentStatus.Loaded | UTorrentTorrentStatus.Checked | UTorrentTorrentStatus.Queued, DownloadItemStatus.Queued)]
        [TestCase(UTorrentTorrentStatus.Loaded | UTorrentTorrentStatus.Started, DownloadItemStatus.Downloading)]
        [TestCase(UTorrentTorrentStatus.Loaded | UTorrentTorrentStatus.Queued | UTorrentTorrentStatus.Started, DownloadItemStatus.Downloading)]
        public void GetItems_should_return_downloading_item_as_downloadItemStatus(UTorrentTorrentStatus apiStatus, DownloadItemStatus expectedItemStatus)
        {
            _downloading.Status = apiStatus;

            PrepareClientToReturnDownloadingItem();

            var item = Subject.GetItems().Single();

            item.Status.Should().Be(expectedItemStatus);
        }

        [TestCase(UTorrentTorrentStatus.Loaded | UTorrentTorrentStatus.Checking, DownloadItemStatus.Queued, true)]
        [TestCase(UTorrentTorrentStatus.Loaded | UTorrentTorrentStatus.Checked, DownloadItemStatus.Completed, true)]
        [TestCase(UTorrentTorrentStatus.Loaded | UTorrentTorrentStatus.Checked | UTorrentTorrentStatus.Queued, DownloadItemStatus.Completed, false)]
        [TestCase(UTorrentTorrentStatus.Loaded | UTorrentTorrentStatus.Checked | UTorrentTorrentStatus.Started, DownloadItemStatus.Completed, false)]
        [TestCase(UTorrentTorrentStatus.Loaded | UTorrentTorrentStatus.Checked | UTorrentTorrentStatus.Queued | UTorrentTorrentStatus.Paused, DownloadItemStatus.Completed, false)]
        public void GetItems_should_return_completed_item_as_downloadItemStatus(UTorrentTorrentStatus apiStatus, DownloadItemStatus expectedItemStatus, bool expectedValue)
        {
            _completed.Status = apiStatus;

            PrepareClientToReturnCompletedItem();

            var item = Subject.GetItems().Single();

            item.Status.Should().Be(expectedItemStatus);
            item.CanBeRemoved.Should().Be(expectedValue);
            item.CanMoveFiles.Should().Be(expectedValue);
        }

        [Test]
        public void should_return_status_with_outputdirs()
        {
            var configItems = new Dictionary<string, string>();

            configItems.Add("dir_active_download_flag", "true");
            configItems.Add("dir_active_download", @"C:\Downloads\Downloading\utorrent".AsOsAgnostic());
            configItems.Add("dir_completed_download", @"C:\Downloads\Finished\utorrent".AsOsAgnostic());
            configItems.Add("dir_completed_download_flag", "true");
            configItems.Add("dir_add_label", "true");

            Mocker.GetMock<IUTorrentProxy>()
                .Setup(v => v.GetConfig(It.IsAny<UTorrentSettings>()))
                .Returns(configItems);

            var result = Subject.GetStatus();

            result.IsLocalhost.Should().BeTrue();
            result.OutputRootFolders.Should().NotBeNull();
            result.OutputRootFolders.First().Should().Be(@"C:\Downloads\Finished\utorrent\tv".AsOsAgnostic());
        }

        [Test]
        public void should_combine_drive_letter()
        {
            WindowsOnly();

            _completed.RootDownloadPath = "D:";

            PrepareClientToReturnCompletedItem();

            var item = Subject.GetItems().Single();

            item.OutputPath.Should().Be(@"D:\" + _title);
        }

        [Test]
        public void Download_should_handle_http_redirect_to_magnet()
        {
            GivenRedirectToMagnet();
            GivenSuccessfulDownload();

            var remoteEpisode = CreateRemoteEpisode();

            var id = Subject.Download(remoteEpisode);

            id.Should().NotBeNullOrEmpty();
        }

        [Test]
        public void Download_should_handle_http_redirect_to_torrent()
        {
            GivenRedirectToTorrent();
            GivenSuccessfulDownload();

            var remoteEpisode = CreateRemoteEpisode();

            var id = Subject.Download(remoteEpisode);

            id.Should().NotBeNullOrEmpty();
        }

        [Test]
        public void GetItems_should_query_with_cache_id_if_available()
        {
            _downloading.Status = UTorrentTorrentStatus.Started;

            GivenTorrents(new List<UTorrentTorrent> { _downloading }, "abc");

            var item1 = Subject.GetItems().Single();

            Mocker.GetMock<IUTorrentProxy>().Verify(v => v.GetTorrents(null, It.IsAny<UTorrentSettings>()), Times.Once());

            GivenTorrents(new List<UTorrentTorrent> { _downloading, _queued }, "abc2");
            GivenDifferentialTorrents("abc", new List<UTorrentTorrent> { _queued }, new List<string>(), "abc2");
            GivenDifferentialTorrents("abc2", new List<UTorrentTorrent>(), new List<string>(), "abc2");

            var item2 = Subject.GetItems().Single();

            Mocker.GetMock<IUTorrentProxy>().Verify(v => v.GetTorrents("abc", It.IsAny<UTorrentSettings>()), Times.Once());

            var item3 = Subject.GetItems().Single();

            Mocker.GetMock<IUTorrentProxy>().Verify(v => v.GetTorrents("abc2", It.IsAny<UTorrentSettings>()), Times.Once());
        }
    }
}
