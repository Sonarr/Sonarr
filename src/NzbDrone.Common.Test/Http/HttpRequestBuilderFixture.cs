using System;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test.Http
{
    [TestFixture]
    public class HttpRequestBuilderFixture : TestBase
    {
        [TestCase("http://host/{seg}/some", "http://host/dir/some")]
        [TestCase("http://host/some/{seg}", "http://host/some/dir")]
        public void should_add_single_segment_url_segments(string url, string result)
        {
            var requestBuilder = new HttpRequestBuilder(url);

            requestBuilder.SetSegment("seg", "dir");

            requestBuilder.Build().Url.Should().Be(result);
        }

        [Test]
        public void shouldnt_add_value_for_nonexisting_segment()
        {
            var requestBuilder = new HttpRequestBuilder("http://host/{seg}/some");
            Assert.Throws<InvalidOperationException>(() => requestBuilder.SetSegment("seg2", "dir"));
        }

        [Test]
        public void should_remove_duplicated_slashes()
        {
            var builder = new HttpRequestBuilder("http://domain/");

            var request = builder.Resource("/v1/").Build();

            request.Url.FullUri.Should().Be("http://domain/v1/");
        }
    }
}
