using System;
using System.Linq;
using System.Net.Http;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.BroadcastheNet;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.IndexerTests.BroadcastheNetTests
{
    [TestFixture]
    public class BroadcastheNetFixture : CoreTest<BroadcastheNet>
    {
        [SetUp]
        public void Setup()
        {
            Subject.Definition = new IndexerDefinition()
                {
                    Name = "BroadcastheNet",
                    Settings = new BroadcastheNetSettings() { ApiKey = "abc", BaseUrl = "https://api.broadcasthe.net/" }
                };
        }

        [Test]
        public void should_parse_recent_feed_from_BroadcastheNet()
        {
            var recentFeed = ReadAllText(@"Files/Indexers/BroadcastheNet/RecentFeed.json");

            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Execute(It.Is<HttpRequest>(v => v.Method == HttpMethod.Post)))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), recentFeed));

            var releases = Subject.FetchRecent();

            releases.Should().HaveCount(2);
            releases.First().Should().BeOfType<TorrentInfo>();

            var torrentInfo = releases.First() as TorrentInfo;

            torrentInfo.Guid.Should().Be("BTN-123");
            torrentInfo.Title.Should().Be("Jimmy.Kimmel.2014.09.15.Jane.Fonda.HDTV.x264-aAF");
            torrentInfo.DownloadProtocol.Should().Be(DownloadProtocol.Torrent);
            torrentInfo.DownloadUrl.Should().Be("https://broadcasthe.net/torrents.php?action=download&id=123&authkey=123&torrent_pass=123");
            torrentInfo.InfoUrl.Should().Be("https://broadcasthe.net/torrents.php?id=237457&torrentid=123");
            torrentInfo.CommentUrl.Should().BeNullOrEmpty();
            torrentInfo.Indexer.Should().Be(Subject.Definition.Name);
            torrentInfo.PublishDate.Should().Be(DateTime.Parse("2014/09/16 21:15:33"));
            torrentInfo.Size.Should().Be(505099926);
            torrentInfo.InfoHash.Should().Be("123");
            torrentInfo.TvdbId.Should().Be(71998);
            torrentInfo.TvRageId.Should().Be(4055);
            torrentInfo.MagnetUrl.Should().BeNullOrEmpty();
            torrentInfo.Peers.Should().Be(40 + 9);
            torrentInfo.Seeders.Should().Be(40);

            torrentInfo.Origin.Should().Be("Scene");
            torrentInfo.Source.Should().Be("HDTV");
            torrentInfo.Container.Should().Be("MP4");
            torrentInfo.Codec.Should().Be("x264");
            torrentInfo.Resolution.Should().Be("SD");
        }

        private void VerifyBackOff()
        {
            Mocker.GetMock<IIndexerStatusService>()
                .Verify(v => v.RecordFailure(It.IsAny<int>(), It.IsAny<TimeSpan>()), Times.Once());
        }

        [Test]
        public void should_back_off_on_bad_request()
        {
            Mocker.GetMock<IHttpClient>()
                .Setup(v => v.Execute(It.IsAny<HttpRequest>()))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), new byte[0], System.Net.HttpStatusCode.BadRequest));

            var results = Subject.FetchRecent();

            results.Should().BeEmpty();

            VerifyBackOff();

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_back_off_and_report_api_key_invalid()
        {
            Mocker.GetMock<IHttpClient>()
                .Setup(v => v.Execute(It.IsAny<HttpRequest>()))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), new byte[0], System.Net.HttpStatusCode.Unauthorized));

            var results = Subject.FetchRecent();

            results.Should().BeEmpty();

            VerifyBackOff();

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_back_off_on_unknown_method()
        {
            Mocker.GetMock<IHttpClient>()
                .Setup(v => v.Execute(It.IsAny<HttpRequest>()))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), new byte[0], System.Net.HttpStatusCode.NotFound));

            var results = Subject.FetchRecent();

            results.Should().BeEmpty();

            VerifyBackOff();

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_back_off_api_limit_reached()
        {
            Mocker.GetMock<IHttpClient>()
                .Setup(v => v.Execute(It.IsAny<HttpRequest>()))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), new byte[0], System.Net.HttpStatusCode.ServiceUnavailable));

            var results = Subject.FetchRecent();

            results.Should().BeEmpty();

            VerifyBackOff();

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_replace_https_http_as_needed()
        {
            var recentFeed = ReadAllText(@"Files/Indexers/BroadcastheNet/RecentFeed.json");

            (Subject.Definition.Settings as BroadcastheNetSettings).BaseUrl = "http://api.broadcasthe.net/";

            recentFeed = recentFeed.Replace("http:", "https:");

            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Execute(It.Is<HttpRequest>(v => v.Method == HttpMethod.Post)))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), recentFeed));

            var releases = Subject.FetchRecent();

            releases.Should().HaveCount(2);
            releases.First().Should().BeOfType<TorrentInfo>();

            var torrentInfo = releases.First() as TorrentInfo;

            torrentInfo.DownloadUrl.Should().Be("http://broadcasthe.net/torrents.php?action=download&id=123&authkey=123&torrent_pass=123");
            torrentInfo.InfoUrl.Should().Be("http://broadcasthe.net/torrents.php?id=237457&torrentid=123");
        }
    }
}
