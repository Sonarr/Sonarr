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

            releases.Should().HaveCount(100);

            releases.First().Should().BeOfType<TorrentInfo>();
            var releaseInfo = releases.First() as TorrentInfo;



            releaseInfo.Title.Should().Be("X Company S01E04 720p HDTV x264-CROOKS");
            releaseInfo.DownloadProtocol.Should().Be(DownloadProtocol.Torrent);
            releaseInfo.DownloadUrl.Should().Be("https://hdaccess.net/download.php?torrent=11338&apikey=123456&hit=1");
            releaseInfo.InfoUrl.Should().Be("https://hdaccess.net/details.php?torrent=11338");
            releaseInfo.CommentUrl.Should().Be("https://hdaccess.net/details.php?torrent=11338#comments");
            releaseInfo.Indexer.Should().Be(Subject.Definition.Name);
            releaseInfo.PublishDate.Should().Be(DateTime.Parse("2015/03/12 3:42:52"));
            releaseInfo.Size.Should().Be(1171198934);
            releaseInfo.TvRageId.Should().Be(43192);
            releaseInfo.InfoHash.Should().Be("123456789abcdef123456789cdefacef78945648");
            releaseInfo.Seeders.Should().Be(1);
            releaseInfo.Peers.Should().Be(2);
        }
    }
}
