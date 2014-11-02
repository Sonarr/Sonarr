using System;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.NzbIndex;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.IndexerTests.NzbIndexTests
{
    [TestFixture]
    public class NzbIndexFixture : CoreTest<NzbIndex>
    {
        [SetUp]
        public void Setup()
        {
            Subject.Definition = new IndexerDefinition
            {
                Name = "NzbIndex",
                Settings = new NzbIndexSettings
                {
                    Url = "http://indexer.local/"
                }
            };
        }

        [Test]
        public void should_parse_recent_feed_from_nzbindex_nl()
        {
            var recentFeed = ReadAllText(@"Files/RSS/nzbindex.xml");

            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Execute(It.Is<HttpRequest>(v => v.Method == HttpMethod.GET)))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), recentFeed));

            var releases = Subject.FetchRecent();

            releases.Should().HaveCount(50);

            var releaseInfo = releases.First();

            releaseInfo.Age.Should().
                Be(Convert.ToInt32(Math.Truncate(DateTime.UtcNow.Subtract(releaseInfo.PublishDate).TotalDays)));
            releaseInfo.AgeHours.Should().
                BeApproximately(DateTime.UtcNow.Subtract(releaseInfo.PublishDate).TotalHours, 2);
            releaseInfo.Title.Should().Be("93923-FULL-a.b.teeveeEFNet-Britains.Got.More.Talent.S06E04.PDTV.x264-C4TV-0136-britains.got.more.talent.s06e04.pdtv.x264-c4tv.nfo.nzb");
            releaseInfo.DownloadProtocol.Should().Be(DownloadProtocol.Usenet);
            releaseInfo.DownloadUrl.Should().Be("http://www.nzbindex.nl/download/69196969/93923-FULL-a.b.teeveeEFNet-Britains.Got.More.Talent.S06E04.PDTV.x264-C4TV-0136-britains.got.more.talent.s06e04.pdtv.x264-c4tv.nfo.nzb");
            releaseInfo.InfoUrl.Should().Be("http://www.nzbindex.nl/release/69196969/93923-FULL-a.b.teeveeEFNet-Britains.Got.More.Talent.S06E04.PDTV.x264-C4TV-0136-britains.got.more.talent.s06e04.pdtv.x264-c4tv.nfo.nzb");
            releaseInfo.CommentUrl.Should().Be("http://www.nzbindex.nl/nfo/69196969/93923-FULL-a.b.teeveeEFNet-Britains.Got.More.Talent.S06E04.PDTV.x264-C4TV-0136-britains.got.more.talent.s06e04.pdtv.x264-c4tv.nfo.nzb/?q=");
            releaseInfo.Indexer.Should().Be(Subject.Definition.Name);
            releaseInfo.PublishDate.Should().Be(DateTime.Parse("2012/04/14 21:32:13"));
            releaseInfo.Size.Should().Be(587327040);
        }

        [Test]
        public void should_parse_removing_passwordprotected()
        {
            var recentFeed = ReadAllText(@"Files/RSS/nzbindex_com.xml");

            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Execute(It.Is<HttpRequest>(v => v.Method == HttpMethod.GET)))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), recentFeed));

            var releases = Subject.FetchRecent();

            releases.Should().HaveCount(1);
            
            var releaseInfo = releases.First();
            
            releaseInfo.Age.Should().
                Be(Convert.ToInt32(Math.Truncate(DateTime.UtcNow.Subtract(DateTime.Parse("2014/10/28 00:34:47")).TotalDays)));
            releaseInfo.AgeHours.Should().
                BeApproximately(DateTime.UtcNow.Subtract(DateTime.Parse("2014/10/28 00:34:47")).TotalHours, 2);
            releaseInfo.Title.Should().Be("TOWN-www.town.ag-partner-of-www.ssl-news.info-OPEN-0118-2.Broke.Girls.S04E01.720p.HDTV.X264-DIMENSION.par2-66494-MB.nzb");
            releaseInfo.DownloadProtocol.Should().Be(DownloadProtocol.Usenet);
            releaseInfo.DownloadUrl.Should().Be("http://nzbindex.com/download/113966187/TOWN-www.town.ag-partner-of-www.ssl-news.info-OPEN-0118-2.Broke.Girls.S04E01.720p.HDTV.X264-DIMENSION.par2-66494-MB.nzb");
            releaseInfo.InfoUrl.Should().Be("http://nzbindex.com/release/113966187/TOWN-www.town.ag-partner-of-www.ssl-news.info-OPEN-0118-2.Broke.Girls.S04E01.720p.HDTV.X264-DIMENSION.par2-66494-MB.nzb");
            releaseInfo.CommentUrl.Should().Be("http://nzbindex.com/nfo/113966187/TOWN-www.town.ag-partner-of-www.ssl-news.info-OPEN-0118-2.Broke.Girls.S04E01.720p.HDTV.X264-DIMENSION.par2-66494-MB.nzb/?q=");
            releaseInfo.Indexer.Should().Be(Subject.Definition.Name);
            releaseInfo.PublishDate.Should().Be(DateTime.Parse("2014/10/28 00:34:47"));
            releaseInfo.Size.Should().Be(723679941);
        }

    }
}
