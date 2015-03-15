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

            releaseInfo.Title.Should().Be("The 100 S02 720p HDTV x264-Scene");
            releaseInfo.DownloadProtocol.Should().Be(DownloadProtocol.Torrent);
            releaseInfo.DownloadUrl.Should().Be("https://hdaccess.net/download.php?torrent=11379&passkey=123456");
            releaseInfo.InfoUrl.Should().Be("https://hdaccess.net/details.php?id=11379&hit=1");
            releaseInfo.CommentUrl.Should().Be("https://hdaccess.net/details.php?id=11379&hit=1#comments");
            releaseInfo.Indexer.Should().Be(Subject.Definition.Name);
            releaseInfo.PublishDate.Should().Be(DateTime.Parse("2015/03/12 20:53:34"));
            releaseInfo.Size.Should().Be(25993658561);
            releaseInfo.TvRageId.Should().Be(0);
            releaseInfo.InfoHash.Should().Be("df068e3b4245c0bb754eaa1e3891ca1f63635e80");
            releaseInfo.Seeders.Should().Be(1);
            releaseInfo.Peers.Should().Be(1);
        }
    }
}
