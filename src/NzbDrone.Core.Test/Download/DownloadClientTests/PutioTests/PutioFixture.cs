using System;
using System.Linq;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.MediaFiles.TorrentInfo;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients.Putio;

namespace NzbDrone.Core.Test.Download.DownloadClientTests.PutioTests
{
    [TestFixture]
    public class PutioFixture : DownloadClientFixtureBase<Putio>
    {
        protected PutioSettings _settings;
        protected PutioTorrent _queued;
        protected PutioTorrent _downloading;
        protected PutioTorrent _failed;
        protected PutioTorrent _completed;
        protected PutioTorrent _magnet;
        protected Dictionary<string, object> _PutioAccountSettingsItems;

        [SetUp]
        public void Setup()
        {
            _settings = new PutioSettings
            {
            };

            Subject.Definition = new DownloadClientDefinition();
            Subject.Definition.Settings = _settings;

            _queued = new PutioTorrent
                    {
                        HashString = "HASH",
                        IsFinished = false,
                        Status = PutioTorrentStatus.Queued,
                        Name = _title,
                        TotalSize = 1000,
                        LeftUntilDone = 1000,
                        DownloadDir = "somepath"
                    };

            _downloading = new PutioTorrent
                {
                    HashString = "HASH",
                    IsFinished = false,
                    Status = PutioTorrentStatus.Downloading,
                    Name = _title,
                    TotalSize = 1000,
                    LeftUntilDone = 100,
                    DownloadDir = "somepath"
                };

            _failed = new PutioTorrent
                    {
                        HashString = "HASH",
                        IsFinished = false,
                        Status = PutioTorrentStatus.Stopped,
                        Name = _title,
                        TotalSize = 1000,
                        LeftUntilDone = 100,
                        ErrorString = "Error",
                        DownloadDir = "somepath"
                    };

            _completed = new PutioTorrent
                    {
                        HashString = "HASH",
                        IsFinished = true,
                        Status = PutioTorrentStatus.Stopped,
                        Name = _title,
                        TotalSize = 1000,
                        LeftUntilDone = 0,
                        DownloadDir = "somepath"
                    };

            _magnet = new PutioTorrent
            {
                HashString = "HASH",
                IsFinished = false,
                Status = PutioTorrentStatus.Downloading,
                Name = _title,
                TotalSize = 0,
                LeftUntilDone = 100,
                DownloadDir = "somepath"
            };

            Mocker.GetMock<ITorrentFileInfoReader>()
                  .Setup(s => s.GetHashFromTorrentFile(It.IsAny<byte[]>()))
                  .Returns("CBC2F069FE8BB2F544EAE707D75BCD3DE9DCF951");

            Mocker.GetMock<IHttpClient>()
                  .Setup(s => s.Get(It.IsAny<HttpRequest>()))
                  .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), new byte[0]));

            _PutioAccountSettingsItems = new Dictionary<string, object>();

            _PutioAccountSettingsItems.Add("download-dir", @"C:/Downloads/Finished/Putio");
            _PutioAccountSettingsItems.Add("incomplete-dir", null);
            _PutioAccountSettingsItems.Add("incomplete-dir-enabled", false);

            Mocker.GetMock<IPutioProxy>()
                .Setup(v => v.GetAccountSettings(It.IsAny<PutioSettings>()))
                .Returns(_PutioAccountSettingsItems);

        }

        protected void GivenFailedDownload()
        {
            Mocker.GetMock<IPutioProxy>()
                .Setup(s => s.AddTorrentFromUrl(It.IsAny<string>(), It.IsAny<PutioSettings>()))
                .Throws<InvalidOperationException>();
        }

        protected void GivenSuccessfulDownload()
        {
            Mocker.GetMock<IHttpClient>()
                  .Setup(s => s.Get(It.IsAny<HttpRequest>()))
                  .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), new byte[1000]));

            Mocker.GetMock<IPutioProxy>()
                .Setup(s => s.AddTorrentFromUrl(It.IsAny<string>(), It.IsAny<PutioSettings>()))
                .Callback(PrepareClientToReturnQueuedItem);

            Mocker.GetMock<IPutioProxy>()
                .Setup(s => s.AddTorrentFromData(It.IsAny<byte[]>(), It.IsAny<PutioSettings>()))
                .Callback(PrepareClientToReturnQueuedItem);
        }
        
        protected virtual void GivenTorrents(List<PutioTorrent> torrents)
        {
            if (torrents == null)
            {
                torrents = new List<PutioTorrent>();
            }

            Mocker.GetMock<IPutioProxy>()
                .Setup(s => s.GetTorrents(It.IsAny<PutioSettings>()))
                .Returns(torrents);
        }

        protected void PrepareClientToReturnQueuedItem()
        {
            GivenTorrents(new List<PutioTorrent> 
                {
                    _queued
                });
        }

        protected void PrepareClientToReturnDownloadingItem()
        {
            GivenTorrents(new List<PutioTorrent> 
                {
                    _downloading
                });
        }

        protected void PrepareClientToReturnFailedItem()
        {
            GivenTorrents(new List<PutioTorrent> 
                {
                    _failed
                });
        }

        protected void PrepareClientToReturnCompletedItem()
        {
            GivenTorrents(new List<PutioTorrent>
                {
                    _completed
                });
        }

        protected void PrepareClientToReturnMagnetItem()
        {
            GivenTorrents(new List<PutioTorrent>
                {
                    _magnet
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
        public void magnet_download_should_not_return_the_item()
        {
            PrepareClientToReturnMagnetItem();
            Subject.GetItems().Count().Should().Be(0);
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
        public void Download_should_get_hash_from_magnet_url(string magnetUrl, string expectedHash)
        {
            GivenSuccessfulDownload();

            var remoteEpisode = CreateRemoteEpisode();
            remoteEpisode.Release.DownloadUrl = magnetUrl;

            var id = Subject.Download(remoteEpisode);

            id.Should().Be(expectedHash);
        }

        [TestCase(PutioTorrentStatus.Stopped, DownloadItemStatus.Downloading)]
        [TestCase(PutioTorrentStatus.CheckWait, DownloadItemStatus.Downloading)]
        [TestCase(PutioTorrentStatus.Check, DownloadItemStatus.Downloading)]
        [TestCase(PutioTorrentStatus.Queued, DownloadItemStatus.Queued)]
        [TestCase(PutioTorrentStatus.Downloading, DownloadItemStatus.Downloading)]
        [TestCase(PutioTorrentStatus.SeedingWait, DownloadItemStatus.Completed)]
        [TestCase(PutioTorrentStatus.Seeding, DownloadItemStatus.Completed)]
        public void GetItems_should_return_queued_item_as_downloadItemStatus(PutioTorrentStatus apiStatus, DownloadItemStatus expectedItemStatus)
        {
            _queued.Status = apiStatus;

            PrepareClientToReturnQueuedItem();

            var item = Subject.GetItems().Single();

            item.Status.Should().Be(expectedItemStatus);
        }

        [TestCase(PutioTorrentStatus.Queued, DownloadItemStatus.Queued)]
        [TestCase(PutioTorrentStatus.Downloading, DownloadItemStatus.Downloading)]
        [TestCase(PutioTorrentStatus.Seeding, DownloadItemStatus.Completed)]
        public void GetItems_should_return_downloading_item_as_downloadItemStatus(PutioTorrentStatus apiStatus, DownloadItemStatus expectedItemStatus)
        {
            _downloading.Status = apiStatus;

            PrepareClientToReturnDownloadingItem();

            var item = Subject.GetItems().Single();

            item.Status.Should().Be(expectedItemStatus);
        }

        [TestCase(PutioTorrentStatus.Stopped, DownloadItemStatus.Completed, false)]
        [TestCase(PutioTorrentStatus.CheckWait, DownloadItemStatus.Downloading, true)]
        [TestCase(PutioTorrentStatus.Check, DownloadItemStatus.Downloading, true)]
        [TestCase(PutioTorrentStatus.Queued, DownloadItemStatus.Completed, true)]
        [TestCase(PutioTorrentStatus.SeedingWait, DownloadItemStatus.Completed, true)]
        [TestCase(PutioTorrentStatus.Seeding, DownloadItemStatus.Completed, true)]
        public void GetItems_should_return_completed_item_as_downloadItemStatus(PutioTorrentStatus apiStatus, DownloadItemStatus expectedItemStatus, bool expectedReadOnly)
        {
            _completed.Status = apiStatus;
            
            PrepareClientToReturnCompletedItem();

            var item = Subject.GetItems().Single();

            item.Status.Should().Be(expectedItemStatus);
            item.IsReadOnly.Should().Be(expectedReadOnly);
        }

        [Test]
        public void should_return_status_with_outputdirs()
        {
            var result = Subject.GetStatus();

            result.IsLocalhost.Should().BeTrue();
            result.OutputRootFolders.Should().NotBeNull();
            result.OutputRootFolders.First().Should().Be(@"C:\Downloads\Finished\Putio");
        }

        [Test]
        public void should_fix_forward_slashes()
        {
            WindowsOnly();

            _downloading.DownloadDir = @"C:/Downloads/Finished/Putio";

            GivenTorrents(new List<PutioTorrent> 
                {
                    _downloading
                });

            var items = Subject.GetItems().ToList();

            items.Should().HaveCount(1);
            items.First().OutputPath.Should().Be(@"C:\Downloads\Finished\Putio\" + _title);
        }

        [TestCase(-1)] // Infinite/Unknown
        [TestCase(-2)] // Magnet Downloading
        public void should_ignore_negative_eta(int eta)
        {
            _completed.Eta = eta;

            PrepareClientToReturnCompletedItem();
            var item = Subject.GetItems().Single();
            item.RemainingTime.Should().NotHaveValue();
        }
    }
}
