using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients.RTorrent;
using NzbDrone.Core.MediaFiles.TorrentInfo;

namespace NzbDrone.Core.Test.Download.DownloadClientTests.RTorrentTests
{
    [TestFixture]
    public class RTorrentFixture : DownloadClientFixtureBase<RTorrent>
    {
        protected RTorrentTorrent _downloading;
        protected RTorrentTorrent _completed;

        [SetUp]
        public void Setup()
        {
            Subject.Definition = new DownloadClientDefinition();
            Subject.Definition.Settings = new RTorrentSettings()
            {
                TvCategory = null
            };

            _downloading = new RTorrentTorrent
                    {
                        Hash = "HASH",
                        IsFinished = false,
                        IsOpen = true,
                        IsActive = true,
                        Name = _title,
                        TotalSize = 1000,
                        RemainingSize = 500,
                        Path = "somepath"
                    };

            _completed = new RTorrentTorrent
                    {
                        Hash = "HASH",
                        IsFinished = true,
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
            Mocker.GetMock<IRTorrentProxy>()
                  .Setup(s => s.AddTorrentFromUrl(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<RTorrentPriority>(), It.IsAny<string>(), It.IsAny<RTorrentSettings>()))
                  .Callback(PrepareClientToReturnCompletedItem);

            Mocker.GetMock<IRTorrentProxy>()
                  .Setup(s => s.AddTorrentFromFile(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<RTorrentPriority>(), It.IsAny<string>(), It.IsAny<RTorrentSettings>()))
                  .Callback(PrepareClientToReturnCompletedItem);

            Mocker.GetMock<IRTorrentProxy>()
                  .Setup(s => s.HasHashTorrent(It.IsAny<string>(), It.IsAny<RTorrentSettings>()))
                  .Returns(true);
        }

        protected virtual void GivenTorrents(List<RTorrentTorrent> torrents)
        {
            if (torrents == null)
            {
                torrents = new List<RTorrentTorrent>();
            }

            Mocker.GetMock<IRTorrentProxy>()
                .Setup(s => s.GetTorrents(It.IsAny<RTorrentSettings>()))
                .Returns(torrents);
        }

        protected void PrepareClientToReturnDownloadingItem()
        {
            GivenTorrents(new List<RTorrentTorrent>
                {
                    _downloading
                });
        }

        protected void PrepareClientToReturnCompletedItem()
        {
            GivenTorrents(new List<RTorrentTorrent>
                {
                    _completed
                });
        }

        [Test]
        public void downloading_item_should_have_required_properties()
        {
            PrepareClientToReturnDownloadingItem();
            var item = Subject.GetItems().Single();
            VerifyDownloading(item);
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
    }
}
