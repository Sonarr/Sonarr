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
        private String _recentFeed;

        [TestFixtureSetUp]
        public void SetupFixture()
        {
            Subject.Definition = new IndexerDefinition()
                {
                    Name = "Nyaa",
                    Settings = new NyaaSettings()
                };

            _recentFeed = ReadAllText(@"Files/RSS/Nyaa.xml");
        }

        [Test]
        public void Indexer_TestFeedParser_Nyaa()
        {
            var httpClientMock = Mocker.GetMock<IHttpClient>();
            httpClientMock.Setup(o => o.Get(It.IsAny<HttpRequest>()))
                          .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), _recentFeed));

            var httpClient = httpClientMock.Object;

            var fetchService = new FetchFeedService(httpClient, TestLogger);
            var releases = fetchService.FetchRss(Subject);

            releases.Should().HaveCount(4);

            var firstRelease = releases.First();

            Assert.IsInstanceOf<TorrentInfo>(firstRelease);

            var torrentInfo = (TorrentInfo)firstRelease;

            torrentInfo.Title.Should().Be("[TSRaws] Futsuu no Joshikousei ga [Locodol] Yattemita. #07 (TBS).ts");
            torrentInfo.DownloadProtocol.Should().Be(DownloadProtocol.Torrent);
            torrentInfo.DownloadUrl.Should().Be("http://www.nyaa.se/?page=download&tid=587750");
            torrentInfo.Indexer.Should().Be(Subject.Definition.Name);
            firstRelease.PublishDate.Should().Be(DateTime.Parse("2014/08/14 18:10:36"));
            torrentInfo.Size.Should().Be(2523293286); //2.35 GiB
            torrentInfo.InfoHash.Should().Be(null);
            torrentInfo.MagnetUrl.Should().Be(null);
            torrentInfo.Peers.Should().Be(2);
            torrentInfo.Seeds.Should().Be(1);
        }
    }
}
