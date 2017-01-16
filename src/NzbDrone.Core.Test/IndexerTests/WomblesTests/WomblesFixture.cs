using System;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.Wombles;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Test.IndexerTests.WomblesTests
{

    [TestFixture]
    public class TorrentRssIndexerFixture : CoreTest<Wombles>
    {
        [SetUp]
        public void Setup()
        {

            Subject.Definition = new IndexerDefinition()
            {
                Name = "Wombles",
                Settings = new NullConfig(),
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
        public void should_parse_recent_feed_from_wombles()
        {
            GivenRecentFeedResponse("Wombles/wombles.xml");

            var releases = Subject.FetchRecent();

            releases.Should().HaveCount(5);
            
            var releaseInfo = releases.First();

            releaseInfo.Title.Should().Be("One.Child.S01E01.720p.HDTV.x264-TLA");
            releaseInfo.DownloadProtocol.Should().Be(DownloadProtocol.Usenet);
            releaseInfo.DownloadUrl.Should().Be("http://indexer.local/nzb/bb4/One.Child.S01E01.720p.HDTV.x264-TLA.nzb");
            releaseInfo.InfoUrl.Should().BeNullOrEmpty();
            releaseInfo.CommentUrl.Should().BeNullOrEmpty();
            releaseInfo.Indexer.Should().Be(Subject.Definition.Name);
            releaseInfo.PublishDate.Should().Be(DateTime.Parse("2016-02-17 23:03:52 +0000").ToUniversalTime());
            releaseInfo.Size.Should().Be(956*1024*1024);
        }
    }
}
