using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.BitMeTv;
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

namespace NzbDrone.Core.Test.IndexerTests.BitMeTvTests
{
    [TestFixture]
    public class BitMeTvFixture : CoreTest<BitMeTv>
    {
        private String _recentFeed;

        [TestFixtureSetUp]
        public void SetupFixture()
        {
            Subject.Definition = new IndexerDefinition()
                                     {
                                         Name = "BitMeTV",
                                         Settings = new BitMeTvSettings()
                                     };

            _recentFeed = ReadAllText(@"Files/RSS/BitMeTv.xml");
        }

        [Test]
        public void Indexer_TestFeedParser_BitMeTv()
        {
            var httpClientMock = Mocker.GetMock<IHttpClient>();
            httpClientMock.Setup(o => o.Get(It.IsAny<HttpRequest>()))
                          .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), _recentFeed));

            var httpClient = httpClientMock.Object;

            var fetchService = new FetchFeedService(httpClient, TestLogger);
            var releases = fetchService.FetchRss(Subject);

            releases.Should().HaveCount(5);

            var firstRelease = releases.First();

            Assert.IsInstanceOf<TorrentInfo>(firstRelease);

            var torrentInfo = (TorrentInfo)firstRelease;

            torrentInfo.Title.Should().Be("Total.Divas.S02E08.HDTV.x264-CRiMSON");
            torrentInfo.DownloadProtocol.Should().Be(DownloadProtocol.Torrent);
            torrentInfo.DownloadUrl.Should().Be("http://www.bitmetv.org/download.php/12/Total.Divas.S02E08.HDTV.x264-CRiMSON.torrent");
            torrentInfo.Indexer.Should().Be(Subject.Definition.Name);
            firstRelease.PublishDate.Should().Be(DateTime.Parse("2014/05/13 17:04:29"));
            torrentInfo.Size.Should().Be(395009065);
            torrentInfo.InfoHash.Should().Be(null);
            torrentInfo.MagnetUrl.Should().Be(null);
            torrentInfo.Peers.Should().Be(null);
            torrentInfo.Seeds.Should().Be(null);
        }
    }
}
