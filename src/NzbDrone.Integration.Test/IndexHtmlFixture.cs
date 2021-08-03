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
    }
}
