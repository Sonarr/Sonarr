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
        [SetUp]
        public void Setup()
        {
            Subject.Definition = new IndexerDefinition()
                {
                    Name = "Kickass Torrents",
                    Settings = new KickassTorrentsSettings() { VerifiedOnly = false }
                };
        }

        [Test]
        public void should_parse_recent_feed_from_KickassTorrents()
        {
            var recentFeed = ReadAllText(@"Files/RSS/KickassTorrents.xml");

            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Execute(It.Is<HttpRequest>(v => v.Method == HttpMethod.GET)))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), recentFeed));

            var releases = Subject.FetchRecent();

            releases.Should().HaveCount(5);
            releases.First().Should().BeOfType<TorrentInfo>();

            var torrentInfo = releases.First() as TorrentInfo;

            torrentInfo.Title.Should().Be("Doctor Stranger.E03.140512.HDTV.H264.720p-iPOP.avi [CTRG]");
            torrentInfo.DownloadProtocol.Should().Be(DownloadProtocol.Torrent);
            torrentInfo.DownloadUrl.Should().Be("http://torcache.net/torrent/208C4F7866612CC88BFEBC7C496FA72C2368D1C0.torrent?title=[kickass.to]doctor.stranger.e03.140512.hdtv.h264.720p.ipop.avi.ctrg");
            torrentInfo.InfoUrl.Should().Be("http://kickass.to/doctor-stranger-e03-140512-hdtv-h264-720p-ipop-avi-ctrg-t9100648.html");
            torrentInfo.CommentUrl.Should().BeNullOrEmpty();
            torrentInfo.Indexer.Should().Be(Subject.Definition.Name);
            torrentInfo.PublishDate.Should().Be(DateTime.Parse("2014/05/12 16:16:49"));
            torrentInfo.Size.Should().Be(1205364736);
            torrentInfo.InfoHash.Should().Be("208C4F7866612CC88BFEBC7C496FA72C2368D1C0");
            torrentInfo.MagnetUrl.Should().Be("magnet:?xt=urn:btih:208C4F7866612CC88BFEBC7C496FA72C2368D1C0&dn=doctor+stranger+e03+140512+hdtv+h264+720p+ipop+avi+ctrg&tr=udp%3A%2F%2Fopen.demonii.com%3A1337%2Fannounce");
            torrentInfo.Peers.Should().Be(311);
            torrentInfo.Seeds.Should().Be(206);
        }

        [Test]
        public void should_return_empty_list_on_404()
        {
            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Execute(It.Is<HttpRequest>(v => v.Method == HttpMethod.GET)))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), new Byte[0], System.Net.HttpStatusCode.NotFound));

            var releases = Subject.FetchRecent();

            releases.Should().HaveCount(0);

            ExceptionVerification.IgnoreWarns();
        }

        [Test]
        public void should_not_return_unverified_releases_if_not_configured()
        {
            (Subject.Definition.Settings as KickassTorrentsSettings).VerifiedOnly = true;

            var recentFeed = ReadAllText(@"Files/RSS/KickassTorrents.xml");

            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Execute(It.Is<HttpRequest>(v => v.Method == HttpMethod.GET)))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), recentFeed));

            var releases = Subject.FetchRecent();

            releases.Should().HaveCount(4);
        }
    }
}
