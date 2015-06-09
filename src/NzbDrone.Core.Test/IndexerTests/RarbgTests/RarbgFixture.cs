using System;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.Rarbg;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.IndexerTests.RarbgTests
{
    [TestFixture]
    public class RarbgFixture : CoreTest<Rarbg>
    {
        [SetUp]
        public void Setup()
        {
            Subject.Definition = new IndexerDefinition()
                {
                    Name = "Rarbg",
                    Settings = new RarbgSettings()
                };

            Mocker.GetMock<IRarbgTokenProvider>()
                .Setup(v => v.GetToken(It.IsAny<RarbgSettings>()))
                .Returns("validtoken");
        }

        [Test]
        public void should_parse_recent_feed_from_Rarbg()
        {
            var recentFeed = ReadAllText(@"Files/Indexers/Rarbg/RecentFeed.json");

            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Execute(It.Is<HttpRequest>(v => v.Method == HttpMethod.GET)))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), recentFeed));

            var releases = Subject.FetchRecent();

            releases.Should().HaveCount(3);
            releases.First().Should().BeOfType<TorrentInfo>();

            var torrentInfo = releases.First() as TorrentInfo;

            torrentInfo.Title.Should().Be("Thunderbirds.Are.Go.S01E09.Slingshot.1080p.WEB-DL.AAC2.0.H.264-Coo7[rartv]");
            torrentInfo.DownloadProtocol.Should().Be(DownloadProtocol.Torrent);
            torrentInfo.DownloadUrl.Should().Be("magnet:?xt=urn:btih:ff4737b5230307836ec8abce6ab73727f1358bf3&dn=Thunderbirds.Are.Go.S01E09.Slingshot.1080p.WEB-DL.AAC2.0.H.264-Coo7%5Brartv%5D&tr=http%3A%2F%2Ftracker.trackerfix.com%3A80%2Fannounce&tr=udp%3A%2F%2F9.rarbg.me%3A2710&tr=udp%3A%2F%2F9.rarbg.to%3A2710&tr=udp%3A%2F%2Fopen.demonii.com%3A1337%2Fannounce");
            torrentInfo.Indexer.Should().Be(Subject.Definition.Name);
            torrentInfo.PublishDate.Should().Be(DateTime.Parse("2015-05-24 19:36:09"));
            torrentInfo.Size.Should().Be(896238116);
            torrentInfo.InfoHash.Should().BeNull();
            torrentInfo.MagnetUrl.Should().BeNull();
            torrentInfo.Peers.Should().Be(44+19);
            torrentInfo.Seeders.Should().Be(44);
        }
    }
}
