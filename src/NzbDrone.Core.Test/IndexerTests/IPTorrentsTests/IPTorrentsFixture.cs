using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.IPTorrents;
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

namespace NzbDrone.Core.Test.IndexerTests.IPTorrentsTests
{
    [TestFixture]
    public class IPTorrentsFixture : CoreTest<IPTorrents>
    {
        private String _recentFeed;

        [TestFixtureSetUp]
        public void SetupFixture()
        {
            Subject.Definition = new IndexerDefinition()
                                    {
                                        Name = "IPTorrents",
                                        Settings = new IPTorrentsSettings() {  Url = "http://fake.com/" }
                                    };

            _recentFeed = ReadAllText(@"Files/RSS/IPTorrents.xml");
        }

        [Test]
        public void Indexer_TestFeedParser_IPTorrents()
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

            torrentInfo.Title.Should().Be("24 S03E12 720p WEBRip h264-DRAWER");
            torrentInfo.DownloadProtocol.Should().Be(DownloadProtocol.Torrent);
            torrentInfo.DownloadUrl.Should().Be("http://iptorrents.com/download.php/1234/24.S03E12.720p.WEBRip.h264-DRAWER.torrent?torrent_pass=abcd");
            torrentInfo.Indexer.Should().Be(Subject.Definition.Name);
            firstRelease.PublishDate.Should().Be(DateTime.Parse("2014/05/12 19:06:34"));
            torrentInfo.Size.Should().Be(1471026299);
            torrentInfo.InfoHash.Should().Be(null);
            torrentInfo.MagnetUrl.Should().Be(null);
            torrentInfo.Peers.Should().Be(null);
            torrentInfo.Seeds.Should().Be(null);
        }
    }
}
