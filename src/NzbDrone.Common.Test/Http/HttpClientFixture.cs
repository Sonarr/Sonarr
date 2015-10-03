using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Http;
using NzbDrone.Test.Common;
using NzbDrone.Test.Common.Categories;
using NLog;
using NzbDrone.Common.TPL;
using Moq;

namespace NzbDrone.Common.Test.Http
{
    [TestFixture]
    [IntegrationTest]
    public class HttpClientFixture : TestBase<HttpClient>
    {
        [SetUp]
        public void SetUp()
        {
            Mocker.SetConstant<ICacheManager>(Mocker.Resolve<CacheManager>());
            Mocker.SetConstant<IRateLimitService>(Mocker.Resolve<RateLimitService>());
            Mocker.SetConstant<IEnumerable<IHttpRequestInterceptor>>(new IHttpRequestInterceptor[0]);
        }

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

        [Test]
        public void should_execute_simple_post()
        {
            var request = new HttpRequest("http://eu.httpbin.org/post");
            request.Body = "{ my: 1 }";

            var response = Subject.Post<HttpBinResource>(request);

            response.Resource.Data.Should().Be(request.Body);
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
        public void should_throw_on_unsuccessful_status_codes(int statusCode)
        {
            var request = new HttpRequest("http://eu.httpbin.org/status/" + statusCode);

            var exception = Assert.Throws<HttpException>(() => Subject.Get<HttpBinResource>(request));

            ((int)exception.Response.StatusCode).Should().Be(statusCode);

            ExceptionVerification.IgnoreWarns();
        }

        [Test]
        public void should_not_follow_redirects_when_not_in_production()
        {
            var request = new HttpRequest("http://eu.httpbin.org/redirect/1");

            Subject.Get(request);

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_follow_redirects()
        {
            var request = new HttpRequest("http://eu.httpbin.org/redirect/1");
            request.AllowAutoRedirect = true;

            Subject.Get(request);

            ExceptionVerification.ExpectedErrors(0);
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
        public void should_send_headers(string header, string value)
        {
            var request = new HttpRequest("http://eu.httpbin.org/get");
            request.Headers.Add(header, value);

            var response = Subject.Get<HttpBinResource>(request);

            response.Resource.Headers[header].ToString().Should().Be(value);
        }

        [Test]
        public void should_not_download_file_with_error()
        {
            var file = GetTempFilePath();

            Assert.Throws<WebException>(() => Subject.DownloadFile("http://download.sonarr.tv/wrongpath", file));

            File.Exists(file).Should().BeFalse();

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_send_cookie()
        {
            var request = new HttpRequest("http://eu.httpbin.org/get");
            request.AddCookie("my", "cookie");

            var response = Subject.Get<HttpBinResource>(request);

            response.Resource.Headers.Should().ContainKey("Cookie");

            var cookie = response.Resource.Headers["Cookie"].ToString();

            cookie.Should().Contain("my=cookie");
        }

        public void GivenOldCookie()
        {
            var oldRequest = new HttpRequest("http://eu.httpbin.org/get");
            oldRequest.AddCookie("my", "cookie");

            var oldClient = new HttpClient(new IHttpRequestInterceptor[0], Mocker.Resolve<ICacheManager>(), Mocker.Resolve<IRateLimitService>(), Mocker.Resolve<Logger>());

            oldClient.Should().NotBeSameAs(Subject);

            var oldResponse = oldClient.Get<HttpBinResource>(oldRequest);

            oldResponse.Resource.Headers.Should().ContainKey("Cookie");
        }

        [Test]
        public void should_preserve_cookie_during_session()
        {
            GivenOldCookie();

            var request = new HttpRequest("http://eu.httpbin.org/get");

            var response = Subject.Get<HttpBinResource>(request);

            response.Resource.Headers.Should().ContainKey("Cookie");

            var cookie = response.Resource.Headers["Cookie"].ToString();

            cookie.Should().Contain("my=cookie");
        }

        [Test]
        public void should_not_send_cookie_to_other_host()
        {
            GivenOldCookie();

            var request = new HttpRequest("http://httpbin.org/get");

            var response = Subject.Get<HttpBinResource>(request);

            response.Resource.Headers.Should().NotContainKey("Cookie");
        }

        [Test]
        public void should_not_store_response_cookie()
        {
            var requestSet = new HttpRequest("http://eu.httpbin.org/cookies/set?my=cookie");
            requestSet.AllowAutoRedirect = false;

            var responseSet = Subject.Get(requestSet);

            var request = new HttpRequest("http://eu.httpbin.org/get");

            var response = Subject.Get<HttpBinResource>(request);

            response.Resource.Headers.Should().NotContainKey("Cookie");

            ExceptionVerification.IgnoreErrors();
        }

        [Test]
        public void should_store_response_cookie()
        {
            var requestSet = new HttpRequest("http://eu.httpbin.org/cookies/set?my=cookie");
            requestSet.AllowAutoRedirect = false;
            requestSet.StoreResponseCookie = true;

            var responseSet = Subject.Get(requestSet);

            var request = new HttpRequest("http://eu.httpbin.org/get");

            var response = Subject.Get<HttpBinResource>(request);

            response.Resource.Headers.Should().ContainKey("Cookie");

            var cookie = response.Resource.Headers["Cookie"].ToString();

            cookie.Should().Contain("my=cookie");

            ExceptionVerification.IgnoreErrors();
        }

        [Test]
        public void should_overwrite_response_cookie()
        {
            var requestSet = new HttpRequest("http://eu.httpbin.org/cookies/set?my=cookie");
            requestSet.AllowAutoRedirect = false;
            requestSet.StoreResponseCookie = true;
            requestSet.AddCookie("my", "oldcookie");

            var responseSet = Subject.Get(requestSet);

            var request = new HttpRequest("http://eu.httpbin.org/get");

            var response = Subject.Get<HttpBinResource>(request);

            response.Resource.Headers.Should().ContainKey("Cookie");

            var cookie = response.Resource.Headers["Cookie"].ToString();

            cookie.Should().Contain("my=cookie");

            ExceptionVerification.IgnoreErrors();
        }

        [Test]
        public void should_throw_on_http429_too_many_requests()
        {
            var request = new HttpRequest("http://eu.httpbin.org/status/429");

            Assert.Throws<TooManyRequestsException>(() => Subject.Get(request));

            ExceptionVerification.IgnoreWarns();
        }

        [Test]
        public void should_call_interceptor()
        {
            Mocker.SetConstant<IEnumerable<IHttpRequestInterceptor>>(new [] { Mocker.GetMock<IHttpRequestInterceptor>().Object });

            Mocker.GetMock<IHttpRequestInterceptor>()
                .Setup(v => v.PreRequest(It.IsAny<HttpRequest>()))
                .Returns<HttpRequest>(r => r);

            Mocker.GetMock<IHttpRequestInterceptor>()
                .Setup(v => v.PostResponse(It.IsAny<HttpResponse>()))
                .Returns<HttpResponse>(r => r);

            var request = new HttpRequest("http://eu.httpbin.org/get");

            Subject.Get(request);

            Mocker.GetMock<IHttpRequestInterceptor>()
                .Verify(v => v.PreRequest(It.IsAny<HttpRequest>()), Times.Once());

            Mocker.GetMock<IHttpRequestInterceptor>()
                .Verify(v => v.PostResponse(It.IsAny<HttpResponse>()), Times.Once());
        }
    }

    public class HttpBinResource
    {
        public Dictionary<string, object> Headers { get; set; }
        public string Origin { get; set; }
        public string Url { get; set; }
        public string Data { get; set; }
    }
}