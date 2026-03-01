using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients.RQBit;
using NzbDrone.Core.MediaFiles.TorrentInfo;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Download.DownloadClientTests.RQBitTests
{
    [TestFixture]
    public class RQBitFixture : DownloadClientFixtureBase<RQBit>
    {
        protected RQBitTorrent _queued;
        protected RQBitTorrent _downloading;
        protected RQBitTorrent _failed;
        protected RQBitTorrent _completed;

        [SetUp]
        public void Setup()
        {
            Subject.Definition = new DownloadClientDefinition();
            Subject.Definition.Settings = new RQbitSettings
            {
                Host = "127.0.0.1",
                Port = 3030,
                UseSsl = false
            };

            _queued = new RQBitTorrent
            {
                Hash = "HASH",
                IsFinished = false,
                IsActive = false,
                Name = _title,
                TotalSize = 1000,
                RemainingSize = 1000,
                Path = "somepath"
            };

            _downloading = new RQBitTorrent
            {
                Hash = "HASH",
                IsFinished = false,
                IsActive = true,
                Name = _title,
                TotalSize = 1000,
                RemainingSize = 100,
                Path = "somepath"
            };

            _failed = new RQBitTorrent
            {
                Hash = "HASH",
                IsFinished = false,
                IsActive = false,
                Name = _title,
                TotalSize = 1000,
                RemainingSize = 1000,
                Path = "somepath"
            };

            _completed = new RQBitTorrent
            {
                Hash = "HASH",
                IsFinished = true,
                IsActive = false,
                Name = _title,
                TotalSize = 1000,
                RemainingSize = 0,
                Path = "somepath"
            };

            Mocker.GetMock<ITorrentFileInfoReader>()
                  .Setup(s => s.GetHashFromTorrentFile(It.IsAny<byte[]>()))
                  .Returns("CBC2F069FE8BB2F544EAE707D75BCD3DE9DCF951");
        }

        protected void GivenSuccessfulDownload()
        {
            Mocker.GetMock<IRQbitProxy>()
                  .Setup(s => s.AddTorrentFromUrl(It.IsAny<string>(), It.IsAny<RQbitSettings>()))
                  .Returns("CBC2F069FE8BB2F544EAE707D75BCD3DE9DCF951");
            Mocker.GetMock<IRQbitProxy>()
                  .Setup(s => s.AddTorrentFromFile(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<RQbitSettings>()))
                  .Returns("CBC2F069FE8BB2F544EAE707D75BCD3DE9DCF951");
        }

        protected virtual void GivenTorrents(List<RQBitTorrent> torrents)
        {
            if (torrents == null)
            {
                torrents = new List<RQBitTorrent>();
            }

            Mocker.GetMock<IRQbitProxy>()
                  .Setup(s => s.GetTorrents(It.IsAny<RQbitSettings>()))
                  .Returns(torrents);
        }

        protected void PrepareClientToReturnQueuedItem()
        {
            GivenTorrents(new List<RQBitTorrent>
                {
                    _queued
                });
        }

        protected void PrepareClientToReturnDownloadingItem()
        {
            GivenTorrents(new List<RQBitTorrent>
                {
                    _downloading
                });
        }

        protected void PrepareClientToReturnFailedItem()
        {
            GivenTorrents(new List<RQBitTorrent>
                {
                    _failed
                });
        }

        protected void PrepareClientToReturnCompletedItem()
        {
            GivenTorrents(new List<RQBitTorrent>
                {
                    _completed
                });
        }

        [Test]
        public void queued_item_should_have_required_properties()
        {
            PrepareClientToReturnQueuedItem();
            var item = Subject.GetItems().Single();
            VerifyPaused(item);
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
            VerifyPaused(item);
        }

        [Test]
        public void completed_download_should_have_required_properties()
        {
            PrepareClientToReturnCompletedItem();
            var item = Subject.GetItems().Single();
            VerifyCompleted(item);
        }

        [Test]
        public async Task Download_should_return_unique_id()
        {
            GivenSuccessfulDownload();

            var remoteEpisode = CreateRemoteEpisode();

            var id = await Subject.Download(remoteEpisode, CreateIndexer());

            id.Should().NotBeNullOrEmpty();
        }

        [TestCase("magnet:?xt=urn:btih:ZPBPA2P6ROZPKRHK44D5OW6NHXU5Z6KR&tr=udp", "CBC2F069FE8BB2F544EAE707D75BCD3DE9DCF951")]
        public async Task Download_should_get_hash_from_magnet_url(string magnetUrl, string expectedHash)
        {
            GivenSuccessfulDownload();

            var remoteEpisode = CreateRemoteEpisode();
            remoteEpisode.Release.DownloadUrl = magnetUrl;

            var id = await Subject.Download(remoteEpisode, CreateIndexer());

            id.Should().Be(expectedHash);
        }

        [Test]
        public void should_return_status_with_outputdirs()
        {
            var result = Subject.GetStatus();

            result.IsLocalhost.Should().BeTrue();
        }

        [Test]
        public void GetItems_should_ignore_torrents_with_empty_path()
        {
            var torrents = new List<RQBitTorrent>
            {
                new RQBitTorrent { Name = "Test1", Hash = "Hash1", Path = "" },
                new RQBitTorrent { Name = "Test2", Hash = "Hash2", Path = "/valid/path" }
            };

            GivenTorrents(torrents);

            var items = Subject.GetItems();

            items.Should().HaveCount(1);
            items.First().Title.Should().Be("Test2");

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void GetItems_should_ignore_torrents_with_relative_path()
        {
            var torrents = new List<RQBitTorrent>
            {
                new RQBitTorrent { Name = "Test1", Hash = "Hash1", Path = "./relative/path" },
                new RQBitTorrent { Name = "Test2", Hash = "Hash2", Path = "/absolute/path" }
            };

            GivenTorrents(torrents);

            var items = Subject.GetItems();

            items.Should().HaveCount(1);
            items.First().Title.Should().Be("Test2");

            ExceptionVerification.ExpectedWarns(1);
        }
    }
}
