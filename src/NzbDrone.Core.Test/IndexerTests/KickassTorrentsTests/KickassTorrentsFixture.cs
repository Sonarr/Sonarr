using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.KickassTorrents;
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

namespace NzbDrone.Core.Test.IndexerTests.KickassTorrentsTests
{
    [TestFixture]
    public class KickassTorrentsFixture : CoreTest<KickassTorrents>
    {
        private String _recentFeed;

        [TestFixtureSetUp]
        public void SetupFixture()
        {
            Subject.Definition = new IndexerDefinition()
                {
                    Name = "Kickass Torrents",
                    Settings = new KickassTorrentsSettings()
                };


            _recentFeed = ReadAllText(@"Files/RSS/KickassTorrents.xml");
        }

        [Test]
        public void Indexer_TestFeedParser_KickassTorrents()
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

            torrentInfo.Title.Should().Be("Doctor Stranger.E03.140512.HDTV.H264.720p-iPOP.avi [CTRG]");
            torrentInfo.DownloadProtocol.Should().Be(DownloadProtocol.Torrent);
            torrentInfo.DownloadUrl.Should().Be("http://torcache.net/torrent/208C4F7866612CC88BFEBC7C496FA72C2368D1C0.torrent?title=[kickass.to]doctor.stranger.e03.140512.hdtv.h264.720p.ipop.avi.ctrg");
            torrentInfo.Indexer.Should().Be(Subject.Definition.Name);
            firstRelease.PublishDate.Should().Be(DateTime.Parse("2014/05/12 16:16:49"));
            torrentInfo.Size.Should().Be(1205364736);
            torrentInfo.InfoHash.Should().Be("208C4F7866612CC88BFEBC7C496FA72C2368D1C0");
            torrentInfo.MagnetUrl.Should().Be("magnet:?xt=urn:btih:208C4F7866612CC88BFEBC7C496FA72C2368D1C0&dn=doctor+stranger+e03+140512+hdtv+h264+720p+ipop+avi+ctrg&tr=udp%3A%2F%2Fopen.demonii.com%3A1337%2Fannounce");
            torrentInfo.Peers.Should().Be(311);
            torrentInfo.Seeds.Should().Be(206);
        }
    }
}
