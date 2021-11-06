using System.Linq;
using System.Net;
using FluentAssertions;
using NUnit.Framework;

namespace NzbDrone.Integration.Test
{
    [TestFixture]
    public class IndexHtmlFixture : IntegrationTest
    {
        [Test]
        public void should_get_index_html()
        {
            var text = new WebClient().DownloadString(RootUrl);
            text.Should().NotBeNullOrWhiteSpace();
        }

        [Test]
        public void index_should_not_be_cached()
        {
            var client = new WebClient();
            _ = client.DownloadString(RootUrl);

            var headers = client.ResponseHeaders;

            headers.Get("Cache-Control").Split(',').Select(x => x.Trim())
                .Should().BeEquivalentTo("no-store, no-cache".Split(',').Select(x => x.Trim()));
            headers.Get("Pragma").Should().Be("no-cache");
            headers.Get("Expires").Should().Be("-1");
        }
    }
}
