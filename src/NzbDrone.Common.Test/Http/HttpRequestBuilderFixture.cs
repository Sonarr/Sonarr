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

        [TestCase("192.168.0.1", "http://192.168.0.1:8080/")]
        [TestCase("localhost", "http://localhost:8080/")]
        [TestCase("::1", "http://[::1]:8080/")]
        [TestCase("[::1]", "http://[::1]:8080/")]
        [TestCase("fd3a:9a29:b136:eeee:aaaa::6", "http://[fd3a:9a29:b136:eeee:aaaa::6]:8080/")]
        public void should_build_base_url_for_host(string host, string expected)
        {
            HttpRequestBuilder.BuildBaseUrl(false, host, 8080, "/").Should().Be(expected);
        }

        [TestCase("::1", "http://[::1]:8080/api")]
        [TestCase("[::1]", "http://[::1]:8080/api")]
        public void should_build_request_for_ipv6_host(string host, string expected)
        {
            var builder = new HttpRequestBuilder(false, host, 8080);

            builder.Resource("/api").Build().Url.FullUri.Should().Be(expected);
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
