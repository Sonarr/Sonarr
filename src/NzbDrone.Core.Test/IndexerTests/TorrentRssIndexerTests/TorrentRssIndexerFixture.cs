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
        public void should_parse_recent_feed_from_Ezrss()
        {
            GivenRecentFeedResponse("TorrentRss/Ezrss.xml");

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

        [Test]
        public void should_parse_recent_feed_from_Doki()
        {
            Subject.Definition.Settings.As<TorrentRssIndexerSettings>().AllowZeroSize = true;

            GivenRecentFeedResponse("TorrentRss/Doki.xml");

            var releases = Subject.FetchRecent();

            releases.Should().HaveCount(5);
            releases.First().Should().BeOfType<TorrentInfo>();

            var torrentInfo = releases.First() as TorrentInfo;

            torrentInfo.Title.Should().Be("[Doki] PriPara   50 (848x480 h264 AAC) [6F0B49FD] mkv");
            torrentInfo.DownloadProtocol.Should().Be(DownloadProtocol.Torrent);
            torrentInfo.DownloadUrl.Should().Be("http://tracker.anime-index.org/download.php?id=82d8ad84403e01a7786130905ca169a3429e657f&f=%5BDoki%5D+PriPara+-+50+%28848x480+h264+AAC%29+%5B6F0B49FD%5D.mkv.torrent");
            torrentInfo.InfoUrl.Should().BeNullOrEmpty();
            torrentInfo.CommentUrl.Should().BeNullOrEmpty();
            torrentInfo.Indexer.Should().Be(Subject.Definition.Name);
            torrentInfo.PublishDate.Should().Be(DateTime.Parse("Thu, 02 Jul 2015 08:18:29 GMT").ToUniversalTime());
            torrentInfo.Size.Should().Be(0);
            torrentInfo.InfoHash.Should().BeNull();
            torrentInfo.MagnetUrl.Should().BeNull();
            torrentInfo.Peers.Should().NotHaveValue();
            torrentInfo.Seeders.Should().NotHaveValue();
        }

        [Test]
        public void should_parse_recent_feed_from_BeyondHD()
        {
            GivenRecentFeedResponse("TorrentRss/BeyondHD.xml");

            var releases = Subject.FetchRecent();

            releases.Should().HaveCount(5);
            foreach (var release in releases)
            {
                release.Should().BeOfType<TorrentInfo>();
            }

            var torrentInfo = releases.First() as TorrentInfo;

            torrentInfo.Title.Should().Be("Tosh 0 S07E16 720p WEB-DL AAC2 0 H 264-pcsyndicate ");
            torrentInfo.DownloadProtocol.Should().Be(DownloadProtocol.Torrent);
            torrentInfo.DownloadUrl.Should().Be("https://beyondhd.me/download.php?torrent=39507&torrent_pass=0123456789abcdef0123456789abcdef");
            torrentInfo.InfoUrl.Should().BeNullOrEmpty();
            torrentInfo.CommentUrl.Should().BeNullOrEmpty();
            torrentInfo.Indexer.Should().Be(Subject.Definition.Name);
            torrentInfo.PublishDate.Should().Be((DateTime.Today + DateTime.Parse("04:27 PM").TimeOfDay).ToUniversalTime());
            torrentInfo.Size.Should().Be(672210616);
            torrentInfo.InfoHash.Should().BeNull();
            torrentInfo.MagnetUrl.Should().BeNull();
            torrentInfo.Peers.Should().Be(3);
            torrentInfo.Seeders.Should().Be(3);

            torrentInfo = releases.ElementAt(1) as TorrentInfo;

            torrentInfo.Title.Should().Be("Mr Robot S01E10 eps1 9 1 zer0-daY avi 1080p WEB-DL DD5 1 H 264-NTb ");
            torrentInfo.DownloadProtocol.Should().Be(DownloadProtocol.Torrent);
            torrentInfo.DownloadUrl.Should().Be("https://beyondhd.me/download.php?torrent=39504&torrent_pass=0123456789abcdef0123456789abcdef");
            torrentInfo.InfoUrl.Should().BeNullOrEmpty();
            torrentInfo.CommentUrl.Should().BeNullOrEmpty();
            torrentInfo.Indexer.Should().Be(Subject.Definition.Name);
            torrentInfo.PublishDate.Should().Be((DateTime.Today.Date - TimeSpan.FromDays(1) + DateTime.Parse("10:52 AM").TimeOfDay).ToUniversalTime());
            torrentInfo.Size.Should().Be(2254857830);
            torrentInfo.InfoHash.Should().BeNull();
            torrentInfo.MagnetUrl.Should().BeNull();
            torrentInfo.Peers.Should().Be(31);
            torrentInfo.Seeders.Should().Be(31);

            torrentInfo = releases.ElementAt(3) as TorrentInfo;

            torrentInfo.Title.Should().Be("The Last Ship S02E12 1080p WEB-DL DD5 1 H 264-pcsyndicate ");
            torrentInfo.DownloadProtocol.Should().Be(DownloadProtocol.Torrent);
            torrentInfo.DownloadUrl.Should().Be("https://beyondhd.me/download.php?torrent=39493&torrent_pass=0123456789abcdef0123456789abcdef");
            torrentInfo.InfoUrl.Should().BeNullOrEmpty();
            torrentInfo.CommentUrl.Should().BeNullOrEmpty();
            torrentInfo.Indexer.Should().Be(Subject.Definition.Name);
            torrentInfo.PublishDate.Should().Be(DateTime.Parse("2 Sep 2015"));
            torrentInfo.Size.Should().Be(1717986918);
            torrentInfo.InfoHash.Should().BeNull();
            torrentInfo.MagnetUrl.Should().BeNull();
            torrentInfo.Peers.Should().Be(7);
            torrentInfo.Seeders.Should().Be(7);
        }

        [Test]
        public void should_parse_recent_feed_from_HD4Free()
        {
            GivenRecentFeedResponse("TorrentRss/HD4Free.xml");

            var releases = Subject.FetchRecent();

            releases.Should().HaveCount(5);
            foreach (var release in releases)
            {
                release.Should().BeOfType<TorrentInfo>();
            }

            var torrentInfo = releases.First() as TorrentInfo;

            torrentInfo.Title.Should().Be("Extant S02E11 1080p WEB-DL DD5.1 H.264-KiNGS");
            torrentInfo.DownloadProtocol.Should().Be(DownloadProtocol.Torrent);
            torrentInfo.DownloadUrl.Should().Be("https://hd4free.xyz/download.php?torrent=12074&torrent_pass=0123456789abcdef0123456789abcdef&ssl=1");
            torrentInfo.InfoUrl.Should().BeNullOrEmpty();
            torrentInfo.CommentUrl.Should().BeNullOrEmpty();
            torrentInfo.Indexer.Should().Be(Subject.Definition.Name);
            torrentInfo.PublishDate.Should().Be((DateTime.Today + DateTime.Parse("12:51 AM").TimeOfDay).ToUniversalTime());
            torrentInfo.Size.Should().Be(1750199173);
            torrentInfo.InfoHash.Should().BeNull();
            torrentInfo.MagnetUrl.Should().BeNull();
            torrentInfo.Peers.Should().Be(4);
            torrentInfo.Seeders.Should().Be(3);

            torrentInfo = releases.ElementAt(1) as TorrentInfo;

            torrentInfo.Title.Should().Be("Tosh 0 S07E16 1080p WEB-DL AAC2.0 H.264-pcsyndicate");
            torrentInfo.DownloadProtocol.Should().Be(DownloadProtocol.Torrent);
            torrentInfo.DownloadUrl.Should().Be("https://hd4free.xyz/download.php?torrent=12054&torrent_pass=0123456789abcdef0123456789abcdef&ssl=1");
            torrentInfo.InfoUrl.Should().BeNullOrEmpty();
            torrentInfo.CommentUrl.Should().BeNullOrEmpty();
            torrentInfo.Indexer.Should().Be(Subject.Definition.Name);
            torrentInfo.PublishDate.Should().Be((DateTime.Today.Date - TimeSpan.FromDays(1) + DateTime.Parse("04:17 PM").TimeOfDay).ToUniversalTime());
            torrentInfo.Size.Should().Be(835222241);
            torrentInfo.InfoHash.Should().BeNull();
            torrentInfo.MagnetUrl.Should().BeNull();
            torrentInfo.Peers.Should().Be(2);
            torrentInfo.Seeders.Should().Be(2);

            torrentInfo = releases.ElementAt(3) as TorrentInfo;

            torrentInfo.Title.Should().Be("The Last Ship S02E12 1080p WEB-DL DD5.1 H.264-pcsyndicate");
            torrentInfo.DownloadProtocol.Should().Be(DownloadProtocol.Torrent);
            torrentInfo.DownloadUrl.Should().Be("https://hd4free.xyz/download.php?torrent=12019&torrent_pass=0123456789abcdef0123456789abcdef&ssl=1");
            torrentInfo.InfoUrl.Should().BeNullOrEmpty();
            torrentInfo.CommentUrl.Should().BeNullOrEmpty();
            torrentInfo.Indexer.Should().Be(Subject.Definition.Name);
            torrentInfo.PublishDate.Should().Be(DateTime.Parse("2 Sep 2015"));
            torrentInfo.Size.Should().Be(1717986918);
            torrentInfo.InfoHash.Should().BeNull();
            torrentInfo.MagnetUrl.Should().BeNull();
            torrentInfo.Peers.Should().Be(2);
            torrentInfo.Seeders.Should().Be(2);
        }

    }
}
