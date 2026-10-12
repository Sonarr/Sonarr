using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Http;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

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
                .Setup(v => v.GetDownloadClients(It.IsAny<DownloadProtocol>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<HashSet<int>>()))
                .Returns<DownloadProtocol, int, bool, HashSet<int>>((v, i, f, t) => _downloadClients.Where(d => d.Protocol == v));

            Mocker.GetMock<IProvideDownloadClient>()
                .Setup(v => v.GetDownloadClient(It.IsAny<DownloadProtocol>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<HashSet<int>>()))
                .Returns<DownloadProtocol, int, bool, HashSet<int>>((v, i, f, t) => _downloadClients.FirstOrDefault(d => d.Protocol == v));

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
            mock.SetupGet(s => s.Definition).Returns(Builder<DownloadClientDefinition>.CreateNew()
                .With(d => d.Id = _downloadClients.Count + 1)
                .Build());

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
        public async Task Download_report_should_publish_on_grab_event()
        {
            var mock = WithUsenetClient();
            mock.Setup(s => s.Download(It.IsAny<RemoteEpisode>(), It.IsAny<IIndexer>()));

            await Subject.DownloadReport(_parseResult, null);

            VerifyEventPublished<EpisodeGrabbedEvent>();
        }

        [Test]
        public async Task Download_report_should_grab_using_client()
        {
            var mock = WithUsenetClient();
            mock.Setup(s => s.Download(It.IsAny<RemoteEpisode>(), It.IsAny<IIndexer>()));

            await Subject.DownloadReport(_parseResult, null);

            mock.Verify(s => s.Download(It.IsAny<RemoteEpisode>(), It.IsAny<IIndexer>()), Times.Once());
        }

        [Test]
        public void Download_report_should_not_publish_on_failed_grab_event()
        {
            var mock = WithUsenetClient();
            mock.Setup(s => s.Download(It.IsAny<RemoteEpisode>(), It.IsAny<IIndexer>()))
                .Throws(new WebException());

            Assert.ThrowsAsync<DownloadClientUnavailableException>(async () => await Subject.DownloadReport(_parseResult, null));

            VerifyEventNotPublished<EpisodeGrabbedEvent>();
        }

        [Test]
        public void Download_report_should_trigger_indexer_backoff_on_indexer_error()
        {
            var mock = WithUsenetClient();
            mock.Setup(s => s.Download(It.IsAny<RemoteEpisode>(), It.IsAny<IIndexer>()))
                .Callback<RemoteEpisode, IIndexer>((v, indexer) =>
                {
                    throw new ReleaseDownloadException(v.Release, "Error", new WebException());
                });

            Assert.ThrowsAsync<ReleaseDownloadException>(async () => await Subject.DownloadReport(_parseResult, null));

            Mocker.GetMock<IIndexerStatusService>()
                .Verify(v => v.RecordFailure(It.IsAny<int>(), It.IsAny<TimeSpan>()), Times.Once());
        }

        [Test]
        public void Download_report_should_trigger_indexer_backoff_on_http429_with_long_time()
        {
            var request = new HttpRequest("http://my.indexer.com");
            var response = new HttpResponse(request, new HttpHeader(), Array.Empty<byte>(), (HttpStatusCode)429);
            response.Headers["Retry-After"] = "300";

            var mock = WithUsenetClient();
            mock.Setup(s => s.Download(It.IsAny<RemoteEpisode>(), It.IsAny<IIndexer>()))
                .Callback<RemoteEpisode, IIndexer>((v, indexer) =>
                {
                    throw new ReleaseDownloadException(v.Release, "Error", new TooManyRequestsException(request, response));
                });

            Assert.ThrowsAsync<ReleaseDownloadException>(async () => await Subject.DownloadReport(_parseResult, null));

            Mocker.GetMock<IIndexerStatusService>()
                .Verify(v => v.RecordFailure(It.IsAny<int>(), TimeSpan.FromMinutes(5.0)), Times.Once());
        }

        [Test]
        public void Download_report_should_trigger_indexer_backoff_on_http429_based_on_date()
        {
            var request = new HttpRequest("http://my.indexer.com");
            var response = new HttpResponse(request, new HttpHeader(), Array.Empty<byte>(), (HttpStatusCode)429);
            response.Headers["Retry-After"] = DateTime.UtcNow.AddSeconds(300).ToString("r");

            var mock = WithUsenetClient();
            mock.Setup(s => s.Download(It.IsAny<RemoteEpisode>(), It.IsAny<IIndexer>()))
                .Callback<RemoteEpisode, IIndexer>((v, indexer) =>
                {
                    throw new ReleaseDownloadException(v.Release, "Error", new TooManyRequestsException(request, response));
                });

            Assert.ThrowsAsync<ReleaseDownloadException>(async () => await Subject.DownloadReport(_parseResult, null));

            Mocker.GetMock<IIndexerStatusService>()
                .Verify(v => v.RecordFailure(It.IsAny<int>(),
                    It.IsInRange<TimeSpan>(TimeSpan.FromMinutes(4.9), TimeSpan.FromMinutes(5.1), Moq.Range.Inclusive)),
                    Times.Once());
        }

        [Test]
        public void Download_report_should_not_trigger_indexer_backoff_on_downloadclient_error()
        {
            var mock = WithUsenetClient();
            mock.Setup(s => s.Download(It.IsAny<RemoteEpisode>(), It.IsAny<IIndexer>()))
                .Throws(new DownloadClientException("Some Error"));

            Assert.ThrowsAsync<DownloadClientUnavailableException>(async () => await Subject.DownloadReport(_parseResult, null));

            Mocker.GetMock<IIndexerStatusService>()
                .Verify(v => v.RecordFailure(It.IsAny<int>(), It.IsAny<TimeSpan>()), Times.Never());
        }

        [TestCase(typeof(DownloadClientException))]
        [TestCase(typeof(DownloadClientUnavailableException))]
        [TestCase(typeof(DownloadClientAuthenticationException))]
        [TestCase(typeof(IOException))]
        [TestCase(typeof(UnauthorizedAccessException))]
        public void should_not_retry_failed_client_for_another_release(Type exceptionType)
        {
            var mock = WithUsenetClient();
            mock.Setup(s => s.Download(It.IsAny<RemoteEpisode>(), It.IsAny<IIndexer>()))
                .Throws((Exception)Activator.CreateInstance(exceptionType, "Unable to add download"));

            Assert.ThrowsAsync<DownloadClientUnavailableException>(async () => await Subject.DownloadReport(_parseResult, null));

            Assert.ThrowsAsync<DownloadClientUnavailableException>(async () => await Subject.DownloadReport(_parseResult, null));

            _parseResult.Release.DownloadUrl = "http://test.site/download2.ext";

            Assert.ThrowsAsync<DownloadClientUnavailableException>(async () => await Subject.DownloadReport(_parseResult, null));

            mock.Verify(s => s.Download(It.IsAny<RemoteEpisode>(), It.IsAny<IIndexer>()), Times.Once());
            Mocker.GetMock<IDownloadClientStatusService>()
                .Verify(s => s.RecordFailure(mock.Object.Definition.Id, TimeSpan.FromMinutes(5)), Times.Once());
            VerifyEventNotPublished<EpisodeGrabbedEvent>();
        }

        [Test]
        public async Task should_fall_back_to_healthy_client_without_retrying_failed_client()
        {
            var failedClient = WithUsenetClient();
            var healthyClient = WithUsenetClient();
            failedClient.Setup(s => s.Download(It.IsAny<RemoteEpisode>(), It.IsAny<IIndexer>()))
                .Throws(new DownloadClientException("Unable to add download"));

            await Subject.DownloadReport(_parseResult, null);
            await Subject.DownloadReport(_parseResult, null);

            failedClient.Verify(s => s.Download(It.IsAny<RemoteEpisode>(), It.IsAny<IIndexer>()), Times.Once());
            healthyClient.Verify(s => s.Download(It.IsAny<RemoteEpisode>(), It.IsAny<IIndexer>()), Times.Exactly(2));
        }

        [Test]
        public async Task should_retry_client_after_backoff_expires()
        {
            var mock = WithUsenetClient();
            mock.Setup(s => s.Download(It.IsAny<RemoteEpisode>(), It.IsAny<IIndexer>()))
                .Throws(new IOException("Unable to write NZB"));

            Assert.ThrowsAsync<DownloadClientUnavailableException>(async () => await Subject.DownloadReport(_parseResult, null));

            Mocker.Resolve<ICacheManager>().GetCache<bool>(typeof(DownloadService), "failedDownloadClients")
                .Set(mock.Object.Definition.Id.ToString(), true, TimeSpan.FromSeconds(-1));
            mock.Setup(s => s.Download(It.IsAny<RemoteEpisode>(), It.IsAny<IIndexer>()))
                .ReturnsAsync("download-id");

            await Subject.DownloadReport(_parseResult, null);

            mock.Verify(s => s.Download(It.IsAny<RemoteEpisode>(), It.IsAny<IIndexer>()), Times.Exactly(2));
            VerifyEventPublished<EpisodeGrabbedEvent>();
        }

        [Test]
        public async Task should_allow_explicit_client_retry_and_clear_backoff_on_success()
        {
            var mock = WithUsenetClient();
            mock.Setup(s => s.Download(It.IsAny<RemoteEpisode>(), It.IsAny<IIndexer>()))
                .Throws(new DownloadClientException("Unable to add download"));

            Assert.ThrowsAsync<DownloadClientUnavailableException>(async () => await Subject.DownloadReport(_parseResult, null));

            Mocker.GetMock<IProvideDownloadClient>().Setup(s => s.Get(mock.Object.Definition.Id)).Returns(mock.Object);
            mock.Setup(s => s.Download(It.IsAny<RemoteEpisode>(), It.IsAny<IIndexer>()))
                .ReturnsAsync("download-id");

            await Subject.DownloadReport(_parseResult, mock.Object.Definition.Id);
            await Subject.DownloadReport(_parseResult, null);

            mock.Verify(s => s.Download(It.IsAny<RemoteEpisode>(), It.IsAny<IIndexer>()), Times.Exactly(3));
        }

        [TestCase(typeof(ReleaseDownloadException))]
        [TestCase(typeof(ReleaseUnavailableException))]
        [TestCase(typeof(DownloadClientRejectedReleaseException))]
        public void should_not_backoff_client_for_release_errors(Type exceptionType)
        {
            var mock = WithUsenetClient();
            mock.Setup(s => s.Download(It.IsAny<RemoteEpisode>(), It.IsAny<IIndexer>()))
                .Throws((Exception)Activator.CreateInstance(exceptionType, _parseResult.Release, "Release error"));

            Assert.ThrowsAsync(exceptionType, async () => await Subject.DownloadReport(_parseResult, null));
            Assert.ThrowsAsync(exceptionType, async () => await Subject.DownloadReport(_parseResult, null));

            mock.Verify(s => s.Download(It.IsAny<RemoteEpisode>(), It.IsAny<IIndexer>()), Times.Exactly(2));
            Mocker.GetMock<IDownloadClientStatusService>()
                .Verify(s => s.RecordFailure(It.IsAny<int>(), It.IsAny<TimeSpan>()), Times.Never());
        }

        [Test]
        public void should_not_backoff_client_for_invalid_nzb()
        {
            var mock = WithUsenetClient();
            mock.Setup(s => s.Download(It.IsAny<RemoteEpisode>(), It.IsAny<IIndexer>()))
                .Throws(new InvalidNzbException("Invalid NZB"));

            Assert.ThrowsAsync<DownloadClientUnavailableException>(async () => await Subject.DownloadReport(_parseResult, null));
            Assert.ThrowsAsync<DownloadClientUnavailableException>(async () => await Subject.DownloadReport(_parseResult, null));

            mock.Verify(s => s.Download(It.IsAny<RemoteEpisode>(), It.IsAny<IIndexer>()), Times.Exactly(2));
            Mocker.GetMock<IDownloadClientStatusService>()
                .Verify(s => s.RecordFailure(It.IsAny<int>(), It.IsAny<TimeSpan>()), Times.Never());
        }

        [Test]
        public void Download_report_should_not_trigger_indexer_backoff_on_indexer_404_error()
        {
            var mock = WithUsenetClient();
            mock.Setup(s => s.Download(It.IsAny<RemoteEpisode>(), It.IsAny<IIndexer>()))
                .Callback<RemoteEpisode, IIndexer>((v, indexer) =>
                {
                    throw new ReleaseUnavailableException(v.Release, "Error", new WebException());
                });

            Assert.ThrowsAsync<ReleaseUnavailableException>(async () => await Subject.DownloadReport(_parseResult, null));

            Mocker.GetMock<IIndexerStatusService>()
                .Verify(v => v.RecordFailure(It.IsAny<int>(), It.IsAny<TimeSpan>()), Times.Never());
        }

        [Test]
        public void should_not_attempt_download_if_client_isnt_configured()
        {
            Assert.ThrowsAsync<DownloadClientUnavailableException>(async () => await Subject.DownloadReport(_parseResult, null));

            Mocker.GetMock<IDownloadClient>().Verify(c => c.Download(It.IsAny<RemoteEpisode>(), It.IsAny<IIndexer>()), Times.Never());
            VerifyEventNotPublished<EpisodeGrabbedEvent>();
        }

        [TestCase(ReleaseSourceType.Rss, true)]
        [TestCase(ReleaseSourceType.Search, true)]
        [TestCase(ReleaseSourceType.InteractiveSearch, false)]
        public async Task should_filter_blocked_clients_for_automatic_grabs(ReleaseSourceType releaseSource, bool filterBlockedClients)
        {
            var mockUsenet = WithUsenetClient();
            _parseResult.ReleaseSource = releaseSource;

            await Subject.DownloadReport(_parseResult, null);

            Mocker.GetMock<IProvideDownloadClient>()
                .Verify(c => c.GetDownloadClients(_parseResult.Release.DownloadProtocol, _parseResult.Release.IndexerId, filterBlockedClients, _parseResult.Series.Tags), Times.Once());
            mockUsenet.Verify(c => c.Download(It.IsAny<RemoteEpisode>(), It.IsAny<IIndexer>()), Times.Once());
            VerifyEventPublished<EpisodeGrabbedEvent>();
        }

        [Test]
        public async Task should_send_download_to_correct_usenet_client()
        {
            var mockTorrent = WithTorrentClient();
            var mockUsenet = WithUsenetClient();

            await Subject.DownloadReport(_parseResult, null);

            mockTorrent.Verify(c => c.Download(It.IsAny<RemoteEpisode>(), It.IsAny<IIndexer>()), Times.Never());
            mockUsenet.Verify(c => c.Download(It.IsAny<RemoteEpisode>(), It.IsAny<IIndexer>()), Times.Once());
        }

        [Test]
        public async Task should_send_download_to_correct_torrent_client()
        {
            var mockTorrent = WithTorrentClient();
            var mockUsenet = WithUsenetClient();

            _parseResult.Release.DownloadProtocol = DownloadProtocol.Torrent;

            await Subject.DownloadReport(_parseResult, null);

            mockTorrent.Verify(c => c.Download(It.IsAny<RemoteEpisode>(), It.IsAny<IIndexer>()), Times.Once());
            mockUsenet.Verify(c => c.Download(It.IsAny<RemoteEpisode>(), It.IsAny<IIndexer>()), Times.Never());
        }
    }
}
