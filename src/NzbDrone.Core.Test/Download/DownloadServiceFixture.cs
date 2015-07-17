using System.Linq;
using System.Net;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;
using System.Collections.Generic;
using NzbDrone.Core.Indexers;

namespace NzbDrone.Core.Test.Download
{
    [TestFixture]
    public class DownloadServiceFixture : CoreTest<DownloadService>
    {
        private RemoteEpisode _parseResult;
        private List<IDownloadClient> _downloadClients;
        [SetUp]
        public void Setup()
        {
            _downloadClients = new List<IDownloadClient>();

            Mocker.GetMock<IProvideDownloadClient>()
                .Setup(v => v.GetDownloadClients())
                .Returns(_downloadClients);

            Mocker.GetMock<IProvideDownloadClient>()
                .Setup(v => v.GetDownloadClient(It.IsAny<DownloadProtocol>()))
                .Returns<DownloadProtocol>(v => _downloadClients.FirstOrDefault(d => d.Protocol == v));

            var episodes = Builder<Episode>.CreateListOfSize(2)
                .TheFirst(1).With(s => s.Id = 12)
                .TheNext(1).With(s => s.Id = 99)
                .All().With(s => s.SeriesId = 5)
                .Build().ToList();

            var releaseInfo = Builder<ReleaseInfo>.CreateNew()
                .With(v => v.DownloadProtocol = DownloadProtocol.Usenet)
                .With(v => v.DownloadUrl = "http://test.site/download1.ext")
                .Build();

            _parseResult = Builder<RemoteEpisode>.CreateNew()
                   .With(c => c.Series = Builder<Series>.CreateNew().Build())
                   .With(c => c.Release = releaseInfo)
                   .With(c => c.Episodes = episodes)
                   .Build();
        }

        private Mock<IDownloadClient> WithUsenetClient()
        {
            var mock = new Mock<IDownloadClient>(MockBehavior.Default);
            mock.SetupGet(s => s.Definition).Returns(Builder<IndexerDefinition>.CreateNew().Build());

            _downloadClients.Add(mock.Object);

            mock.SetupGet(v => v.Protocol).Returns(DownloadProtocol.Usenet);

            return mock;
        }

        private Mock<IDownloadClient> WithTorrentClient()
        {
            var mock = new Mock<IDownloadClient>(MockBehavior.Default);
            mock.SetupGet(s => s.Definition).Returns(Builder<IndexerDefinition>.CreateNew().Build());

            _downloadClients.Add(mock.Object);

            mock.SetupGet(v => v.Protocol).Returns(DownloadProtocol.Torrent);

            return mock;
        }

        [Test]
        public void Download_report_should_publish_on_grab_event()
        {
            var mock = WithUsenetClient();
            mock.Setup(s => s.Download(It.IsAny<RemoteEpisode>()));
            
            Subject.DownloadReport(_parseResult);

            VerifyEventPublished<EpisodeGrabbedEvent>();
        }

        [Test]
        public void Download_report_should_grab_using_client()
        {
            var mock = WithUsenetClient();
            mock.Setup(s => s.Download(It.IsAny<RemoteEpisode>()));
            
            Subject.DownloadReport(_parseResult);

            mock.Verify(s => s.Download(It.IsAny<RemoteEpisode>()), Times.Once());
        }

        [Test]
        public void Download_report_should_not_publish_on_failed_grab_event()
        {
            var mock = WithUsenetClient();
            mock.Setup(s => s.Download(It.IsAny<RemoteEpisode>()))
                .Throws(new WebException());

            Assert.Throws<WebException>(() => Subject.DownloadReport(_parseResult));

            VerifyEventNotPublished<EpisodeGrabbedEvent>();
        }

        [Test]
        public void should_not_attempt_download_if_client_isnt_configure()
        {
            Subject.DownloadReport(_parseResult);

            Mocker.GetMock<IDownloadClient>().Verify(c => c.Download(It.IsAny<RemoteEpisode>()), Times.Never());
            VerifyEventNotPublished<EpisodeGrabbedEvent>();

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_send_download_to_correct_usenet_client()
        {
            var mockTorrent = WithTorrentClient();
            var mockUsenet = WithUsenetClient();

            Subject.DownloadReport(_parseResult);

            mockTorrent.Verify(c => c.Download(It.IsAny<RemoteEpisode>()), Times.Never());
            mockUsenet.Verify(c => c.Download(It.IsAny<RemoteEpisode>()), Times.Once());
        }

        [Test]
        public void should_send_download_to_correct_torrent_client()
        {
            var mockTorrent = WithTorrentClient();
            var mockUsenet = WithUsenetClient();

            _parseResult.Release.DownloadProtocol = DownloadProtocol.Torrent;

            Subject.DownloadReport(_parseResult);

            mockTorrent.Verify(c => c.Download(It.IsAny<RemoteEpisode>()), Times.Once());
            mockUsenet.Verify(c => c.Download(It.IsAny<RemoteEpisode>()), Times.Never());
        }
    }
}
