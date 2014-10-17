using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.BitMeTv;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using System;
using System.Linq;
using FluentAssertions;

namespace NzbDrone.Core.Test.IndexerTests.BitMeTvTests
{
    [TestFixture]
    public class BitMeTvFixture : CoreTest<BitMeTv>
    {
        [SetUp]
        public void Setup()
        {
            Subject.Definition = new IndexerDefinition()
                                     {
                                         Name = "BitMeTV",
                                         Settings = new BitMeTvSettings()
                                     };
        }

        [Test]
        public void should_parse_recent_feed_from_BitMeTv()
        {
            var recentFeed = ReadAllText(@"Files/RSS/BitMeTv.xml");
            
               
            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Execute(It.Is<HttpRequest>(v => v.Method == HttpMethod.GET)))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), recentFeed));

            var releases = Subject.FetchRecent();

            releases.Should().HaveCount(5);
            releases.First().Should().BeOfType<TorrentInfo>();

            var torrentInfo = releases.First() as TorrentInfo;

            torrentInfo.Title.Should().Be("Total.Divas.S02E08.HDTV.x264-CRiMSON");
            torrentInfo.DownloadProtocol.Should().Be(DownloadProtocol.Torrent);
            torrentInfo.DownloadUrl.Should().Be("http://www.bitmetv.org/download.php/12/Total.Divas.S02E08.HDTV.x264-CRiMSON.torrent");
            torrentInfo.InfoUrl.Should().BeNullOrEmpty();
            torrentInfo.CommentUrl.Should().BeNullOrEmpty();
            torrentInfo.Indexer.Should().Be(Subject.Definition.Name);
            torrentInfo.PublishDate.Should().Be(DateTime.Parse("2014/05/13 17:04:29"));
            torrentInfo.Size.Should().Be(395009065);
            torrentInfo.InfoHash.Should().Be(null);
            torrentInfo.MagnetUrl.Should().Be(null);
            torrentInfo.Peers.Should().Be(null);
            torrentInfo.Seeds.Should().Be(null);
        }
    }
}
