using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Common.Http;
using NzbDrone.Core.MediaFiles.TorrentInfo;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients.Deluge;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Test.Common;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Download.DownloadClientTests.DelugeTests
{
    [TestFixture]
    public class DelugeFixture : DownloadClientFixtureBase<Deluge>
    {
        protected DelugeTorrent _queued;
        protected DelugeTorrent _downloading;
        protected DelugeTorrent _failed;
        protected DelugeTorrent _completed;

        [SetUp]
        public void Setup()
        {
            Subject.Definition = new DownloadClientDefinition();
            Subject.Definition.Settings = new DelugeSettings()
            {
                TvCategory = null
            };

            _queued = new DelugeTorrent
                    {
                        Hash = "HASH",
                        IsFinished = false,
                        State = DelugeTorrentStatus.Queued,
                        Name = _title,
                        Size = 1000,
                        BytesDownloaded = 0,
                        Progress = 0.0,
                        DownloadPath = "somepath"
                    };

            _downloading = new DelugeTorrent
                    {
                        Hash = "HASH",
                        IsFinished = false,
                        State = DelugeTorrentStatus.Downloading,
                        Name = _title,
                        Size = 1000,
                        BytesDownloaded = 100,
                        Progress = 10.0,
                        DownloadPath = "somepath"
                    };

            _failed = new DelugeTorrent
                    {
                        Hash = "HASH",
                        IsFinished = false,
                        State = DelugeTorrentStatus.Error,
                        Name = _title,
                        Size = 1000,
                        BytesDownloaded = 100,
                        Progress = 10.0,
                        Message = "Error",
                        DownloadPath = "somepath"
                    };

            _completed = new DelugeTorrent
                    {
                        Hash = "HASH",
                        IsFinished = true,
                        State = DelugeTorrentStatus.Paused,
                        Name = _title,
                        Size = 1000,
                        BytesDownloaded = 1000,
                        Progress = 100.0,
                        DownloadPath = "somepath"
                    };

            Mocker.GetMock<ITorrentFileInfoReader>()
                  .Setup(s => s.GetHashFromTorrentFile(It.IsAny<Byte[]>()))
                  .Returns("CBC2F069FE8BB2F544EAE707D75BCD3DE9DCF951");

            Mocker.GetMock<IHttpClient>()
                  .Setup(s => s.Get(It.IsAny<HttpRequest>()))
                  .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), new Byte[0]));
        }

        protected void GivenFailedDownload()
        {
            Mocker.GetMock<IDelugeProxy>()
                .Setup(s => s.AddTorrentFromMagnet(It.IsAny<String>(), It.IsAny<DelugeSettings>()))
                .Throws<InvalidOperationException>();

            Mocker.GetMock<IDelugeProxy>()
                .Setup(s => s.AddTorrentFromFile(It.IsAny<String>(), It.IsAny<Byte[]>(), It.IsAny<DelugeSettings>()))
                .Throws<InvalidOperationException>();
        }

        protected void GivenSuccessfulDownload()
        {
            Mocker.GetMock<IHttpClient>()
                  .Setup(s => s.Get(It.IsAny<HttpRequest>()))
                  .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), new Byte[1000]));

            Mocker.GetMock<IDelugeProxy>()
                .Setup(s => s.AddTorrentFromMagnet(It.IsAny<String>(), It.IsAny<DelugeSettings>()))
                .Returns("CBC2F069FE8BB2F544EAE707D75BCD3DE9DCF951".ToLower())
                .Callback(PrepareClientToReturnQueuedItem);

            Mocker.GetMock<IDelugeProxy>()
                .Setup(s => s.AddTorrentFromFile(It.IsAny<String>(), It.IsAny<Byte[]>(), It.IsAny<DelugeSettings>()))
                .Returns("CBC2F069FE8BB2F544EAE707D75BCD3DE9DCF951".ToLower())
                .Callback(PrepareClientToReturnQueuedItem);
        }
        
        protected virtual void GivenTorrents(List<DelugeTorrent> torrents)
        {
            if (torrents == null)
            {
                torrents = new List<DelugeTorrent>();
            }

            Mocker.GetMock<IDelugeProxy>()
                .Setup(s => s.GetTorrents(It.IsAny<DelugeSettings>()))
                .Returns(torrents.ToArray());
        }

        protected void PrepareClientToReturnQueuedItem()
        {
            GivenTorrents(new List<DelugeTorrent> 
                {
                    _queued
                });
        }

        protected void PrepareClientToReturnDownloadingItem()
        {
            GivenTorrents(new List<DelugeTorrent> 
                {
                    _downloading
                });
        }

        protected void PrepareClientToReturnFailedItem()
        {
            GivenTorrents(new List<DelugeTorrent> 
                {
                    _failed
                });
        }

        protected void PrepareClientToReturnCompletedItem()
        {
            GivenTorrents(new List<DelugeTorrent>
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
            VerifyFailed(item);
        }

        [Test]
        public void completed_download_should_have_required_properties()
        {
            PrepareClientToReturnCompletedItem();
            var item = Subject.GetItems().Single();
            VerifyCompleted(item);
        }

        [Test]
        public void Download_should_return_unique_id()
        {
            GivenSuccessfulDownload();

            var remoteEpisode = CreateRemoteEpisode();

            var id = Subject.Download(remoteEpisode);

            id.Should().NotBeNullOrEmpty();
        }

        [TestCase("magnet:?xt=urn:btih:ZPBPA2P6ROZPKRHK44D5OW6NHXU5Z6KR&tr=udp", "CBC2F069FE8BB2F544EAE707D75BCD3DE9DCF951")]
        public void Download_should_get_hash_from_magnet_url(String magnetUrl, String expectedHash)
        {
            GivenSuccessfulDownload();

            var remoteEpisode = CreateRemoteEpisode();
            remoteEpisode.Release.DownloadUrl = magnetUrl;

            var id = Subject.Download(remoteEpisode);

            id.Should().Be(expectedHash);
        }

        [TestCase(DelugeTorrentStatus.Paused, DownloadItemStatus.Paused)]
        [TestCase(DelugeTorrentStatus.Checking, DownloadItemStatus.Downloading)]
        [TestCase(DelugeTorrentStatus.Queued, DownloadItemStatus.Queued)]
        [TestCase(DelugeTorrentStatus.Downloading, DownloadItemStatus.Downloading)]
        [TestCase(DelugeTorrentStatus.Seeding, DownloadItemStatus.Downloading)]
        public void GetItems_should_return_queued_item_as_downloadItemStatus(String apiStatus, DownloadItemStatus expectedItemStatus)
        {
            _queued.State = apiStatus;

            PrepareClientToReturnQueuedItem();

            var item = Subject.GetItems().Single();

            item.Status.Should().Be(expectedItemStatus);
        }

        [TestCase(DelugeTorrentStatus.Paused, DownloadItemStatus.Paused)]
        [TestCase(DelugeTorrentStatus.Checking, DownloadItemStatus.Downloading)]
        [TestCase(DelugeTorrentStatus.Queued, DownloadItemStatus.Queued)]
        [TestCase(DelugeTorrentStatus.Downloading, DownloadItemStatus.Downloading)]
        [TestCase(DelugeTorrentStatus.Seeding, DownloadItemStatus.Downloading)]
        public void GetItems_should_return_downloading_item_as_downloadItemStatus(String apiStatus, DownloadItemStatus expectedItemStatus)
        {
            _downloading.State = apiStatus;

            PrepareClientToReturnDownloadingItem();

            var item = Subject.GetItems().Single();

            item.Status.Should().Be(expectedItemStatus);
        }

        [TestCase(DelugeTorrentStatus.Paused, DownloadItemStatus.Completed, true)]
        [TestCase(DelugeTorrentStatus.Checking, DownloadItemStatus.Downloading, true)]
        [TestCase(DelugeTorrentStatus.Queued, DownloadItemStatus.Completed, true)]
        [TestCase(DelugeTorrentStatus.Seeding, DownloadItemStatus.Completed, true)]
        public void GetItems_should_return_completed_item_as_downloadItemStatus(String apiStatus, DownloadItemStatus expectedItemStatus, Boolean expectedReadOnly)
        {
            _completed.State = apiStatus;

            PrepareClientToReturnCompletedItem();

            var item = Subject.GetItems().Single();

            item.Status.Should().Be(expectedItemStatus);
            item.IsReadOnly.Should().Be(expectedReadOnly);
        }

        [Test]
        public void GetItems_should_check_share_ratio_for_readonly()
        {
            _completed.State = DelugeTorrentStatus.Paused;
            _completed.IsAutoManaged = true;
            _completed.StopAtRatio = true;
            _completed.StopRatio = 1.0;
            _completed.Ratio = 1.01;

            PrepareClientToReturnCompletedItem();

            var item = Subject.GetItems().Single();

            item.Status.Should().Be(DownloadItemStatus.Completed);
            item.IsReadOnly.Should().BeFalse();
        }

        [Test]
        public void should_return_status_with_outputdirs()
        {
            var configItems = new Dictionary<String, Object>();

            configItems.Add("download_location", @"C:\Downloads\Downloading\deluge".AsOsAgnostic());
            configItems.Add("move_completed_path", @"C:\Downloads\Finished\deluge".AsOsAgnostic());
            configItems.Add("move_completed", true);

            Mocker.GetMock<IDelugeProxy>()
                .Setup(v => v.GetConfig(It.IsAny<DelugeSettings>()))
                .Returns(configItems);

            var result = Subject.GetStatus();

            result.IsLocalhost.Should().BeTrue();
            result.OutputRootFolders.Should().NotBeNull();
            result.OutputRootFolders.First().Should().Be(@"C:\Downloads\Finished\deluge".AsOsAgnostic());
        }
    }
}
