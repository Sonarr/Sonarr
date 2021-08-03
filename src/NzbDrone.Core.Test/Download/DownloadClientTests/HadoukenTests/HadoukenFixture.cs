using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients.Hadouken;
using NzbDrone.Core.Download.Clients.Hadouken.Models;
using NzbDrone.Core.MediaFiles.TorrentInfo;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Download.DownloadClientTests.HadoukenTests
{
    [TestFixture]
    public class HadoukenFixture : DownloadClientFixtureBase<Hadouken>
    {
        protected HadoukenTorrent _queued;
        protected HadoukenTorrent _downloading;
        protected HadoukenTorrent _failed;
        protected HadoukenTorrent _completed;

        [SetUp]
        public void Setup()
        {
            Subject.Definition = new DownloadClientDefinition();
            Subject.Definition.Settings = new HadoukenSettings();

            _queued = new HadoukenTorrent
            {
                InfoHash = "HASH",
                IsFinished = false,
                State = HadoukenTorrentState.QueuedForChecking,
                Name = _title,
                TotalSize = 1000,
                DownloadedBytes = 0,
                Progress = 0.0,
                SavePath = "somepath",
                Label = "sonarr-tv"
            };

            _downloading = new HadoukenTorrent
            {
                InfoHash = "HASH",
                IsFinished = false,
                State = HadoukenTorrentState.Downloading,
                Name = _title,
                TotalSize = 1000,
                DownloadedBytes = 100,
                Progress = 10.0,
                SavePath = "somepath",
                Label = "sonarr-tv"
            };

            _failed = new HadoukenTorrent
            {
                InfoHash = "HASH",
                IsFinished = false,
                State = HadoukenTorrentState.Downloading,
                Error = "some error",
                Name = _title,
                TotalSize = 1000,
                DownloadedBytes = 100,
                Progress = 10.0,
                SavePath = "somepath",
                Label = "sonarr-tv"
            };

            _completed = new HadoukenTorrent
            {
                InfoHash = "HASH",
                IsFinished = true,
                State = HadoukenTorrentState.Paused,
                Name = _title,
                TotalSize = 1000,
                DownloadedBytes = 1000,
                Progress = 100.0,
                SavePath = "somepath",
                Label = "sonarr-tv"
            };

            Mocker.GetMock<ITorrentFileInfoReader>()
                  .Setup(s => s.GetHashFromTorrentFile(It.IsAny<byte[]>()))
                  .Returns("CBC2F069FE8BB2F544EAE707D75BCD3DE9DCF951");

            Mocker.GetMock<IHttpClient>()
                  .Setup(s => s.Get(It.IsAny<HttpRequest>()))
                  .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), new byte[0]));
        }

        protected void GivenFailedDownload()
        {
            Mocker.GetMock<IHadoukenProxy>()
                .Setup(s => s.AddTorrentUri(It.IsAny<HadoukenSettings>(), It.IsAny<string>()))
                .Throws<InvalidOperationException>();

            Mocker.GetMock<IHadoukenProxy>()
                .Setup(s => s.AddTorrentFile(It.IsAny<HadoukenSettings>(), It.IsAny<byte[]>()))
                .Throws<InvalidOperationException>();
        }

        protected void GivenSuccessfulDownload()
        {
            Mocker.GetMock<IHttpClient>()
                  .Setup(s => s.Get(It.IsAny<HttpRequest>()))
                  .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), new byte[1000]));

            Mocker.GetMock<IHadoukenProxy>()
                .Setup(s => s.AddTorrentUri(It.IsAny<HadoukenSettings>(), It.IsAny<string>()))
                .Callback(PrepareClientToReturnQueuedItem);

            Mocker.GetMock<IHadoukenProxy>()
                .Setup(s => s.AddTorrentFile(It.IsAny<HadoukenSettings>(), It.IsAny<byte[]>()))
                .Returns("CBC2F069FE8BB2F544EAE707D75BCD3DE9DCF951".ToLower())
                .Callback(PrepareClientToReturnQueuedItem);
        }

        protected virtual void GivenTorrents(List<HadoukenTorrent> torrents)
        {
            if (torrents == null)
            {
                torrents = new List<HadoukenTorrent>();
            }

            Mocker.GetMock<IHadoukenProxy>()
                .Setup(s => s.GetTorrents(It.IsAny<HadoukenSettings>()))
                .Returns(torrents.ToArray());
        }

        protected void PrepareClientToReturnQueuedItem()
        {
            GivenTorrents(new List<HadoukenTorrent>
                {
                    _queued
                });
        }

        protected void PrepareClientToReturnDownloadingItem()
        {
            GivenTorrents(new List<HadoukenTorrent>
                {
                    _downloading
                });
        }

        protected void PrepareClientToReturnFailedItem()
        {
            GivenTorrents(new List<HadoukenTorrent>
                {
                    _failed
                });
        }

        protected void PrepareClientToReturnCompletedItem()
        {
            GivenTorrents(new List<HadoukenTorrent>
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
        public void should_return_status_with_outputdirs()
        {
            var configItems = new Dictionary<string, object>();

            configItems.Add("bittorrent.defaultSavePath", @"C:\Downloads\Downloading\deluge".AsOsAgnostic());

            Mocker.GetMock<IHadoukenProxy>()
                .Setup(v => v.GetConfig(It.IsAny<HadoukenSettings>()))
                .Returns(configItems);

            var result = Subject.GetStatus();

            result.IsLocalhost.Should().BeTrue();
            result.OutputRootFolders.Should().NotBeNull();
            result.OutputRootFolders.First().Should().Be(@"C:\Downloads\Downloading\deluge".AsOsAgnostic());
        }

        [Test]
        public void GetItems_should_return_torrents_with_DownloadId_uppercase()
        {
            var torrent = new HadoukenTorrent
            {
                InfoHash = "hash",
                IsFinished = true,
                State = HadoukenTorrentState.Paused,
                Name = _title,
                TotalSize = 1000,
                DownloadedBytes = 1000,
                Progress = 100.0,
                SavePath = "somepath",
                Label = "sonarr-tv"
            };

            var torrents = new HadoukenTorrent[] { torrent };
            Mocker.GetMock<IHadoukenProxy>()
                .Setup(v => v.GetTorrents(It.IsAny<HadoukenSettings>()))
                .Returns(torrents);

            var result = Subject.GetItems();
            var downloadItem = result.First();

            downloadItem.DownloadId.Should().Be("HASH");
        }

        [Test]
        public void GetItems_should_ignore_torrents_with_a_different_category()
        {
            var torrent = new HadoukenTorrent
            {
                InfoHash = "hash",
                IsFinished = true,
                State = HadoukenTorrentState.Paused,
                Name = _title,
                TotalSize = 1000,
                DownloadedBytes = 1000,
                Progress = 100.0,
                SavePath = "somepath",
                Label = "sonarr-tv-other"
            };

            var torrents = new HadoukenTorrent[] { torrent };
            Mocker.GetMock<IHadoukenProxy>()
                .Setup(v => v.GetTorrents(It.IsAny<HadoukenSettings>()))
                .Returns(torrents);

            Subject.GetItems().Should().BeEmpty();
        }

        [Test]
        public void Download_from_magnet_link_should_return_hash_uppercase()
        {
            var remoteEpisode = CreateRemoteEpisode();

            remoteEpisode.Release.DownloadUrl = "magnet:?xt=urn:btih:a45129e59d8750f9da982f53552b1e4f0457ee9f";

            Mocker.GetMock<IHadoukenProxy>()
               .Setup(v => v.AddTorrentUri(It.IsAny<HadoukenSettings>(), It.IsAny<string>()));

            var result = Subject.Download(remoteEpisode);

            Assert.IsFalse(result.Any(c => char.IsLower(c)));
        }

        [Test]
        public void Download_from_torrent_file_should_return_hash_uppercase()
        {
            var remoteEpisode = CreateRemoteEpisode();

            Mocker.GetMock<IHadoukenProxy>()
               .Setup(v => v.AddTorrentFile(It.IsAny<HadoukenSettings>(), It.IsAny<byte[]>()))
               .Returns("hash");

            var result = Subject.Download(remoteEpisode);

            Assert.IsFalse(result.Any(c => char.IsLower(c)));
        }

        [Test]
        public void Test_should_return_validation_failure_for_old_hadouken()
        {
            var systemInfo = new HadoukenSystemInfo()
            {
                Versions = new Dictionary<string, string>() { { "hadouken", "5.0.0.0" } }
            };

            Mocker.GetMock<IHadoukenProxy>()
               .Setup(v => v.GetSystemInfo(It.IsAny<HadoukenSettings>()))
               .Returns(systemInfo);

            var result = Subject.Test();

            result.Errors.First().ErrorMessage.Should().Be("Old Hadouken client with unsupported API, need 5.1 or higher");
        }
    }
}
