using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Blocklisting;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.Blocklisting
{
    [TestFixture]
    public class BlocklistServiceFixture : CoreTest<BlocklistService>
    {
        private DownloadFailedEvent _event;

        [SetUp]
        public void Setup()
        {
            _event = new DownloadFailedEvent
                     {
                         SeriesId = 12345,
                         EpisodeIds = new List<int> { 1 },
                         Quality = new QualityModel(Quality.Bluray720p),
                         SourceTitle = "series.title.s01e01",
                         DownloadClient = "SabnzbdClient",
                         DownloadId = "Sabnzbd_nzo_2dfh73k"
                     };

            _event.Data.Add("publishedDate", DateTime.UtcNow.ToString("s") + "Z");
            _event.Data.Add("size", "1000");
            _event.Data.Add("indexer", "nzbs.org");
            _event.Data.Add("protocol", "1");
            _event.Data.Add("message", "Marked as failed");
        }

        [Test]
        public void should_add_to_repository()
        {
            Subject.Handle(_event);

            Mocker.GetMock<IBlocklistRepository>()
                .Verify(v => v.Insert(It.Is<Blocklist>(b => b.EpisodeIds == _event.EpisodeIds)), Times.Once());
        }

        [Test]
        public void should_add_to_repository_missing_size_and_protocol()
        {
            Subject.Handle(_event);

            _event.Data.Remove("size");
            _event.Data.Remove("protocol");

            Mocker.GetMock<IBlocklistRepository>()
                .Verify(v => v.Insert(It.Is<Blocklist>(b => b.EpisodeIds == _event.EpisodeIds)), Times.Once());
        }

        [Test]
        public void should_store_indexer_guid_from_failed_download_history()
        {
            const string indexerGuid = "nzb-guid-123";
            _event.Data.Add("guid", indexerGuid);

            Subject.Handle(_event);

            Mocker.GetMock<IBlocklistRepository>()
                .Verify(v => v.Insert(It.Is<Blocklist>(b => b.IndexerGuid == indexerGuid)), Times.Once());
        }

        [Test]
        public void should_store_indexer_guid_when_release_is_blocked_directly()
        {
            const string indexerGuid = "nzb-guid-456";
            var remoteEpisode = new RemoteEpisode
            {
                Series = new Series { Id = _event.SeriesId },
                Episodes = new List<Episode> { new() { Id = 1 } },
                ParsedEpisodeInfo = new ParsedEpisodeInfo { Quality = _event.Quality },
                Release = GivenUsenetRelease("Indexer A", indexerGuid)
            };

            Subject.Block(remoteEpisode, "Failed", "Test");

            Mocker.GetMock<IBlocklistRepository>()
                .Verify(v => v.Insert(It.Is<Blocklist>(b => b.IndexerGuid == indexerGuid)), Times.Once());
        }

        [Test]
        public void should_block_exact_same_indexer_and_guid_when_guid_retry_is_enabled()
        {
            GivenGuidRetryEnabled();
            var release = GivenUsenetRelease("Indexer A", "nzb-guid-123");
            GivenBlocklistedRelease(release, "Indexer A", "nzb-guid-123");

            Assert.That(Subject.Blocklisted(_event.SeriesId, release), Is.True);
        }

        [Test]
        public void should_allow_different_guid_from_same_indexer_when_guid_retry_is_enabled()
        {
            GivenGuidRetryEnabled();
            var release = GivenUsenetRelease("Indexer A", "working-guid");
            GivenBlocklistedRelease(release, "Indexer A", "failed-guid");

            Assert.That(Subject.Blocklisted(_event.SeriesId, release), Is.False);
        }

        [Test]
        public void should_allow_same_guid_from_different_indexer_when_guid_retry_is_enabled()
        {
            GivenGuidRetryEnabled();
            var release = GivenUsenetRelease("Indexer B", "shared-guid");
            GivenBlocklistedRelease(release, "Indexer A", "shared-guid");

            Assert.That(Subject.Blocklisted(_event.SeriesId, release), Is.False);
        }

        [Test]
        public void should_allow_release_with_guid_when_blocklist_guid_is_missing()
        {
            GivenGuidRetryEnabled();
            var release = GivenUsenetRelease("Indexer B", "nzb-guid-123");
            GivenBlocklistedRelease(release, "Indexer A", null);

            Assert.That(Subject.Blocklisted(_event.SeriesId, release), Is.False);
        }

        [Test]
        public void should_allow_release_without_guid_when_blocklist_guid_is_present()
        {
            GivenGuidRetryEnabled();
            var release = GivenUsenetRelease("Indexer B", null);
            GivenBlocklistedRelease(release, "Indexer A", "nzb-guid-123");

            Assert.That(Subject.Blocklisted(_event.SeriesId, release), Is.False);
        }

        [Test]
        public void should_fall_back_to_standard_matching_when_both_guids_are_missing()
        {
            GivenGuidRetryEnabled();
            var release = GivenUsenetRelease("Indexer B", null);
            GivenBlocklistedRelease(release, "Indexer A", null);

            Assert.That(Subject.Blocklisted(_event.SeriesId, release), Is.True);
        }

        [Test]
        public void should_preserve_cross_indexer_usenet_blocking_when_guid_retry_is_disabled()
        {
            var release = GivenUsenetRelease("Indexer B", "working-guid");
            GivenBlocklistedRelease(release, "Indexer A", "failed-guid");

            Assert.That(Subject.Blocklisted(_event.SeriesId, release), Is.True);
        }

        private void GivenGuidRetryEnabled()
        {
            Mocker.GetMock<IConfigService>()
                .SetupGet(s => s.RetryUsenetReleasesByGuid)
                .Returns(true);
        }

        private ReleaseInfo GivenUsenetRelease(string indexer, string indexerGuid)
        {
            return new ReleaseInfo
            {
                Title = _event.SourceTitle,
                Indexer = indexer,
                Guid = indexerGuid,
                DownloadProtocol = DownloadProtocol.Usenet,
                PublishDate = DateTime.UtcNow,
                Size = 1000
            };
        }

        private void GivenBlocklistedRelease(ReleaseInfo release, string indexer, string indexerGuid)
        {
            Mocker.GetMock<IBlocklistRepository>()
                .Setup(s => s.BlocklistedByTitle(_event.SeriesId, release.Title))
                .Returns(new List<Blocklist>
                {
                    new()
                    {
                        SeriesId = _event.SeriesId,
                        SourceTitle = release.Title,
                        Indexer = indexer,
                        IndexerGuid = indexerGuid,
                        Protocol = DownloadProtocol.Usenet,
                        PublishedDate = release.PublishDate,
                        Size = release.Size
                    }
                });
        }
    }
}
