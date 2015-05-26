using Moq;
using NLog;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.TorrentRssIndexer;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;
using System;
using System.Linq;
using FluentAssertions;

namespace NzbDrone.Core.Test.IndexerTests.TorrentRssIndexerTests
{
    using NzbDrone.Common.Cache;

    [TestFixture]
    public class TorrentRssIndexerFixture : CoreTest<TestTorrentRssIndexer>
    {
        private readonly Logger _logger  = LogManager.GetCurrentClassLogger();

        [SetUp]
        public void Setup()
        {
            Subject.Definition = new IndexerDefinition()
            {
                Name = "TorrentRssIndexer",
                Settings = new TorrentRssIndexerSettings() { },
            };

            Mocker.SetConstant<ITorrentRssParserFactory>(new TorrentRssParserFactory(Mocker.Resolve<CacheManager>(), new TorrentRssSettingsDetector(new HttpClient(new CacheManager(), TestLogger), TestLogger), TestLogger));
        }

        [TestCase("https://www.ezrss.it/", "Eztv.xml")]
        [TestCase("https://www.speed.cd/", "speed.cd.xml")]
        //[TestCase("https://www.showrss.info/", "showrss.info.xml")]
        [TestCase("https://immortalseed.me/rss.php?secret_key=12345678910&feedtype=download&timezone=-12&showrows=50&categories=8", "ImmortalSeed.xml")]
        public void should_detect_and_parse_recent_feed(string baseUrl, string rssXmlFile)
        {
            Subject.Definition.Settings = new TorrentRssIndexerSettings { BaseUrl = baseUrl };

            var recentFeed = ReadAllText(@"Files/RSS/" + rssXmlFile);

            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Execute(It.Is<HttpRequest>(v => v.Method == HttpMethod.GET)))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), recentFeed));

            Subject.TestPublic().Should().BeEmpty();
        }

        [TestCase("https://www.ezrss.it/1", "Eztv_InvalidSize.xml")]
        [TestCase("https://www.ezrss.it/2", "Eztv_InvalidTitles.xml")]
        [TestCase("https://www.ezrss.it/3", "Eztv_InvalidDownloadUrl.xml")]
        [TestCase("https://immortalseed.me/rss.php?secret_key=12345678910&feedtype=download&timezone=-12&showrows=50&categories=8", "ImmortalSeed_InvalidSize.xml")]
        [TestCase("https://immortalseed.me/rss.php?secret_key=12345678910&feedtype=download&timezone=-12&showrows=50&categories=9", "ImmortalSeed_InvalidTitles.xml")]
        [TestCase("https://immortalseed.me/rss.php?secret_key=12345678910&feedtype=download&timezone=-12&showrows=50&categories=10", "ImmortalSeed_InvalidDownloadUrl.xml")]
        public void should_reject_recent_feed(string baseUrl, string rssXmlFile)
        {
            Subject.Definition.Settings = new TorrentRssIndexerSettings { BaseUrl = baseUrl };

            var recentFeed = ReadAllText(@"Files/RSS/" + rssXmlFile);

            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Execute(It.Is<HttpRequest>(v => v.Method == HttpMethod.GET)))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), recentFeed));

            Subject.TestPublic().Should().HaveCount(1);
            ExceptionVerification.IgnoreWarns();
        }

        [Test]
        public void should_parse_recent_feed_from_ImmortalSeed()
        {
            Subject.Definition.Settings = new TorrentRssIndexerSettings { BaseUrl = "https://immortalseed.me/rss.php?secret_key=12345678910&feedtype=download&timezone=-12&showrows=50&categories=8" };
            
            var recentFeed = ReadAllText(@"Files/RSS/ImmortalSeed.xml");

            _logger.Debug("Feed: [{0}]", recentFeed);

            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Execute(It.Is<HttpRequest>(v => v.Method == HttpMethod.GET)))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), recentFeed));

            _logger.Trace("Test");
            var releases = Subject.FetchRecent();

            releases.Should().HaveCount(50);
            releases.First().Should().BeOfType<TorrentInfo>();
            
            var torrentInfo = (TorrentInfo)releases.First();

            torrentInfo.Title.Should().Be("Conan.2015.02.05.Jeff.Bridges.720p.HDTV.X264-CROOKS");
            torrentInfo.DownloadProtocol.Should().Be(DownloadProtocol.Torrent);
            torrentInfo.DownloadUrl.Should().Be("https://immortalseed.me/download.php?type=rss&secret_key=12345678910&id=374534");
            torrentInfo.InfoUrl.Should().BeNullOrEmpty();
            torrentInfo.CommentUrl.Should().BeNullOrEmpty();
            torrentInfo.Indexer.Should().Be(Subject.Definition.Name);
            torrentInfo.PublishDate.Should().Be(DateTime.Parse("2015-02-06 12:32:26"));
            torrentInfo.Size.Should().Be(984078090);
            torrentInfo.InfoHash.Should().BeNullOrEmpty();
            torrentInfo.MagnetUrl.Should().BeNullOrEmpty();
            torrentInfo.Peers.Should().Be(8);
            torrentInfo.Seeders.Should().Be(6);
        }

        [Test]
        public void should_parse_recent_feed_from_Eztv()
        {
            Subject.Definition.Settings = new TorrentRssIndexerSettings { BaseUrl = "https://www.ezrss.it/" };

            var recentFeed = ReadAllText(@"Files/RSS/Eztv.xml");

            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Execute(It.Is<HttpRequest>(v => v.Method == HttpMethod.GET)))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), recentFeed));

            var releases = Subject.FetchRecent();

            releases.Should().HaveCount(3);
            releases.First().Should().BeOfType<TorrentInfo>();

            var torrentInfo = releases.First() as TorrentInfo;

            torrentInfo.Title.Should().Be("S4C I Grombil Cyfandir Pell American Interior [PDTV - MVGROUP]");
            torrentInfo.DownloadProtocol.Should().Be(DownloadProtocol.Torrent);
            torrentInfo.DownloadUrl.Should().Be("http://re.zoink.it/20a4ed4eFC");
            torrentInfo.InfoUrl.Should().Be("http://eztv.it/ep/58439/s4c-i-grombil-cyfandir-pell-american-interior-pdtv-x264-mvgroup/");
            torrentInfo.CommentUrl.Should().Be("http://eztv.it/forum/discuss/58439/");
            torrentInfo.Indexer.Should().Be(Subject.Definition.Name);
            torrentInfo.PublishDate.Should().Be(DateTime.Parse("2014/09/15 18:39:00"));
            torrentInfo.Size.Should().Be(796606175);
            torrentInfo.InfoHash.Should().Be("20FC4FBFA88272274AC671F857CC15144E9AA83E");
            torrentInfo.MagnetUrl.Should().Be("magnet:?xt=urn:btih:ED6E7P5IQJZCOSWGOH4FPTAVCRHJVKB6&dn=S4C.I.Grombil.Cyfandir.Pell.American.Interior.PDTV.x264-MVGroup");
            torrentInfo.Peers.Should().NotHaveValue();
            torrentInfo.Seeders.Should().NotHaveValue();
        }
    }
}
