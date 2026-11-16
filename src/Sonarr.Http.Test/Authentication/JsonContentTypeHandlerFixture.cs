using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Sonarr.Http.Authentication;

namespace Sonarr.Http.Test.Authentication
{
    [TestFixture]
    public class JsonContentTypeHandlerFixture
    {
        private static async Task<HttpResponseMessage> Send(string contentType)
        {
            var content = new StringContent("{\"sub\":\"1\"}");
            content.Headers.Remove("Content-Type");

            if (contentType != null)
            {
                content.Headers.TryAddWithoutValidation("Content-Type", contentType);
            }

            using var client = new HttpClient(new JsonContentTypeHandler(new StubHandler(content)));

            return await client.GetAsync("http://localhost/userinfo");
        }

        [Test]
        public async Task should_fix_duplicated_json_content_type()
        {
            var response = await Send("application/json; charset=utf-8, application/json");

            response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
        }

        [TestCase("application/json; charset=utf-8", "application/json")]
        [TestCase("application/jwt", "application/jwt")]
        public async Task should_not_change_valid_content_type(string contentType, string expected)
        {
            var response = await Send(contentType);

            response.Content.Headers.ContentType.MediaType.Should().Be(expected);
        }

        [TestCase(null)]
        [TestCase("text/html, application/json")]
        public async Task should_not_add_json_content_type_for_other_responses(string contentType)
        {
            var response = await Send(contentType);

            response.Content.Headers.ContentType.Should().BeNull();
        }

        private class StubHandler : HttpMessageHandler
        {
            private readonly HttpContent _content;

            public StubHandler(HttpContent content)
            {
                _content = content;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = _content });
            }
        }
    }
}
