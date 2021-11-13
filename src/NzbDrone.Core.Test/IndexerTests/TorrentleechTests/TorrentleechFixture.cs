using System;
using System.Linq;
using System.Net.Http;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.Torrentleech;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.IndexerTests.TorrentleechTests
{
    [TestFixture]
    public class TorrentleechFixture : CoreTest<Torrentleech>
    {
        [SetUp]
        public void Setup()
        {
            Subject.Definition = new IndexerDefinition()
                {
                    Name = "Torrentleech",
                    Settings = new TorrentleechSettings()
                };
        }

        [Test]
        public void should_parse_recent_feed_from_Torrentleech()
        {
            var recentFeed = ReadAllText(@"Files/Indexers/Torrentleech/Torrentleech.xml");

            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Execute(It.Is<HttpRequest>(v => v.Method == HttpMethod.Get)))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), recentFeed));

            var releases = Subject.FetchRecent();

            releases.Should().HaveCount(5);
            releases.First().Should().BeOfType<TorrentInfo>();

            var torrentInfo = releases.First() as TorrentInfo;

            torrentInfo.Title.Should().Be("Classic Car Rescue S02E04 720p HDTV x264-C4TV");
            torrentInfo.DownloadProtocol.Should().Be(DownloadProtocol.Torrent);
            torrentInfo.DownloadUrl.Should().Be("http://www.torrentleech.org/rss/download/513575/1234/Classic.Car.Rescue.S02E04.720p.HDTV.x264-C4TV.torrent");
            torrentInfo.InfoUrl.Should().Be("http://www.torrentleech.org/torrent/513575");
            torrentInfo.CommentUrl.Should().Be("http://www.torrentleech.org/torrent/513575#comments");
            torrentInfo.Indexer.Should().Be(Subject.Definition.Name);
            torrentInfo.PublishDate.Should().Be(DateTime.Parse("2014/05/12 19:15:28"));
            torrentInfo.Size.Should().Be(0);
            torrentInfo.InfoHash.Should().Be(null);
            torrentInfo.MagnetUrl.Should().Be(null);
            torrentInfo.Peers.Should().Be(7 + 1);
            torrentInfo.Seeders.Should().Be(1);
        }
    }
}
