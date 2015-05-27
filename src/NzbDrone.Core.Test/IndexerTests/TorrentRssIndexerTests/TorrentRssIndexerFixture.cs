using System;
using System.Linq;
using FluentAssertions;
using Moq;
using NLog;
using NUnit.Framework;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.TorrentRss;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.IndexerTests.TorrentRssIndexerTests
{

    [TestFixture]
    public class TorrentRssIndexerFixture : CoreTest<TestTorrentRssIndexer>
    {
        private const string _indexerUrl = "http://my.indexer.tv/recent";

        [SetUp]
        public void Setup()
        {
            Mocker.SetConstant<IHttpClient>(Mocker.GetMock<IHttpClient>().Object);
            Mocker.SetConstant<ICacheManager>(Mocker.Resolve<CacheManager>());
            Mocker.SetConstant<ITorrentRssSettingsDetector>(Mocker.Resolve<TorrentRssSettingsDetector>());
            Mocker.SetConstant<ITorrentRssParserFactory>(Mocker.Resolve<TorrentRssParserFactory>());

            Subject.Definition = new IndexerDefinition()
            {
                Name = "TorrentRssIndexer",
                Settings = new TorrentRssIndexerSettings() { BaseUrl = _indexerUrl },
            };
        }

        private void GivenRecentFeedResponse(string rssXmlFile)
        {
            var recentFeed = ReadAllText(@"Files/Indexers/" + rssXmlFile);

            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Execute(It.IsAny<HttpRequest>()))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), recentFeed));
        }

        [Test]
        public void should_parse_recent_feed_from_ImmortalSeed()
        {
            GivenRecentFeedResponse("TorrentRss/ImmortalSeed.xml");

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
            torrentInfo.PublishDate.Should().Be(DateTime.Parse("2015-02-06 13:32:26"));
            torrentInfo.Size.Should().Be(984078090);
            torrentInfo.InfoHash.Should().BeNullOrEmpty();
            torrentInfo.MagnetUrl.Should().BeNullOrEmpty();
            torrentInfo.Peers.Should().Be(8);
            torrentInfo.Seeders.Should().Be(6);
        }

        [Test]
        public void should_parse_recent_feed_from_Eztv()
        {
            GivenRecentFeedResponse("Eztv/Eztv.xml");

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

        [Test]
        public void should_parse_recent_feed_from_ShowRSS_info()
        {
            Subject.Definition.Settings.As<TorrentRssIndexerSettings>().AllowZeroSize = true;

            GivenRecentFeedResponse("TorrentRss/ShowRSS.info.xml");

            var releases = Subject.FetchRecent();

            releases.Should().HaveCount(5);
            releases.First().Should().BeOfType<TorrentInfo>();

            var torrentInfo = releases.First() as TorrentInfo;

            torrentInfo.Title.Should().Be("The Voice 8x25");
            torrentInfo.DownloadProtocol.Should().Be(DownloadProtocol.Torrent);
            torrentInfo.DownloadUrl.Should().Be("magnet:?xt=urn:btih:96CD620BEDA3EFD7C4D7746EF94549D03A2EB13B&dn=The+Voice+S08E25+WEBRip+x264+WNN&tr=udp://tracker.coppersurfer.tk:6969/announce&tr=udp://tracker.leechers-paradise.org:6969&tr=udp://open.demonii.com:1337");
            torrentInfo.InfoUrl.Should().BeNullOrEmpty();
            torrentInfo.CommentUrl.Should().BeNullOrEmpty();
            torrentInfo.Indexer.Should().Be(Subject.Definition.Name);
            torrentInfo.PublishDate.Should().Be(DateTime.Parse("2015/05/15 08:30:01"));
            torrentInfo.Size.Should().Be(0);
            torrentInfo.InfoHash.Should().Be("96CD620BEDA3EFD7C4D7746EF94549D03A2EB13B");
            torrentInfo.MagnetUrl.Should().Be("magnet:?xt=urn:btih:96CD620BEDA3EFD7C4D7746EF94549D03A2EB13B&dn=The+Voice+S08E25+WEBRip+x264+WNN&tr=udp://tracker.coppersurfer.tk:6969/announce&tr=udp://tracker.leechers-paradise.org:6969&tr=udp://open.demonii.com:1337");
            torrentInfo.Peers.Should().NotHaveValue();
            torrentInfo.Seeders.Should().NotHaveValue();
        }
    }
}
