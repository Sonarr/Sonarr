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
        private String _recentFeed;

        [TestFixtureSetUp]
        public void SetupFixture()
        {
            Subject.Definition = new IndexerDefinition()
                {
                    Name = "Torrentleech",
                    Settings = new TorrentleechSettings()
                };

            _recentFeed = ReadAllText(@"Files/RSS/Torrentleech.xml");
        }

        [Test]
        public void Indexer_TestFeedParser_Torrentleech()
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

            torrentInfo.Title.Should().Be("Classic Car Rescue S02E04 720p HDTV x264-C4TV");
            torrentInfo.DownloadProtocol.Should().Be(DownloadProtocol.Torrent);
            torrentInfo.DownloadUrl.Should().Be("http://www.torrentleech.org/rss/download/513575/1234/Classic.Car.Rescue.S02E04.720p.HDTV.x264-C4TV.torrent");
            torrentInfo.Indexer.Should().Be(Subject.Definition.Name);
            firstRelease.PublishDate.Should().Be(DateTime.Parse("2014/05/12 19:15:28"));
            torrentInfo.Size.Should().Be(0);
            torrentInfo.InfoHash.Should().Be(null);
            torrentInfo.MagnetUrl.Should().Be(null);
            torrentInfo.Peers.Should().Be(7);
            torrentInfo.Seeds.Should().Be(1);
        }
    }
}
