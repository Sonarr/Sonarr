using System.Collections.Generic;
using System.Net;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Test.Common;
using NzbDrone.Test.Common.Categories;

namespace NzbDrone.Common.Test.Http
{
    [TestFixture]
    [IntegrationTest]
    public class RestClientFixture : TestBase<HttpClient>
    {
        [Test]
        public void should_execute_simple_get()
        {
            var request = new HttpRequest("http://eu.httpbin.org/get");

            var response = Subject.Execute(request);

            response.Content.Should().NotBeNullOrWhiteSpace();
        }

        [Test]
        public void should_execute_typed_get()
        {
            var request = new HttpRequest("http://eu.httpbin.org/get");

            var response = Subject.Get<HttpBinResource>(request);

            response.Resource.Url.Should().Be(request.Url.ToString());
        }

        [TestCase("gzip")]
        public void should_execute_get_using_gzip(string compression)
        {
            var request = new HttpRequest("http://eu.httpbin.org/" + compression);

            var response = Subject.Get<HttpBinResource>(request);

            response.Resource.Headers["Accept-Encoding"].ToString().Should().Be(compression);
            response.Headers.ContentLength.Should().BeLessOrEqualTo(response.Content.Length);
        }

        [TestCase(HttpStatusCode.Unauthorized)]
        [TestCase(HttpStatusCode.Forbidden)]
        [TestCase(HttpStatusCode.NotFound)]
        [TestCase(HttpStatusCode.InternalServerError)]
        [TestCase(HttpStatusCode.ServiceUnavailable)]
        [TestCase(HttpStatusCode.BadGateway)]
        public void should_throw_on_none_success_error_codes(HttpStatusCode statusCode)
        {
            var request = new HttpRequest("http://eu.httpbin.org/status/" + (int)statusCode);

            var exception = Assert.Throws<HttpException>(() => Subject.Get<HttpBinResource>(request));

            exception.Response.StatusCode.Should().Be(statusCode);
        }
    }


    public class HttpBinResource
    {
        public Dictionary<string, object> Headers { get; set; }
        public string Origin { get; set; }
        public string Url { get; set; }
    }




}