using System;
using System.Net.Http;
using System.Net.Http.Headers;
using FluentAssertions;
using NUnit.Framework;

namespace NzbDrone.Integration.Test
{
    [TestFixture]
    public class IndexHtmlFixture : IntegrationTest
    {
        private HttpClient _httpClient = new HttpClient();

        [Test]
        public void should_get_index_html()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, RootUrl);
            var response = _httpClient.Send(request);
            var text = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            text.Should().NotBeNullOrWhiteSpace();
        }

        [Test]
        public void index_should_not_be_cached()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, RootUrl);
            var response = _httpClient.Send(request);

            var headers = response.Headers;

            headers.CacheControl.NoStore.Should().BeTrue();
            headers.CacheControl.NoCache.Should().BeTrue();
            headers.Pragma.Should().Contain(new NameValueHeaderValue("no-cache"));

            response.Content.Headers.Expires.Should().BeBefore(DateTime.UtcNow);
        }
    }
}
