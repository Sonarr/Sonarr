using System;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.Nyaa;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.IndexerTests.NyaaTests
{
    [TestFixture]
    public class NyaaFixture : CoreTest<Nyaa>
    {
        [SetUp]
        public void Setup()
        {
            Subject.Definition = new IndexerDefinition()
                {
                    Name = "Nyaa",
                    Settings = new NyaaSettings()
                };
        }

        [Test]
        public void should_parse_recent_feed_from_Nyaa()
        {
            var recentFeed = ReadAllText(@"Files/Indexers/Nyaa/Nyaa.xml");

            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Execute(It.Is<HttpRequest>(v => v.Method == HttpMethod.GET)))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), recentFeed));

            var releases = Subject.FetchRecent();

            releases.Should().HaveCount(4);
            releases.First().Should().BeOfType<TorrentInfo>();

            var torrentInfo = releases.First() as TorrentInfo;

            torrentInfo.Title.Should().Be("[TSRaws] Futsuu no Joshikousei ga [Locodol] Yattemita. #07 (TBS).ts");
            torrentInfo.DownloadProtocol.Should().Be(DownloadProtocol.Torrent);
            torrentInfo.DownloadUrl.Should().Be("https://www.nyaa.se/?page=download&tid=587750");
            torrentInfo.InfoUrl.Should().Be("https://www.nyaa.se/?page=view&tid=587750");
            torrentInfo.CommentUrl.Should().BeNullOrEmpty();
            torrentInfo.Indexer.Should().Be(Subject.Definition.Name);
            torrentInfo.PublishDate.Should().Be(DateTime.Parse("2014/08/14 18:10:36"));
            torrentInfo.Size.Should().Be(2523293286); //2.35 GiB
            torrentInfo.InfoHash.Should().Be(null);
            torrentInfo.MagnetUrl.Should().Be(null);
            torrentInfo.Peers.Should().Be(2+1);
            torrentInfo.Seeders.Should().Be(1);
        }
    }
}
