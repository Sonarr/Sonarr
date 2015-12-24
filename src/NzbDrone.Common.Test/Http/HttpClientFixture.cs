using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using FluentAssertions;
using Moq;
using NLog;
using NUnit.Framework;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Http;
using NzbDrone.Common.Http.Dispatchers;
using NzbDrone.Common.TPL;
using NzbDrone.Test.Common;
using NzbDrone.Test.Common.Categories;

namespace NzbDrone.Common.Test.Http
{
    [IntegrationTest]
    [TestFixture(typeof(ManagedHttpDispatcher))]
    [TestFixture(typeof(CurlHttpDispatcher))]
    public class HttpClientFixture<TDispatcher> : TestBase<HttpClient> where TDispatcher : IHttpDispatcher
    {        
        [SetUp]
        public void SetUp()
        {
            Mocker.SetConstant<ICacheManager>(Mocker.Resolve<CacheManager>());
            Mocker.SetConstant<IRateLimitService>(Mocker.Resolve<RateLimitService>());
            Mocker.SetConstant<IEnumerable<IHttpRequestInterceptor>>(new IHttpRequestInterceptor[0]);
            Mocker.SetConstant<IHttpDispatcher>(Mocker.Resolve<TDispatcher>());
        }

        [Test]
        public void should_execute_simple_get()
        {
            var request = new HttpRequest("http://eu.httpbin.org/get");

            var response = Subject.Execute(request);

            response.Content.Should().NotBeNullOrWhiteSpace();
        }
        
        [Test]
        public void should_execute_https_get()
        {
            var request = new HttpRequest("https://eu.httpbin.org/get");

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

            var oldClient = new HttpClient(new IHttpRequestInterceptor[0], Mocker.Resolve<ICacheManager>(), Mocker.Resolve<IRateLimitService>(), Mocker.Resolve<IHttpDispatcher>(), Mocker.Resolve<Logger>());

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

        [TestCase("en-US")]
        [TestCase("es-ES")]
        public void should_parse_malformed_cloudflare_cookie(string culture)
        {
            var origCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(culture);
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(culture);
            try
            {
                // the date is bad in the below - should be 13-Jul-2016
                string malformedCookie = @"__cfduid=d29e686a9d65800021c66faca0a29b4261436890790; expires=Wed, 13-Jul-16 16:19:50 GMT; path=/; HttpOnly";
                string url = "http://eu.httpbin.org/response-headers?Set-Cookie=" +
                    System.Uri.EscapeUriString(malformedCookie);

                var requestSet = new HttpRequest(url);
                requestSet.AllowAutoRedirect = false;
                requestSet.StoreResponseCookie = true;

                var responseSet = Subject.Get(requestSet);

                var request = new HttpRequest("http://eu.httpbin.org/get");

                var response = Subject.Get<HttpBinResource>(request);

                response.Resource.Headers.Should().ContainKey("Cookie");

                var cookie = response.Resource.Headers["Cookie"].ToString();

                cookie.Should().Contain("__cfduid=d29e686a9d65800021c66faca0a29b4261436890790");

                ExceptionVerification.IgnoreErrors();
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = origCulture;
                Thread.CurrentThread.CurrentUICulture = origCulture;
            }
        }

        [TestCase("lang_code=en; expires=Fri, 23-Dec-2016 18:09:14 GMT; Max-Age=31536000; path=/; domain=.abc.com")]
        public void should_reject_malformed_domain_cookie(string malformedCookie)
        {
            try
            {
                // the date is bad in the below - should be 13-Jul-2016
                string url = "http://eu.httpbin.org/response-headers?Set-Cookie=" + Uri.EscapeUriString(malformedCookie);

                var requestSet = new HttpRequest(url);
                requestSet.AllowAutoRedirect = false;
                requestSet.StoreResponseCookie = true;

                var responseSet = Subject.Get(requestSet);

                var request = new HttpRequest("http://eu.httpbin.org/get");

                var response = Subject.Get<HttpBinResource>(request);

                response.Resource.Headers.Should().NotContainKey("Cookie");

                ExceptionVerification.IgnoreErrors();
            }
            finally
            {
            }
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