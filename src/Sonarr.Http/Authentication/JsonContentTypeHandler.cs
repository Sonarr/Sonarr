using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Sonarr.Http.Authentication
{
    // Some identity providers (e.g. Cloudflare Access) send an unparsable Content-Type such as
    // "application/json; charset=utf-8, application/json" from the userinfo endpoint.
    // OpenIdConnectHandler then sees no media type and fails with "Unknown response type".
    public class JsonContentTypeHandler : DelegatingHandler
    {
        public JsonContentTypeHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var headers = response.Content?.Headers;

            if (headers is { ContentType: null } &&
                headers.NonValidated.TryGetValues("Content-Type", out var values) &&
                values.ToString().TrimStart().StartsWith("application/json", StringComparison.OrdinalIgnoreCase))
            {
                headers.Remove("Content-Type");
                headers.ContentType = new MediaTypeHeaderValue("application/json");
            }

            return response;
        }
    }
}
