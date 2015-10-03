using System;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.TitansOfTv;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.IndexerTests.TitansOfTvTests
{
    [TestFixture]
    public class TitansOfTvFixture : CoreTest<TitansOfTv>
    {
        [SetUp]
        public void Setup()
        {
            Subject.Definition = new IndexerDefinition
                {
                    Name = "TitansOfTV",
                    Settings = new TitansOfTvSettings { ApiKey = "abc", BaseUrl = "https://titansof.tv/api" }
                };
        }

        [Test]
        public void should_parse_recent_feed_from_TitansOfTv()
        {
            var recentFeed = ReadAllText(@"Files/Indexers/TitansOfTv/RecentFeed.json");

            Mocker.GetMock<IHttpClient>()
                  .Setup(o => o.Execute(It.Is<HttpRequest>(v => v.Method == HttpMethod.GET)))
                  .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), recentFeed));

            var releases = Subject.FetchRecent();

            releases.Should().HaveCount(2);
            releases.First().Should().BeOfType<TorrentInfo>();

            var torrentInfo = releases.First() as TorrentInfo;

            torrentInfo.Guid.Should().Be("ToTV-19445");
            torrentInfo.Title.Should().Be("Series.Title.S02E04.720p.HDTV.x264-W4F");
            torrentInfo.DownloadProtocol.Should().Be(DownloadProtocol.Torrent);
            torrentInfo.DownloadUrl.Should().Be("https://titansof.tv/api/torrents/19445/download?apikey=abc");
            torrentInfo.InfoUrl.Should().Be("https://titansof.tv/series/287053/episode/5453241");
            torrentInfo.CommentUrl.Should().BeNullOrEmpty();
            torrentInfo.Indexer.Should().Be(Subject.Definition.Name);
            torrentInfo.PublishDate.Should().Be(DateTime.Parse("2015-06-25 04:13:44"));
            torrentInfo.Size.Should().Be(435402993);
            torrentInfo.InfoHash.Should().BeNullOrEmpty();
            torrentInfo.TvRageId.Should().Be(0);
            torrentInfo.MagnetUrl.Should().BeNullOrEmpty();
            torrentInfo.Peers.Should().Be(2+5);
            torrentInfo.Seeders.Should().Be(2);
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
            var recentFeed = ReadAllText(@"Files/Indexers/TitansOfTv/RecentFeed.json");

            (Subject.Definition.Settings as TitansOfTvSettings).BaseUrl = "http://titansof.tv/api/torrents";

            recentFeed = recentFeed.Replace("http:", "https:");

            Mocker.GetMock<IHttpClient>()
                  .Setup(o => o.Execute(It.Is<HttpRequest>(v => v.Method == HttpMethod.GET)))
                  .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), recentFeed));

            var releases = Subject.FetchRecent();

            releases.Should().HaveCount(2);
            releases.First().Should().BeOfType<TorrentInfo>();

            var torrentInfo = releases.First() as TorrentInfo;

            torrentInfo.DownloadUrl.Should().Be("http://titansof.tv/api/torrents/19445/download?apikey=abc");
            torrentInfo.InfoUrl.Should().Be("http://titansof.tv/series/287053/episode/5453241");
        }
    }
}
