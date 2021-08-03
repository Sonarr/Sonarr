using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.IndexerTests
{
    public class BasicRssParserFixture : CoreTest<RssParser>
    {
        [TestCase("5.64 GB", 6055903887)]
        [TestCase("5.54 GiB", 5948529705)]
        [TestCase("398.62 MiB", 417983365)]
        [TestCase("7,162.1MB", 7510006170)]
        [TestCase("162.1MB", 169974170)]
        [TestCase("398.62 MB", 417983365)]
        [TestCase("845 MB", 886046720)]
        [TestCase("7,162,100.0KB", 7333990400)]
        [TestCase("12341234", 12341234)]
        public void should_parse_size(string sizeString, long expectedSize)
        {
            var result = RssParser.ParseSize(sizeString, true);

            result.Should().Be(expectedSize);
        }

        [TestCase("100 Kbps")]
        [TestCase("100 Kb/s")]
        [TestCase(" 12341234")]
        [TestCase("12341234 other")]
        [TestCase("")]
        public void should_not_parse_size(string sizeString)
        {
            var result = RssParser.ParseSize(sizeString, true);

            result.Should().Be(0);
        }

        private IndexerResponse CreateResponse(string url, string content)
        {
            var httpRequest = new HttpRequest(url);
            var httpResponse = new HttpResponse(httpRequest, new HttpHeader(), Encoding.UTF8.GetBytes(content));

            return new IndexerResponse(new IndexerRequest(httpRequest), httpResponse);
        }

        [Test]
        public void should_handle_relative_url()
        {
            var xml = ReadAllText("Files/Indexers/relative_urls.xml");

            var result = Subject.ParseResponse(CreateResponse("http://my.indexer.com/api?q=My+Favourite+Show", xml));

            result.Should().HaveCount(1);

            result.First().CommentUrl.Should().Be("http://my.indexer.com/details/123#comments");
            result.First().DownloadUrl.Should().Be("http://my.indexer.com/getnzb/123.nzb&i=782&r=123");
        }
    }
}
