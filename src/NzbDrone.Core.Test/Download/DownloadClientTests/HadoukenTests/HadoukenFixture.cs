using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients.Hadouken;
using NzbDrone.Core.Download.Clients.Hadouken.Models;
using NzbDrone.Core.MediaFiles.TorrentInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
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
                InfoHash= "HASH",
                IsFinished = false,
                State = HadoukenTorrentState.QueuedForChecking,
                Name = _title,
                TotalSize = 1000,
                DownloadedBytes = 0,
                Progress = 0.0,
                SavePath = "somepath"
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
                SavePath = "somepath"
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
                SavePath= "somepath"
            };

            _completed = new HadoukenTorrent
            {
                InfoHash = "HASH",
                IsFinished = true,
                State = HadoukenTorrentState.Downloading,
                IsPaused = true,
                Name = _title,
                TotalSize = 1000,
                DownloadedBytes = 1000,
                Progress = 100.0,
                SavePath = "somepath"
            };

            Mocker.GetMock<ITorrentFileInfoReader>()
                  .Setup(s => s.GetHashFromTorrentFile(It.IsAny<Byte[]>()))
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
                .Returns(torrents.ToDictionary(k => k.InfoHash));
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
            var configItems = new Dictionary<String, Object>();

            configItems.Add("bittorrent.defaultSavePath", @"C:\Downloads\Downloading\deluge".AsOsAgnostic());

            Mocker.GetMock<IHadoukenProxy>()
                .Setup(v => v.GetConfig(It.IsAny<HadoukenSettings>()))
                .Returns(configItems);

            var result = Subject.GetStatus();

            result.IsLocalhost.Should().BeTrue();
            result.OutputRootFolders.Should().NotBeNull();
            result.OutputRootFolders.First().Should().Be(@"C:\Downloads\Downloading\deluge".AsOsAgnostic());
        }
    }
}
