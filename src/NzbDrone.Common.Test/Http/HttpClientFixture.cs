using System;
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
    public class HttpClientFixture : TestBase<HttpClient>
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
        [TestCase(429)]
        public void should_throw_on_unsuccessful_status_codes(int statusCode)
        {
            var request = new HttpRequest("http://eu.httpbin.org/status/" + statusCode);

            var exception = Assert.Throws<HttpException>(() => Subject.Get<HttpBinResource>(request));

            ((int)exception.Response.StatusCode).Should().Be(statusCode);

            ExceptionVerification.IgnoreWarns();
        }


        [TestCase(HttpStatusCode.Moved)]
        [TestCase(HttpStatusCode.MovedPermanently)]
        public void should_not_follow_redirects_when_not_in_production(HttpStatusCode statusCode)
        {
            var request = new HttpRequest("http://eu.httpbin.org/status/" + (int)statusCode);

            Assert.Throws<Exception>(() => Subject.Get<HttpBinResource>(request));
        }

        [Test]
        public void should_send_user_agent()
        {
            var request = new HttpRequest("http://eu.httpbin.org/get");

            var response = Subject.Get<HttpBinResource>(request);

            response.Resource.Headers.Should().ContainKey("User-Agent");

            var userAgent = response.Resource.Headers["User-Agent"].ToString();

            userAgent.Should().Contain("Sonarr");
        }

        [TestCase("Accept", "text/xml, text/rss+xml, application/rss+xml")]
        public void should_send_headers(String header, String value)
        {
            var request = new HttpRequest("http://eu.httpbin.org/get");
            request.Headers.Add(header, value);

            var response = Subject.Get<HttpBinResource>(request);

            response.Resource.Headers[header].ToString().Should().Be(value);
        }
    }

    public class HttpBinResource
    {
        public Dictionary<string, object> Headers { get; set; }
        public string Origin { get; set; }
        public string Url { get; set; }
    }
}