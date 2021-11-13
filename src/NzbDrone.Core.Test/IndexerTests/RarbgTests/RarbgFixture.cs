using System;
using System.Linq;
using System.Net.Http;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.Rarbg;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.IndexerTests.RarbgTests
{
    [TestFixture]
    public class RarbgFixture : CoreTest<Rarbg>
    {
        [SetUp]
        public void Setup()
        {
            Subject.Definition = new IndexerDefinition()
                {
                    Name = "Rarbg",
                    Settings = new RarbgSettings()
                };

            Mocker.GetMock<IRarbgTokenProvider>()
                .Setup(v => v.GetToken(It.IsAny<RarbgSettings>()))
                .Returns("validtoken");
        }

        [Test]
        public void should_parse_recent_feed_from_Rarbg()
        {
            var recentFeed = ReadAllText(@"Files/Indexers/Rarbg/RecentFeed_v2.json");

            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Execute(It.Is<HttpRequest>(v => v.Method == HttpMethod.Get)))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), recentFeed));

            var releases = Subject.FetchRecent();

            releases.Should().HaveCount(4);
            releases.First().Should().BeOfType<TorrentInfo>();

            var torrentInfo = releases.First() as TorrentInfo;

            torrentInfo.Title.Should().Be("Sense8.S01E01.WEBRip.x264-FGT");
            torrentInfo.DownloadProtocol.Should().Be(DownloadProtocol.Torrent);
            torrentInfo.DownloadUrl.Should().Be("magnet:?xt=urn:btih:d8bde635f573acb390c7d7e7efc1556965fdc802&dn=Sense8.S01E01.WEBRip.x264-FGT&tr=http%3A%2F%2Ftracker.trackerfix.com%3A80%2Fannounce&tr=udp%3A%2F%2F9.rarbg.me%3A2710&tr=udp%3A%2F%2F9.rarbg.to%3A2710&tr=udp%3A%2F%2Fopen.demonii.com%3A1337%2Fannounce");
            torrentInfo.InfoUrl.Should().Be("https://torrentapi.org/redirect_to_info.php?token=i5cx7b9agd&p=8_6_4_4_5_6__d8bde635f5&app_id=Sonarr");
            torrentInfo.Indexer.Should().Be(Subject.Definition.Name);
            torrentInfo.PublishDate.Should().Be(DateTime.Parse("2015-06-05 16:58:11 +0000").ToUniversalTime());
            torrentInfo.Size.Should().Be(564198371);
            torrentInfo.InfoHash.Should().BeNull();
            torrentInfo.MagnetUrl.Should().BeNull();
            torrentInfo.Peers.Should().Be(304 + 200);
            torrentInfo.Seeders.Should().Be(304);
            torrentInfo.TvdbId.Should().Be(268156);
            torrentInfo.TvRageId.Should().Be(35197);
        }

        [Test]
        public void should_parse_error_20_as_empty_results()
        {
            Mocker.GetMock<IHttpClient>()
                   .Setup(o => o.Execute(It.Is<HttpRequest>(v => v.Method == HttpMethod.Get)))
                   .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), "{ error_code: 20, error: \"some message\" }"));

            var releases = Subject.FetchRecent();

            releases.Should().HaveCount(0);
        }

        [Test]
        public void should_warn_on_unknown_error()
        {
            Mocker.GetMock<IHttpClient>()
                   .Setup(o => o.Execute(It.Is<HttpRequest>(v => v.Method == HttpMethod.Get)))
                   .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), "{ error_code: 25, error: \"some message\" }"));

            var releases = Subject.FetchRecent();

            releases.Should().HaveCount(0);

            ExceptionVerification.ExpectedWarns(1);
        }
    }
}
