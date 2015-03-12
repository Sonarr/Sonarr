using System;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.Torznab;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.IndexerTests.TorznabTests
{
    [TestFixture]
    public class TorznabFixture : CoreTest<Torznab>
    {
        [SetUp]
        public void Setup()
        {
            Subject.Definition = new IndexerDefinition()
                {
                    Name = "Torznab",
                    Settings = new TorznabSettings()
                        {
                            Url = "http://indexer.local/",
                            Categories = new Int32[] { 1 }
                        }
                };
        }

        [Test]
        public void should_parse_recent_feed_from_torznab_hdaccess_net()
        {
            var recentFeed = ReadAllText(@"Files/RSS/torznab_hdaccess_net.xml");

            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Execute(It.Is<HttpRequest>(v => v.Method == HttpMethod.GET)))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), recentFeed));
            
            var releases = Subject.FetchRecent();

            releases.Should().HaveCount(5);

            releases.First().Should().BeOfType<TorrentInfo>();
            var releaseInfo = releases.First() as TorrentInfo;

            releaseInfo.Title.Should().Be("Better Call Saul S01E05 Alpine Shepherd 1080p NF WEBRip DD5.1 x264");
            releaseInfo.DownloadProtocol.Should().Be(DownloadProtocol.Torrent);
            releaseInfo.DownloadUrl.Should().Be("https://hdaccess.net/download.php?torrent=11515&passkey=123456");
            releaseInfo.InfoUrl.Should().Be("https://hdaccess.net/details.php?id=11515&hit=1");
            releaseInfo.CommentUrl.Should().Be("https://hdaccess.net/details.php?id=11515&hit=1#comments");
            releaseInfo.Indexer.Should().Be(Subject.Definition.Name);
            releaseInfo.PublishDate.Should().Be(DateTime.Parse("2015/03/14 21:10:42"));
            releaseInfo.Size.Should().Be(2538463390);
            releaseInfo.TvRageId.Should().Be(37780);
            releaseInfo.InfoHash.Should().Be("63e07ff523710ca268567dad344ce1e0e6b7e8a3");
            releaseInfo.Seeders.Should().Be(7);
            releaseInfo.Peers.Should().Be(7);
        }
    }
}
