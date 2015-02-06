using System;
using System.Linq;

using Moq;

using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

using FluentAssertions;

using NUnit.Framework;

using NzbDrone.Core.Indexers.ImmortalSeed;

namespace NzbDrone.Core.Test.IndexerTests.ImmortalSeedTests
{
    [TestFixture]
    public class ImmortalSeedFixture : CoreTest<ImmortalSeed>
    {
        [SetUp]
        public void Setup()
        {
            Subject.Definition = new IndexerDefinition()
            {
                Name = "ImmortalSeed",
                Settings = new ImmortalSeedSettings() { BaseUrl = "https://immortalseed.me/rss.php?secret_key=12345678910&feedtype=download&timezone=-12&showrows=50&categories=8" }
            };
        }

        [Test]
        public void should_parse_recent_feed_from_ImmortalSeed()
        {
            var recentFeed = ReadAllText(@"Files/RSS/ImmortalSeed.xml");

            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Execute(It.Is<HttpRequest>(v => v.Method == HttpMethod.GET)))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), recentFeed));

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
        public void should_return_empty_list_on_404()
        {
            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Execute(It.Is<HttpRequest>(v => v.Method == HttpMethod.GET)))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), new Byte[0], System.Net.HttpStatusCode.NotFound));

            var releases = Subject.FetchRecent();

            releases.Should().HaveCount(0);

            ExceptionVerification.IgnoreWarns();
        }
    }
}
