using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.Torrentleech;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Test.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;

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
            var recentFeed = ReadAllText(@"Files/RSS/Torrentleech.xml");

            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Execute(It.Is<HttpRequest>(v => v.Method == HttpMethod.GET)))
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
            torrentInfo.Peers.Should().Be(7);
            torrentInfo.Seeds.Should().Be(1);
        }
    }
}
