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
        private RestRequest BuildGet(string route = "series")
        {
            var request = new RestRequest(route, Method.GET);
            request.AddHeader(AccessControlHeaders.RequestMethod, "POST");

            return request;
        }

        private RestRequest BuildOptions(string route = "series")
        {
            var request = new RestRequest(route, Method.OPTIONS);

            return request;
        }

        [Test]
        public void should_not_have_allow_headers_in_response_when_not_included_in_the_request()
        {
            var request = BuildOptions();
            var response = RestClient.Execute(request);

            response.Headers.Should().NotContain(h => h.Name == AccessControlHeaders.AllowHeaders);
        }

        [Test]
        public void should_have_allow_headers_in_response_when_included_in_the_request()
        {
            var request = BuildOptions();
            request.AddHeader(AccessControlHeaders.RequestHeaders, "X-Test");

            var response = RestClient.Execute(request);

            response.Headers.Should().Contain(h => h.Name == AccessControlHeaders.AllowHeaders);
        }

        [Test]
        public void should_have_allow_origin_in_response()
        {
            var request = BuildOptions();
            var response = RestClient.Execute(request);

            response.Headers.Should().Contain(h => h.Name == AccessControlHeaders.AllowOrigin);
        }

        [Test]
        public void should_have_allow_methods_in_response()
        {
            var request = BuildOptions();
            var response = RestClient.Execute(request);

            response.Headers.Should().Contain(h => h.Name == AccessControlHeaders.AllowMethods);
        }

        [Test]
        public void should_not_have_allow_methods_in_non_options_request()
        {
            var request = BuildGet();
            var response = RestClient.Execute(request);

            response.Headers.Should().NotContain(h => h.Name == AccessControlHeaders.AllowMethods);
        }

        [Test]
        public void should_have_allow_origin_in_non_options_request()
        {
            var request = BuildGet();
            var response = RestClient.Execute(request);

            response.Headers.Should().Contain(h => h.Name == AccessControlHeaders.AllowOrigin);
        }

        [Test]
        public void should_not_have_allow_origin_in_non_api_request()
        {
            var request = BuildGet("../abc");
            var response = RestClient.Execute(request);

            response.Headers.Should().NotContain(h => h.Name == AccessControlHeaders.AllowOrigin);
        }
    }
}
