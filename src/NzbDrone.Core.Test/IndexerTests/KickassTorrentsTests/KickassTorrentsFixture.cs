using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.KickassTorrents;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;
using System;
using System.Linq;
using FluentAssertions;
using System.Text.RegularExpressions;

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
            var recentFeed = ReadAllText(@"Files/Indexers/KickassTorrents/KickassTorrents.xml");

            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Execute(It.Is<HttpRequest>(v => v.Method == HttpMethod.GET)))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), recentFeed));

            var releases = Subject.FetchRecent();

            releases.Should().HaveCount(5);
            releases.First().Should().BeOfType<TorrentInfo>();

            var torrentInfo = (TorrentInfo) releases.First();

            torrentInfo.Title.Should().Be("Doctor Stranger.E03.140512.HDTV.H264.720p-iPOP.avi [CTRG]");
            torrentInfo.DownloadProtocol.Should().Be(DownloadProtocol.Torrent);
            torrentInfo.DownloadUrl.Should().Be("http://torcache.net/torrent/208C4F7866612CC88BFEBC7C496FA72C2368D1C0.torrent?title=%5Bkickass.to%5Ddoctor.stranger.e03.140512.hdtv.h264.720p.ipop.avi.ctrg");
            torrentInfo.InfoUrl.Should().Be("http://kickass.to/doctor-stranger-e03-140512-hdtv-h264-720p-ipop-avi-ctrg-t9100648.html");
            torrentInfo.CommentUrl.Should().BeNullOrEmpty();
            torrentInfo.Indexer.Should().Be(Subject.Definition.Name);
            torrentInfo.PublishDate.Should().Be(DateTime.Parse("2014/05/12 16:16:49"));
            torrentInfo.Size.Should().Be(1205364736);
            torrentInfo.InfoHash.Should().Be("208C4F7866612CC88BFEBC7C496FA72C2368D1C0");
            torrentInfo.MagnetUrl.Should().Be("magnet:?xt=urn:btih:208C4F7866612CC88BFEBC7C496FA72C2368D1C0&dn=doctor+stranger+e03+140512+hdtv+h264+720p+ipop+avi+ctrg&tr=udp%3A%2F%2Fopen.demonii.com%3A1337%2Fannounce");
        }

        [Test]
        public void should_return_empty_list_on_404()
        {
            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Execute(It.Is<HttpRequest>(v => v.Method == HttpMethod.GET)))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), new byte[0], System.Net.HttpStatusCode.NotFound));

            var releases = Subject.FetchRecent();

            releases.Should().HaveCount(0);

            ExceptionVerification.IgnoreWarns();
        }

        [Test]
        public void should_not_return_unverified_releases_if_not_configured()
        {
            ((KickassTorrentsSettings) Subject.Definition.Settings).VerifiedOnly = true;

            var recentFeed = ReadAllText(@"Files/Indexers/KickassTorrents/KickassTorrents.xml");

            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Execute(It.Is<HttpRequest>(v => v.Method == HttpMethod.GET)))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), recentFeed));

            var releases = Subject.FetchRecent();

            releases.Should().HaveCount(4);
        }

        [Test]
        public void should_set_seeders_to_null()
        {
            // Atm, Kickass supplies 0 as seeders and leechers on the rss feed (but not the site), so set it to null if there aren't any peers.
            var recentFeed = ReadAllText(@"Files/Indexers/KickassTorrents/KickassTorrents.xml");

            recentFeed = recentFeed.Replace("<pubDate>Mon, 12 May 2014 16:16:49 +0000</pubDate>", string.Format("<pubDate>{0:R}</pubDate>", DateTime.UtcNow));
            recentFeed = Regex.Replace(recentFeed, @"(seeds|peers)\>\d*", "$1>0");

            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Execute(It.Is<HttpRequest>(v => v.Method == HttpMethod.GET)))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), recentFeed));

            var releases = Subject.FetchRecent();

            releases.Should().HaveCount(5);
            releases.First().Should().BeOfType<TorrentInfo>();

            var torrentInfo = (TorrentInfo)releases.First();

            torrentInfo.Peers.Should().NotHaveValue();
            torrentInfo.Seeders.Should().NotHaveValue();
        }

        [Test]
        public void should_not_set_seeders_to_null_if_has_peers()
        {
            // Atm, Kickass supplies 0 as seeders and leechers on the rss feed (but not the site), so set it to null if there aren't any peers.
            var recentFeed = ReadAllText(@"Files/Indexers/KickassTorrents/KickassTorrents.xml");

            recentFeed = recentFeed.Replace("<pubDate>Mon, 12 May 2014 16:16:49 +0000</pubDate>", string.Format("<pubDate>{0:R}</pubDate>", DateTime.UtcNow));
            recentFeed = Regex.Replace(recentFeed, @"(seeds)\>\d*", "$1>0");

            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Execute(It.Is<HttpRequest>(v => v.Method == HttpMethod.GET)))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), recentFeed));

            var releases = Subject.FetchRecent();

            releases.Should().HaveCount(5);
            releases.First().Should().BeOfType<TorrentInfo>();

            var torrentInfo = (TorrentInfo)releases.First();

            torrentInfo.Peers.Should().Be(311);
            torrentInfo.Seeders.Should().Be(0);
        }

        [Test]
        public void should_not_set_seeders_to_null_if_older_than_12_hours()
        {
            // Atm, Kickass supplies 0 as seeders and leechers on the rss feed (but not the site), so set it to null if there aren't any peers.
            var recentFeed = ReadAllText(@"Files/Indexers/KickassTorrents/KickassTorrents.xml");

            recentFeed = Regex.Replace(recentFeed, @"(seeds|peers)\>\d*", "$1>0");

            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Execute(It.Is<HttpRequest>(v => v.Method == HttpMethod.GET)))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), recentFeed));

            var releases = Subject.FetchRecent();

            releases.Should().HaveCount(5);
            releases.First().Should().BeOfType<TorrentInfo>();

            var torrentInfo = (TorrentInfo)releases.First();

            torrentInfo.Peers.Should().Be(0);
            torrentInfo.Seeders.Should().Be(0);
        }

        
        [Test]
        public void should_handle_xml_with_html_accents()
        {
            var recentFeed = ReadAllText(@"Files/Indexers/KickassTorrents/KickassTorrents_accents.xml");

            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Execute(It.Is<HttpRequest>(v => v.Method == HttpMethod.GET)))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), recentFeed));

            var releases = Subject.FetchRecent();

            releases.Should().HaveCount(5);
        }
    }
}
