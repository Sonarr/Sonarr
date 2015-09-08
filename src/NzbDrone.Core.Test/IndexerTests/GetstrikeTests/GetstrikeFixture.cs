using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.GetStrike;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;
using System;
using System.Linq;
using FluentAssertions;
using NzbDrone.Core.IndexerSearch.Definitions;
using System.Collections.Generic;

namespace NzbDrone.Core.Test.IndexerTests.GetstrikeTests
{
    [TestFixture]
    public class getStrikeFixture : CoreTest<GetStrike>
    {
        [SetUp]
        public void Setup()
        {
            Subject.Definition = new IndexerDefinition
                {
                    Name = "Getstrike",
                    Settings = new GetStrikeSettings(),
                };
        }

        [Test]
        public void should_parse_top_feed_from_Getstrike()
        {
            var recentFeed = ReadAllText(@"Files/Indexers/Getstrike/Getstrike.json");

            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Execute(It.Is<HttpRequest>(v => v.Method == HttpMethod.GET)))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), recentFeed));

            SingleEpisodeSearchCriteria s = new SingleEpisodeSearchCriteria();
            SingleEpisodeSearchCriteria searchCriteria = new SingleEpisodeSearchCriteria
                {
                    SceneTitles = new List<string> { "Series Title" }
                };
            var releases = Subject.Fetch(searchCriteria);

            releases.Should().HaveCount(100);
            releases.First().Should().BeOfType<TorrentInfo>();

            var torrentInfo = (TorrentInfo) releases.First();

            torrentInfo.Title.Should().Be("The Walking Dead S05E14 HDTV x264-KILLERS[ettv]");
            torrentInfo.DownloadProtocol.Should().Be(DownloadProtocol.Torrent);
            torrentInfo.DownloadUrl.Should().BeNullOrEmpty();
            torrentInfo.InfoUrl.Should().Be("https://getstrike.net/torrents/DE8D9A5E469BAD2A0EE16636C5FE5A7BB9AD11C3");
            torrentInfo.CommentUrl.Should().BeNullOrEmpty();
            torrentInfo.Indexer.Should().Be(Subject.Definition.Name);
            torrentInfo.PublishDate.Should().Be(DateTime.Parse("2015/03/16 00:00:00"));
            torrentInfo.Size.Should().Be(421684838);
            torrentInfo.InfoHash.Should().Be("DE8D9A5E469BAD2A0EE16636C5FE5A7BB9AD11C3");
            torrentInfo.MagnetUrl.Should().Be("magnet:?xt=urn:btih:DE8D9A5E469BAD2A0EE16636C5FE5A7BB9AD11C3&dn=The+Walking+Dead+S05E14+HDTV+x264-KILLERS%5Bettv%5D&tr=udp://open.demonii.com:1337&tr=udp://tracker.coppersurfer.tk:6969&tr=udp://tracker.leechers-paradise.org:6969&tr=udp://exodus.desync.com:6969");
            torrentInfo.Peers.Should().Be(75461+20349);
            torrentInfo.Seeders.Should().Be(75461);
        }

        [Test]
        public void should_return_empty_list_on_404()
        {
            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Execute(It.Is<HttpRequest>(v => v.Method == HttpMethod.GET)))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), new Byte[0], System.Net.HttpStatusCode.NotFound));

            SingleEpisodeSearchCriteria s = new SingleEpisodeSearchCriteria();
            SingleEpisodeSearchCriteria searchCriteria = new SingleEpisodeSearchCriteria()
            {
                SceneTitles = new List<string> { "Series Title" }
            };
            var releases = Subject.Fetch(searchCriteria);

            releases.Should().HaveCount(0);

            ExceptionVerification.IgnoreWarns();
        }
    }
}
