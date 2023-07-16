using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
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
                .Setup(o => o.ExecuteAsync(It.IsAny<HttpRequest>()))
                .Returns<HttpRequest>(r => Task.FromResult(new HttpResponse(r, new HttpHeader(), recentFeed)));

            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Execute(It.IsAny<HttpRequest>()))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), recentFeed));
        }

        [Test]
        public async Task should_parse_recent_feed_from_ImmortalSeed()
        {
            GivenRecentFeedResponse("TorrentRss/ImmortalSeed.xml");

            var releases = await Subject.FetchRecent();

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
        public async Task should_parse_recent_feed_from_Ezrss()
        {
            GivenRecentFeedResponse("TorrentRss/Ezrss.xml");

            var releases = await Subject.FetchRecent();

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
        public async Task should_parse_recent_feed_from_ShowRSS_info()
        {
            Subject.Definition.Settings.As<TorrentRssIndexerSettings>().AllowZeroSize = true;

            GivenRecentFeedResponse("TorrentRss/ShowRSS.info.xml");

            var releases = await Subject.FetchRecent();

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
        public async Task should_parse_recent_feed_from_Doki()
        {
            Subject.Definition.Settings.As<TorrentRssIndexerSettings>().AllowZeroSize = true;

            GivenRecentFeedResponse("TorrentRss/Doki.xml");

            var releases = await Subject.FetchRecent();

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
        public async Task should_parse_recent_feed_from_ExtraTorrents()
        {
            GivenRecentFeedResponse("TorrentRss/ExtraTorrents.xml");

            var releases = await Subject.FetchRecent();

            releases.Should().HaveCount(5);
            releases.First().Should().BeOfType<TorrentInfo>();

            var torrentInfo = releases.First() as TorrentInfo;

            torrentInfo.Title.Should().Be("One.Piece.E334.D ED.720p.HDTV.x264-W4F-={SPARROW}=-");
            torrentInfo.DownloadProtocol.Should().Be(DownloadProtocol.Torrent);
            torrentInfo.DownloadUrl.Should().Be("http://ac.me/download/4722030/One.Piece.E334.D+ED.720p.HDTV.x264-W4F-%3D%7BSPARROW%7D%3D-.torrent");
            torrentInfo.InfoUrl.Should().BeNullOrEmpty();
            torrentInfo.CommentUrl.Should().BeNullOrEmpty();
            torrentInfo.Indexer.Should().Be(Subject.Definition.Name);
            torrentInfo.PublishDate.Should().Be(DateTime.Parse("Sun, 21 Feb 2016 09:51:54 +0000").ToUniversalTime());
            torrentInfo.Size.Should().Be(562386947);
            torrentInfo.InfoHash.Should().BeNull();
            torrentInfo.MagnetUrl.Should().BeNull();
            torrentInfo.Peers.Should().NotHaveValue();
            torrentInfo.Seeders.Should().NotHaveValue();
        }

        [Test]
        public async Task should_parse_recent_feed_from_LimeTorrents()
        {
            GivenRecentFeedResponse("TorrentRss/LimeTorrents.xml");

            var releases = await Subject.FetchRecent();

            releases.Should().HaveCount(5);
            releases.First().Should().BeOfType<TorrentInfo>();

            var torrentInfo = releases.First() as TorrentInfo;

            torrentInfo.Title.Should().Be("The Expanse 2x04 (720p-HDTV-x264-SVA)[VTV]");
            torrentInfo.DownloadProtocol.Should().Be(DownloadProtocol.Torrent);
            torrentInfo.DownloadUrl.Should().Be("http://itorrents.org/torrent/51C578C9823DD58F6EEA287C368ED935843D63AB.torrent?title=The-Expanse-2x04-(720p-HDTV-x264-SVA)[VTV]");
            torrentInfo.InfoUrl.Should().BeNullOrEmpty();
            torrentInfo.CommentUrl.Should().Be("http://www.limetorrents.cc/The-Expanse-2x04-(720p-HDTV-x264-SVA)[VTV]-torrent-8643587.html");
            torrentInfo.Indexer.Should().Be(Subject.Definition.Name);
            torrentInfo.PublishDate.Should().Be(DateTime.Parse("16 Feb 2017 05:24:26 +0300").ToUniversalTime());
            torrentInfo.Size.Should().Be(880496711);
            torrentInfo.InfoHash.Should().BeNull();
            torrentInfo.MagnetUrl.Should().BeNull();
            torrentInfo.Peers.Should().NotHaveValue();
            torrentInfo.Seeders.Should().NotHaveValue();
        }

        [Test]
        public async Task should_parse_recent_feed_from_AnimeTosho_without_size()
        {
            GivenRecentFeedResponse("TorrentRss/AnimeTosho_NoSize.xml");

            var releases = await Subject.FetchRecent();

            releases.Should().HaveCount(2);
            releases.First().Should().BeOfType<TorrentInfo>();

            var torrentInfo = releases.First() as TorrentInfo;

            torrentInfo.Title.Should().Be("[FFF] Ore Monogatari!! - Vol.01 [BD][720p-AAC]");
            torrentInfo.DownloadProtocol.Should().Be(DownloadProtocol.Torrent);
            torrentInfo.DownloadUrl.Should().Be("http://storage.animetosho.org/torrents/85a570f25067f69b3c83b901ce6c00c491345288/%5BFFF%5D%20Ore%20Monogatari%21%21%20-%20Vol.01%20%5BBD%5D%5B720p-AAC%5D.torrent");
            torrentInfo.InfoUrl.Should().BeNullOrEmpty();
            torrentInfo.CommentUrl.Should().Be("https://animetosho.org/view/fff-ore-monogatari-vol-01-bd-720p-aac.1009077");
            torrentInfo.Indexer.Should().Be(Subject.Definition.Name);
            torrentInfo.PublishDate.Should().Be(DateTime.Parse("Tue, 02 Aug 2016 13:48:04 +0000").ToUniversalTime());
            torrentInfo.Size.Should().Be((long)Math.Round(1.366D  * 1024L * 1024L * 1024L));
            torrentInfo.InfoHash.Should().BeNull();
            torrentInfo.MagnetUrl.Should().BeNull();
            torrentInfo.Peers.Should().NotHaveValue();
            torrentInfo.Seeders.Should().NotHaveValue();
        }

        [Test]
        public async Task should_parse_multi_enclosure_from_AnimeTosho()
        {
            GivenRecentFeedResponse("TorrentRss/AnimeTosho_NoSize.xml");

            var releases = await Subject.FetchRecent();

            releases.Should().HaveCount(2);
            releases.Last().Should().BeOfType<TorrentInfo>();

            var torrentInfo = releases.Last() as TorrentInfo;

            torrentInfo.Title.Should().Be("DAYS - 05 (1280x720 HEVC2 AAC).mkv");
            torrentInfo.DownloadProtocol.Should().Be(DownloadProtocol.Torrent);
            torrentInfo.DownloadUrl.Should().Be("http://storage.animetosho.org/torrents/4b58360143d59a55cbd922397a3eaa378165f3ff/DAYS%20-%2005%20%281280x720%20HEVC2%20AAC%29.torrent");
        }

        [Test]
        public async Task should_parse_recent_feed_from_AlphaRatio()
        {
            GivenRecentFeedResponse("TorrentRss/AlphaRatio.xml");

            var releases = await Subject.FetchRecent();

            releases.Should().HaveCount(2);
            releases.Last().Should().BeOfType<TorrentInfo>();

            var torrentInfo = releases.Last() as TorrentInfo;

            torrentInfo.Title.Should().Be("TvHD 465860 465831 WWE.RAW.2016.11.28.720p.HDTV.x264-KYR");
            torrentInfo.DownloadProtocol.Should().Be(DownloadProtocol.Torrent);
            torrentInfo.DownloadUrl.Should().Be("https://alpharatio.cc/torrents.php?action=download&authkey=private_auth_key&torrent_pass=private_torrent_pass&id=465831");
        }

        [Test]
        public async Task should_parse_recent_feed_from_EveolutionWorld_without_size()
        {
            Subject.Definition.Settings.As<TorrentRssIndexerSettings>().AllowZeroSize = true;
            GivenRecentFeedResponse("TorrentRss/EvolutionWorld.xml");

            var releases = await Subject.FetchRecent();

            releases.Should().HaveCount(2);
            releases.First().Should().BeOfType<TorrentInfo>();

            var torrentInfo = releases.First() as TorrentInfo;

            torrentInfo.Title.Should().Be("[TVShow --> TVShow Bluray 720p] Fargo S01 Complete Season 1 720p BRRip DD5.1 x264-PSYPHER [SEEDERS (3)/LEECHERS (0)]");
            torrentInfo.DownloadProtocol.Should().Be(DownloadProtocol.Torrent);
            torrentInfo.DownloadUrl.Should().Be("http://ew.pw/download.php?id=dea071a7a62a0d662538d46402fb112f30b8c9fa&f=Fargo%20S01%20Complete%20Season%201%20720p%20BRRip%20DD5.1%20x264-PSYPHER.torrent&auth=secret");
            torrentInfo.InfoUrl.Should().BeNullOrEmpty();
            torrentInfo.CommentUrl.Should().BeNullOrEmpty();
            torrentInfo.Indexer.Should().Be(Subject.Definition.Name);
            torrentInfo.PublishDate.Should().Be(DateTime.Parse("2017-08-13T22:21:43Z").ToUniversalTime());
            torrentInfo.Size.Should().Be(0);
            torrentInfo.InfoHash.Should().BeNull();
            torrentInfo.MagnetUrl.Should().BeNull();
            torrentInfo.Peers.Should().NotHaveValue();
            torrentInfo.Seeders.Should().NotHaveValue();
        }

        [Test]
        public async Task should_record_indexer_failure_if_unsupported_feed()
        {
            GivenRecentFeedResponse("TorrentRss/invalid/TorrentDay_NoPubDate.xml");

            var releases = await Subject.FetchRecent();

            releases.Should().BeEmpty();

            Mocker.GetMock<IIndexerStatusService>()
                  .Verify(v => v.RecordFailure(It.IsAny<int>(), TimeSpan.Zero), Times.Once());

            ExceptionVerification.ExpectedErrors(1);
        }
    }
}
