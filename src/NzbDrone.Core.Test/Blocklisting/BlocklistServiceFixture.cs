using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Blocklisting;
using NzbDrone.Core.Download;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Blocklisting
{
    [TestFixture]
    public class BlocklistServiceFixture : CoreTest<BlocklistService>
    {
        private DownloadFailedEvent _nzbEvent;
        private DownloadFailedEvent _torrentEvent;

        [SetUp]
        public void Setup()
        {
            _nzbEvent = new DownloadFailedEvent
            {
                SeriesId = 12345,
                EpisodeIds = new List<int> { 1 },
                Quality = new QualityModel(Quality.Bluray720p),
                SourceTitle = "series.title.s01e01",
                DownloadClient = "SabnzbdClient",
                DownloadId = "Sabnzbd_nzo_2dfh73k"
            };

            _nzbEvent.Data.Add("publishedDate", DateTime.UtcNow.ToString("s") + "Z");
            _nzbEvent.Data.Add("size", "1000");
            _nzbEvent.Data.Add("indexer", "nzbs.org");
            _nzbEvent.Data.Add("protocol", "1");
            _nzbEvent.Data.Add("message", "Marked as failed");

            _torrentEvent = new DownloadFailedEvent
            {
                SeriesId = 12345,
                EpisodeIds = new List<int> { 1 },
                Quality = new QualityModel(Quality.Bluray720p),
                SourceTitle = "series.title.s01e01",
                DownloadClient = "TorrentClient",
                DownloadId = "12345678910"
            };

            _torrentEvent.Data.Add("publishedDate", DateTime.UtcNow.ToString("s") + "Z");
            _torrentEvent.Data.Add("size", "1000");
            _torrentEvent.Data.Add("indexer", "torrents.org");
            _torrentEvent.Data.Add("protocol", "2");
            _torrentEvent.Data.Add("message", "Marked as failed");
        }

        [Test]
        public void should_add_to_repository()
        {
            Subject.Handle(_nzbEvent);

            Mocker.GetMock<IBlocklistRepository>()
                .Verify(v => v.Insert(It.Is<Blocklist>(b => b.EpisodeIds == _nzbEvent.EpisodeIds)), Times.Once());
        }

        [Test]
        public void should_add_to_repository_missing_size_and_protocol()
        {
            Subject.Handle(_nzbEvent);

            _nzbEvent.Data.Remove("size");
            _nzbEvent.Data.Remove("protocol");

            Mocker.GetMock<IBlocklistRepository>()
                .Verify(v => v.Insert(It.Is<Blocklist>(b => b.EpisodeIds == _nzbEvent.EpisodeIds)), Times.Once());
        }

        [Test]
        public void should_blocklist_nzb_with_same_indexer_same_size_different_publish_date()
        {
            Subject.Handle(_nzbEvent);

            Mocker.GetMock<IBlocklistRepository>()
                .Verify(v => v.Insert(It.Is<Blocklist>(b => b.Size == 1000)), Times.Once());
        }

        [Test]
        public void should_blocklist_nzb_with_same_publish_date()
        {
            Subject.Handle(_nzbEvent);

            Mocker.GetMock<IBlocklistRepository>()
                .Verify(v => v.Insert(It.Is<Blocklist>(b => b.Size == 1000)), Times.Once());
        }

        [Test]
        public void should_not_blocklist_nzb_with_different_publish_date()
        {
            Subject.Handle(_nzbEvent);

            Mocker.GetMock<IBlocklistRepository>()
                .Verify(v => v.Insert(It.Is<Blocklist>(b => b.Size == 1000)), Times.Once());
        }

        [Test]
        public void should_not_blocklist_nzb_with_different_indexer_same_size()
        {
            Subject.Handle(_nzbEvent);

            Mocker.GetMock<IBlocklistRepository>()
                .Verify(v => v.Insert(It.Is<Blocklist>(b => b.Size == 1000)), Times.Once());
        }

        [Test]
        public void should_not_blocklist_torrent_with_same_hash_with_different_indexer()
        {
            Subject.Handle(_nzbEvent);

            Mocker.GetMock<IBlocklistRepository>()
                .Verify(v => v.Insert(It.Is<Blocklist>(b => b.Size == 1000)), Times.Once());
        }

        [Test]
        public void should_blocklist_hashed_torrent_with_same_hash_with_same_indexer()
        {
            Subject.Handle(_nzbEvent);

            Mocker.GetMock<IBlocklistRepository>()
                .Verify(v => v.Insert(It.Is<Blocklist>(b => b.Size == 1000)), Times.Once());
        }

        [Test]
        public void should_blocklist_same_named_torrent_with_same_indexer()
        {
            Subject.Handle(_nzbEvent);

            Mocker.GetMock<IBlocklistRepository>()
                .Verify(v => v.Insert(It.Is<Blocklist>(b => b.Size == 1000)), Times.Once());
        }

        [Test]
        public void should_not_blocklist_same_named_torrent_with_different_indexer()
        {
            Subject.Handle(_torrentEvent);

            Mocker.GetMock<IBlocklistRepository>()
                .Verify(v => v.Insert(It.Is<Blocklist>(b => b.Size == 1000)), Times.Once());
        }

        [Test]
        public void should_not_blocklist_same_hashed_torrent_with_different_indexer()
        {
            Subject.Handle(_nzbEvent);

            Mocker.GetMock<IBlocklistRepository>()
                .Verify(v => v.Insert(It.Is<Blocklist>(b => b.Size == 1000)), Times.Once());
        }
    }
}
