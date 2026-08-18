using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Blocklisting;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.MediaFiles.TorrentInfo;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Download.DownloadClientTests.TorrentClientBaseTests
{
    [TestFixture]
    public class TorrentClientBaseFixture : DownloadClientFixtureBase<TestTorrentClient>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<ITorrentFileInfoReader>()
                  .Setup(s => s.GetHashFromTorrentFile(It.IsAny<byte[]>()))
                  .Returns("HASH");
        }

        private IIndexer CreateIndexerWithFailDownloads(params FailDownloads[] failDownloads)
        {
            var indexer = CreateIndexer();

            indexer.Definition = new IndexerDefinition
            {
                Settings = new TestTorrentIndexerSettings
                {
                    RejectTorrentFilesWithBlockedExtensionsWhileGrabbing = true,
                    FailDownloads = failDownloads.Cast<int>().ToList()
                }
            };

            return indexer;
        }

        private void GivenTorrentFiles(params string[] fileNames)
        {
            Mocker.GetMock<ITorrentFileInfoReader>()
                  .Setup(s => s.GetFileNamesFromTorrentFile(It.IsAny<byte[]>()))
                  .Returns(new List<string>(fileNames));
        }

        [Test]
        public void should_blocklist_and_throw_when_torrent_contains_dangerous_file()
        {
            var remoteEpisode = CreateRemoteEpisode();
            var indexer = CreateIndexerWithFailDownloads(FailDownloads.Executables, FailDownloads.PotentiallyDangerous);

            GivenTorrentFiles("Droned.S01E01.Pilot.1080p.WEB-DL-DRONE.exe");

            Assert.ThrowsAsync<ReleaseBlockedException>(async () => await Subject.Download(remoteEpisode, indexer));

            Mocker.GetMock<IBlocklistService>()
                  .Verify(s => s.Block(remoteEpisode, It.IsAny<string>(), It.IsAny<string>()), Times.Once());

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_blocklist_and_throw_when_torrent_contains_dangerous_file_with_magnet_fallback()
        {
            Subject.SetPreferTorrentFile(true);

            var remoteEpisode = CreateRemoteEpisode();
            var indexer = CreateIndexerWithFailDownloads(FailDownloads.Executables);

            GivenTorrentFiles("Droned.S01E01.Pilot.1080p.WEB-DL-DRONE.exe");

            Assert.ThrowsAsync<ReleaseBlockedException>(async () => await Subject.Download(remoteEpisode, indexer));

            Mocker.GetMock<IBlocklistService>()
                  .Verify(s => s.Block(remoteEpisode, It.IsAny<string>(), It.IsAny<string>()), Times.Once());

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public async Task should_not_check_torrent_files_when_fail_downloads_is_not_enabled()
        {
            var remoteEpisode = CreateRemoteEpisode();

            await Subject.Download(remoteEpisode, CreateIndexer());

            Mocker.GetMock<ITorrentFileInfoReader>()
                  .Verify(s => s.GetFileNamesFromTorrentFile(It.IsAny<byte[]>()), Times.Never());
        }

        [Test]
        public async Task should_not_check_torrent_files_when_reject_setting_is_disabled()
        {
            var remoteEpisode = CreateRemoteEpisode();
            var indexer = CreateIndexer();

            indexer.Definition = new IndexerDefinition
            {
                Settings = new TestTorrentIndexerSettings
                {
                    RejectTorrentFilesWithBlockedExtensionsWhileGrabbing = false,
                    FailDownloads = new List<int> { (int)FailDownloads.Executables }
                }
            };

            await Subject.Download(remoteEpisode, indexer);

            Mocker.GetMock<ITorrentFileInfoReader>()
                  .Verify(s => s.GetFileNamesFromTorrentFile(It.IsAny<byte[]>()), Times.Never());
        }

        [Test]
        public async Task should_not_blocklist_when_torrent_contains_only_safe_files()
        {
            var remoteEpisode = CreateRemoteEpisode();
            var indexer = CreateIndexerWithFailDownloads(FailDownloads.Executables, FailDownloads.PotentiallyDangerous);

            GivenTorrentFiles("Droned.S01E01.Pilot.1080p.WEB-DL-DRONE.mkv", "Sample/sample.mkv");

            await Subject.Download(remoteEpisode, indexer);

            Mocker.GetMock<IBlocklistService>()
                  .Verify(s => s.Block(It.IsAny<RemoteEpisode>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }
    }
}
