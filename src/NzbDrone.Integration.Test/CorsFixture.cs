using FluentAssertions;
using NUnit.Framework;
using Sonarr.Http.Extensions;
using RestSharp;
using Sonarr.Http.Extensions;

namespace NzbDrone.Integration.Test
{
    [TestFixture]
    public class CorsFixture : IntegrationTest
    {
        private RestRequest BuildRequest()
        {
            var request = new RestRequest("series");
            request.AddHeader(AccessControlHeaders.RequestMethod, "POST");

            return request;
        }

        [Test]
        public void should_not_have_allow_headers_in_response_when_not_included_in_the_request()
        {
            var request = BuildRequest();
            var response = RestClient.Get(request);
            
            response.Headers.Should().NotContain(h => h.Name == AccessControlHeaders.AllowHeaders);
        }

        [Test]
        public void should_have_allow_headers_in_response_when_included_in_the_request()
        {
            var request = BuildRequest();
            request.AddHeader(AccessControlHeaders.RequestHeaders, "X-Test");

            var response = RestClient.Get(request);

            response.Headers.Should().Contain(h => h.Name == AccessControlHeaders.AllowHeaders);
        }

        [Test]
        public void should_have_allow_origin_in_response()
        {
            var request = BuildRequest();
            var response = RestClient.Get(request);

            response.Headers.Should().Contain(h => h.Name == AccessControlHeaders.AllowOrigin);
        }

        [Test]
        public void should_have_allow_methods_in_response()
        {
            var request = BuildRequest();
            var response = RestClient.Get(request);

            response.Headers.Should().Contain(h => h.Name == AccessControlHeaders.AllowMethods);
        }
    }
}
