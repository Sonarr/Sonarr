using System.Net;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.BroadcastheNet;
using NzbDrone.Core.Indexers.TitansOfTv;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;
using System;
using System.Linq;
using FluentAssertions;

namespace NzbDrone.Core.Test.IndexerTests.TitansOfTvTests
{
    [TestFixture]
    public class TitansOfTvFixture : CoreTest<TitansOfTv>
    {
        [SetUp]
        public void Setup()
        {
            Subject.Definition = new IndexerDefinition()
                {
                    Name = "TitansOfTv",
                    Settings = new TitansOfTvSettings() {  ApiKey = "abc" }
                };
        }

        [Test]
        public void should_parse_recent_feed_from_TitansOfTv()
        {
            var recentFeed = ReadAllText(@"Files/Indexers/TitansOfTv/Feed.json");

            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Execute(It.Is<HttpRequest>(v => v.Method == HttpMethod.GET)))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), recentFeed, HttpStatusCode.OK));

            var releases = Subject.FetchRecent();

            releases.Should().HaveCount(5);
            releases.First().Should().BeOfType<TorrentInfo>();

            var torrentInfo = releases.First() as TorrentInfo;

            torrentInfo.Should().NotBeNull();

            //torrentInfo.Guid.Should().Be("BTN-123");
            torrentInfo.Title.Should().Be("Archer.2009.S06E13.480p.HDTV.x264-mSD");
            torrentInfo.DownloadProtocol.Should().Be(DownloadProtocol.Torrent);
            torrentInfo.DownloadUrl.Should().Be("https://titansof.tv/api/torrents/4748/download?apikey=e55e663e568b204b6d667707cf4293ddc0d34520");
            //torrentInfo.InfoUrl.Should().Be("https://broadcasthe.net/torrents.php?id=237457&torrentid=123");
            torrentInfo.CommentUrl.Should().BeNullOrEmpty();
            torrentInfo.Indexer.Should().Be(Subject.Definition.Name);
            torrentInfo.PublishDate.Should().Be(DateTime.Parse("2015-04-04 17:10:05"));
            torrentInfo.Size.Should().Be(97388670);
            //torrentInfo.InfoHash.Should().Be("123");
            //torrentInfo.TvRageId.Should().Be(110381);
            torrentInfo.MagnetUrl.Should().BeNullOrEmpty();
            torrentInfo.Peers.Should().Be(0);
            torrentInfo.Seeders.Should().Be(0);
        }

        private void VerifyBackOff()
        {
            // TODO How to detect (and implement) back-off logic.
        }


    }
}
