using System;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Http;

namespace NzbDrone.Common.Test.Http
{
    [TestFixture]
    public class HttpRequestFixture
    {
        [TestCase("http://host/{seg}/some", "http://host/dir/some")]
        [TestCase("http://host/some/{seg}", "http://host/some/dir")]
        public void should_add_single_segment_url_segments(string url, string result)
        {
            var request = new HttpRequest(url);

            request.AddSegment("seg", "dir");

            request.Url.Should().Be(result);
        }

        [Test]
        public void shouldnt_add_value_for_nonexisting_segment()
        {
            var request = new HttpRequest("http://host/{seg}/some");
            Assert.Throws<InvalidOperationException>(() => request.AddSegment("seg2", "dir"));
        }
    }
}