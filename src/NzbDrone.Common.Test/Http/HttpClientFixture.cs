using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using FluentAssertions;
using Moq;
using NLog;
using NUnit.Framework;
using NzbDrone.Common.Cache;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Http;
using NzbDrone.Common.Http.Dispatchers;
using NzbDrone.Common.Http.Proxy;
using NzbDrone.Common.TPL;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Security;
using NzbDrone.Test.Common;
using NzbDrone.Test.Common.Categories;
using HttpClient = NzbDrone.Common.Http.HttpClient;

namespace NzbDrone.Common.Test.Http
{
    [IntegrationTest]
    [TestFixture(typeof(ManagedHttpDispatcher))]
    public class HttpClientFixture<TDispatcher> : TestBase<HttpClient>
        where TDispatcher : IHttpDispatcher
    {
        private string[] _httpBinHosts;
        private int _httpBinSleep;
        private int _httpBinRandom;
        private string _httpBinHost;
        private string _httpBinHost2;

        private System.Net.Http.HttpClient _httpClient = new ();

        [OneTimeSetUp]
        public void FixtureSetUp()
        {
            // Always use our server for main tests
            var mainHost = "httpbin.servarr.com";

            // Use mirrors for tests that use two hosts
            var candidates = new[] { "httpbin1.servarr.com" };

            // httpbin.org is broken right now, occassionally redirecting to https if it's unavailable.
            _httpBinHost = mainHost;
            _httpBinHosts = candidates.Where(IsTestSiteAvailable).ToArray();

            TestLogger.Info($"{candidates.Length} TestSites available.");

            _httpBinSleep = 10;
        }

        private bool IsTestSiteAvailable(string site)
        {
            try
            {
                var res = _httpClient.GetAsync($"https://{site}/get").GetAwaiter().GetResult();

                if (res.StatusCode != HttpStatusCode.OK)
                {
                    return false;
                }

                res = _httpClient.GetAsync($"https://{site}/status/429").GetAwaiter().GetResult();

                if (res == null || res.StatusCode != (HttpStatusCode)429)
                {
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        [SetUp]
        public void SetUp()
        {
            if (!_httpBinHosts.Any())
            {
                Assert.Inconclusive("No TestSites available");
            }

            Mocker.GetMock<IPlatformInfo>().Setup(c => c.Version).Returns(new Version("1.0.0"));
            Mocker.GetMock<IOsInfo>().Setup(c => c.Name).Returns("TestOS");
            Mocker.GetMock<IOsInfo>().Setup(c => c.Version).Returns("9.0.0");

            Mocker.GetMock<IConfigService>().SetupGet(x => x.CertificateValidation).Returns(CertificateValidationType.Enabled);

            Mocker.SetConstant<IUserAgentBuilder>(Mocker.Resolve<UserAgentBuilder>());

            Mocker.SetConstant<ICacheManager>(Mocker.Resolve<CacheManager>());
            Mocker.SetConstant<ICreateManagedWebProxy>(Mocker.Resolve<ManagedWebProxyFactory>());
            Mocker.SetConstant<ICertificateValidationService>(new X509CertificateValidationService(Mocker.GetMock<IConfigService>().Object, TestLogger));

            Mocker.SetConstant<IRateLimitService>(Mocker.Resolve<RateLimitService>());
            Mocker.SetConstant<IEnumerable<IHttpRequestInterceptor>>(new IHttpRequestInterceptor[0]);
            Mocker.SetConstant<IHttpDispatcher>(Mocker.Resolve<TDispatcher>());

            // Used for manual testing of socks proxies.
            //Mocker.GetMock<IHttpProxySettingsProvider>()
            //      .Setup(v => v.GetProxySettings(It.IsAny<HttpRequest>()))
            //      .Returns(new HttpProxySettings(ProxyType.Socks5, "127.0.0.1", 5476, "", false));

            // Roundrobin over the two servers, to reduce the chance of hitting the ratelimiter.
            _httpBinHost2 = _httpBinHosts[_httpBinRandom++ % _httpBinHosts.Length];
        }

        [TearDown]
        public void TearDown()
        {
            Thread.Sleep(_httpBinSleep);
        }

        [Test]
        public void should_execute_simple_get()
        {
            var request = new HttpRequest($"https://{_httpBinHost}/get");

            var response = Subject.Execute(request);

            response.Content.Should().NotBeNullOrWhiteSpace();
        }

        [Test]
        public void should_execute_https_get()
        {
            var request = new HttpRequest($"https://{_httpBinHost}/get");

            var response = Subject.Execute(request);

            response.Content.Should().NotBeNullOrWhiteSpace();
        }

        [TestCase(CertificateValidationType.Enabled)]
        [TestCase(CertificateValidationType.DisabledForLocalAddresses)]
        public void bad_ssl_should_fail_when_remote_validation_enabled(CertificateValidationType validationType)
        {
            Mocker.GetMock<IConfigService>().SetupGet(x => x.CertificateValidation).Returns(validationType);
            var request = new HttpRequest($"https://expired.badssl.com");

            Assert.Throws<HttpRequestException>(() => Subject.Execute(request));
            ExceptionVerification.ExpectedErrors(2);
        }

        [Test]
        public void bad_ssl_should_pass_if_remote_validation_disabled()
        {
            Mocker.GetMock<IConfigService>().SetupGet(x => x.CertificateValidation).Returns(CertificateValidationType.Disabled);

            var request = new HttpRequest($"https://expired.badssl.com");

            Subject.Execute(request);
            ExceptionVerification.ExpectedErrors(0);
        }

        [Test]
        public void should_execute_typed_get()
        {
            var request = new HttpRequest($"https://{_httpBinHost}/get?test=1");

            var response = Subject.Get<HttpBinResource>(request);

            response.Resource.Url.EndsWith("/get?test=1");
            response.Resource.Args.Should().Contain("test", "1");
        }

        [Test]
        public void should_execute_simple_post()
        {
            var message = "{ my: 1 }";

            var request = new HttpRequest($"https://{_httpBinHost}/post");
            request.SetContent(message);

            var response = Subject.Post<HttpBinResource>(request);

            response.Resource.Data.Should().Be(message);
        }

        [Test]
        public void should_execute_post_with_content_type()
        {
            var message = "{ my: 1 }";

            var request = new HttpRequest($"https://{_httpBinHost}/post");
            request.SetContent(message);
            request.Headers.ContentType = "application/json";

            var response = Subject.Post<HttpBinResource>(request);

            response.Resource.Data.Should().Be(message);
        }

        [Test]
        public void should_execute_get_using_gzip()
        {
            var request = new HttpRequest($"https://{_httpBinHost}/gzip");
            var response = Subject.Get<HttpBinResource>(request);

            response.Resource.Headers["Accept-Encoding"].ToString().Should().Contain("gzip");

            response.Resource.Gzipped.Should().BeTrue();
            response.Resource.Brotli.Should().BeFalse();
        }

        [Test]
        public void should_execute_get_using_brotli()
        {
            var request = new HttpRequest($"https://{_httpBinHost}/brotli");
            var response = Subject.Get<HttpBinResource>(request);

            response.Resource.Headers["Accept-Encoding"].ToString().Should().Contain("br");

            response.Resource.Gzipped.Should().BeFalse();
            response.Resource.Brotli.Should().BeTrue();
        }

        [TestCase(HttpStatusCode.Unauthorized)]
        [TestCase(HttpStatusCode.Forbidden)]
        [TestCase(HttpStatusCode.NotFound)]
        [TestCase(HttpStatusCode.InternalServerError)]
        [TestCase(HttpStatusCode.ServiceUnavailable)]
        [TestCase(HttpStatusCode.BadGateway)]
        public void should_throw_on_unsuccessful_status_codes(int statusCode)
        {
            var request = new HttpRequest($"https://{_httpBinHost}/status/{statusCode}");

            var exception = Assert.Throws<HttpException>(() => Subject.Get<HttpBinResource>(request));

            ((int)exception.Response.StatusCode).Should().Be(statusCode);

            ExceptionVerification.IgnoreWarns();
        }

        [Test]
        public void should_not_throw_on_suppressed_status_codes()
        {
            var request = new HttpRequest($"https://{_httpBinHost}/status/{HttpStatusCode.NotFound}");
            request.SuppressHttpErrorStatusCodes = new[] { HttpStatusCode.NotFound };

            Assert.Throws<HttpException>(() => Subject.Get<HttpBinResource>(request));

            ExceptionVerification.IgnoreWarns();
        }

        [Test]
        public void should_log_unsuccessful_status_codes()
        {
            var request = new HttpRequest($"https://{_httpBinHost}/status/{HttpStatusCode.NotFound}");

            var exception = Assert.Throws<HttpException>(() => Subject.Get<HttpBinResource>(request));

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_not_log_unsuccessful_status_codes()
        {
            var request = new HttpRequest($"https://{_httpBinHost}/status/{HttpStatusCode.NotFound}");
            request.LogHttpError = false;

            Assert.Throws<HttpException>(() => Subject.Get<HttpBinResource>(request));

            ExceptionVerification.ExpectedWarns(0);
        }

        [Test]
        public void should_not_follow_redirects_when_not_in_production()
        {
            var request = new HttpRequest($"https://{_httpBinHost}/redirect/1");

            Subject.Get(request);

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_follow_redirects()
        {
            var request = new HttpRequest($"https://{_httpBinHost}/redirect/1");
            request.AllowAutoRedirect = true;

            var response = Subject.Get(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            ExceptionVerification.ExpectedErrors(0);
        }

        [Test]
        public void should_not_follow_redirects()
        {
            var request = new HttpRequest($"https://{_httpBinHost}/redirect/1");
            request.AllowAutoRedirect = false;

            var response = Subject.Get(request);

            response.StatusCode.Should().Be(HttpStatusCode.Found);

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_follow_redirects_to_https()
        {
            var request = new HttpRequestBuilder($"https://{_httpBinHost}/redirect-to")
                .AddQueryParam("url", $"https://sonarr.tv/")
                .Build();
            request.AllowAutoRedirect = true;

            var response = Subject.Get(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Should().Contain("Sonarr");

            ExceptionVerification.ExpectedErrors(0);
        }

        [Test]
        public void should_throw_on_too_many_redirects()
        {
            var request = new HttpRequest($"https://{_httpBinHost}/redirect/6");
            request.AllowAutoRedirect = true;

            Assert.Throws<WebException>(() => Subject.Get(request));

            ExceptionVerification.ExpectedErrors(0);
        }

        [Test]
        public void should_send_user_agent()
        {
            var request = new HttpRequest($"https://{_httpBinHost}/get");

            var response = Subject.Get<HttpBinResource>(request);

            response.Resource.Headers.Should().ContainKey("User-Agent");

            var userAgent = response.Resource.Headers["User-Agent"].ToString();

            userAgent.Should().Contain("Sonarr");
        }

        [TestCase("Accept", "text/xml, text/rss+xml, application/rss+xml")]
        public void should_send_headers(string header, string value)
        {
            var request = new HttpRequest($"https://{_httpBinHost}/get");
            request.Headers.Add(header, value);

            var response = Subject.Get<HttpBinResource>(request);

            response.Resource.Headers[header].ToString().Should().Be(value);
        }

        [Test]
        public void should_download_file()
        {
            var file = GetTempFilePath();

            var url = "https://sonarr.tv/img/slider/seriesdetails.png";

            Subject.DownloadFile(url, file);

            File.Exists(file).Should().BeTrue();
            File.Exists(file + ".part").Should().BeFalse();

            var fileInfo = new FileInfo(file);

            fileInfo.Length.Should().Be(307054);
        }

        [Test]
        public void should_download_file_with_redirect()
        {
            var file = GetTempFilePath();

            var request = new HttpRequestBuilder($"https://{_httpBinHost}/redirect-to")
                .AddQueryParam("url", $"https://sonarr.tv/img/slider/seriesdetails.png")
                .Build();

            Subject.DownloadFile(request.Url.FullUri, file);

            ExceptionVerification.ExpectedErrors(0);

            var fileInfo = new FileInfo(file);
            fileInfo.Exists.Should().BeTrue();
            fileInfo.Length.Should().Be(307054);
        }

        [Test]
        public void should_not_download_file_with_error()
        {
            var file = GetTempFilePath();

            Assert.Throws<HttpException>(() => Subject.DownloadFile("https://download.sonarr.tv/wrongpath", file));

            File.Exists(file).Should().BeFalse();
            File.Exists(file + ".part").Should().BeFalse();

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_not_write_redirect_content_to_stream()
        {
            var file = GetTempFilePath();

            using (var fileStream = new FileStream(file, FileMode.Create))
            {
                var request = new HttpRequest($"https://{_httpBinHost}/redirect/1");
                request.AllowAutoRedirect = false;
                request.ResponseStream = fileStream;

                var response = Subject.Get(request);

                response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            }

            ExceptionVerification.ExpectedErrors(1);

            File.Exists(file).Should().BeTrue();

            var fileInfo = new FileInfo(file);

            fileInfo.Length.Should().Be(0);
        }

        [Test]
        public void should_send_cookie()
        {
            var request = new HttpRequest($"https://{_httpBinHost}/get");
            request.Cookies["my"] = "cookie";

            var response = Subject.Get<HttpBinResource>(request);

            response.Resource.Headers.Should().ContainKey("Cookie");

            var cookie = response.Resource.Headers["Cookie"].ToString();

            cookie.Should().Contain("my=cookie");
        }

        public void GivenOldCookie()
        {
            if (_httpBinHost == _httpBinHost2)
            {
                Assert.Inconclusive("Need both httpbin.org and eu.httpbin.org to run this test.");
            }

            var oldRequest = new HttpRequest($"https://{_httpBinHost2}/get");
            oldRequest.Cookies["my"] = "cookie";

            var oldClient = new HttpClient(new IHttpRequestInterceptor[0], Mocker.Resolve<ICacheManager>(), Mocker.Resolve<IRateLimitService>(), Mocker.Resolve<IHttpDispatcher>(), Mocker.GetMock<IUserAgentBuilder>().Object, Mocker.Resolve<Logger>());

            oldClient.Should().NotBeSameAs(Subject);

            var oldResponse = oldClient.Get<HttpBinResource>(oldRequest);

            oldResponse.Resource.Headers.Should().ContainKey("Cookie");
        }

        [Test]
        public void should_preserve_cookie_during_session()
        {
            GivenOldCookie();

            var request = new HttpRequest($"https://{_httpBinHost2}/get");

            var response = Subject.Get<HttpBinResource>(request);

            response.Resource.Headers.Should().ContainKey("Cookie");

            var cookie = response.Resource.Headers["Cookie"].ToString();

            cookie.Should().Contain("my=cookie");
        }

        [Test]
        public void should_not_send_cookie_to_other_host()
        {
            GivenOldCookie();

            var request = new HttpRequest($"https://{_httpBinHost}/get");

            var response = Subject.Get<HttpBinResource>(request);

            response.Resource.Headers.Should().NotContainKey("Cookie");
        }

        [Test]
        public void should_not_store_request_cookie()
        {
            var requestGet = new HttpRequest($"https://{_httpBinHost}/get");
            requestGet.Cookies.Add("my", "cookie");
            requestGet.AllowAutoRedirect = false;
            requestGet.StoreRequestCookie = false;
            requestGet.StoreResponseCookie = false;
            var responseGet = Subject.Get<HttpBinResource>(requestGet);

            var requestCookies = new HttpRequest($"https://{_httpBinHost}/cookies");
            requestCookies.AllowAutoRedirect = false;
            var responseCookies = Subject.Get<HttpCookieResource>(requestCookies);

            responseCookies.Resource.Cookies.Should().BeEmpty();

            ExceptionVerification.IgnoreErrors();
        }

        [Test]
        public void should_store_request_cookie()
        {
            var requestGet = new HttpRequest($"https://{_httpBinHost}/get");
            requestGet.Cookies.Add("my", "cookie");
            requestGet.AllowAutoRedirect = false;
            requestGet.StoreRequestCookie.Should().BeTrue();
            requestGet.StoreResponseCookie = false;
            var responseGet = Subject.Get<HttpBinResource>(requestGet);

            var requestCookies = new HttpRequest($"https://{_httpBinHost}/cookies");
            requestCookies.AllowAutoRedirect = false;
            var responseCookies = Subject.Get<HttpCookieResource>(requestCookies);

            responseCookies.Resource.Cookies.Should().HaveCount(1).And.Contain("my", "cookie");

            ExceptionVerification.IgnoreErrors();
        }

        [Test]
        public void should_delete_request_cookie()
        {
            var requestDelete = new HttpRequest($"https://{_httpBinHost}/cookies/delete?my");
            requestDelete.Cookies.Add("my", "cookie");
            requestDelete.AllowAutoRedirect = true;
            requestDelete.StoreRequestCookie = false;
            requestDelete.StoreResponseCookie = false;

            // Delete and redirect since that's the only way to check the internal temporary cookie container
            var responseCookies = Subject.Get<HttpCookieResource>(requestDelete);

            responseCookies.Resource.Cookies.Should().BeEmpty();
        }

        [Test]
        public void should_clear_request_cookie()
        {
            var requestSet = new HttpRequest($"https://{_httpBinHost}/cookies");
            requestSet.Cookies.Add("my", "cookie");
            requestSet.AllowAutoRedirect = false;
            requestSet.StoreRequestCookie = true;
            requestSet.StoreResponseCookie = false;

            var responseSet = Subject.Get<HttpCookieResource>(requestSet);

            var requestClear = new HttpRequest($"https://{_httpBinHost}/cookies");
            requestClear.Cookies.Add("my", null);
            requestClear.AllowAutoRedirect = false;
            requestClear.StoreRequestCookie = true;
            requestClear.StoreResponseCookie = false;

            var responseClear = Subject.Get<HttpCookieResource>(requestClear);

            responseClear.Resource.Cookies.Should().BeEmpty();
        }

        [Test]
        public void should_not_store_response_cookie()
        {
            var requestSet = new HttpRequest($"https://{_httpBinHost}/cookies/set?my=cookie");
            requestSet.AllowAutoRedirect = false;
            requestSet.StoreRequestCookie = false;
            requestSet.StoreResponseCookie.Should().BeFalse();

            var responseSet = Subject.Get(requestSet);

            var requestCookies = new HttpRequest($"https://{_httpBinHost}/cookies");

            var responseCookies = Subject.Get<HttpCookieResource>(requestCookies);

            responseCookies.Resource.Cookies.Should().BeEmpty();

            ExceptionVerification.IgnoreErrors();
        }

        [Test]
        public void should_store_response_cookie()
        {
            var requestSet = new HttpRequest($"https://{_httpBinHost}/cookies/set?my=cookie");
            requestSet.AllowAutoRedirect = false;
            requestSet.StoreRequestCookie = false;
            requestSet.StoreResponseCookie = true;

            var responseSet = Subject.Get(requestSet);

            var requestCookies = new HttpRequest($"https://{_httpBinHost}/cookies");

            var responseCookies = Subject.Get<HttpCookieResource>(requestCookies);

            responseCookies.Resource.Cookies.Should().HaveCount(1).And.Contain("my", "cookie");

            ExceptionVerification.IgnoreErrors();
        }

        [Test]
        public void should_temp_store_response_cookie()
        {
            var requestSet = new HttpRequest($"https://{_httpBinHost}/cookies/set?my=cookie");
            requestSet.AllowAutoRedirect = true;
            requestSet.StoreRequestCookie = false;
            requestSet.StoreResponseCookie.Should().BeFalse();
            var responseSet = Subject.Get<HttpCookieResource>(requestSet);

            // Set and redirect since that's the only way to check the internal temporary cookie container
            responseSet.Resource.Cookies.Should().HaveCount(1).And.Contain("my", "cookie");

            ExceptionVerification.IgnoreErrors();
        }

        [Test]
        public void should_overwrite_response_cookie()
        {
            var requestSet = new HttpRequest($"https://{_httpBinHost}/cookies/set?my=cookie");
            requestSet.Cookies.Add("my", "oldcookie");
            requestSet.AllowAutoRedirect = false;
            requestSet.StoreRequestCookie = false;
            requestSet.StoreResponseCookie = true;

            var responseSet = Subject.Get(requestSet);

            var requestCookies = new HttpRequest($"https://{_httpBinHost}/cookies");

            var responseCookies = Subject.Get<HttpCookieResource>(requestCookies);

            responseCookies.Resource.Cookies.Should().HaveCount(1).And.Contain("my", "cookie");

            ExceptionVerification.IgnoreErrors();
        }

        [Test]
        public void should_overwrite_temp_response_cookie()
        {
            var requestSet = new HttpRequest($"https://{_httpBinHost}/cookies/set?my=cookie");
            requestSet.Cookies.Add("my", "oldcookie");
            requestSet.AllowAutoRedirect = true;
            requestSet.StoreRequestCookie = true;
            requestSet.StoreResponseCookie = false;

            var responseSet = Subject.Get<HttpCookieResource>(requestSet);

            responseSet.Resource.Cookies.Should().HaveCount(1).And.Contain("my", "cookie");

            var requestCookies = new HttpRequest($"https://{_httpBinHost}/cookies");

            var responseCookies = Subject.Get<HttpCookieResource>(requestCookies);

            responseCookies.Resource.Cookies.Should().HaveCount(1).And.Contain("my", "oldcookie");

            ExceptionVerification.IgnoreErrors();
        }

        [Test]
        public void should_not_delete_response_cookie()
        {
            var requestCookies = new HttpRequest($"https://{_httpBinHost}/cookies");
            requestCookies.Cookies.Add("my", "cookie");
            requestCookies.AllowAutoRedirect = false;
            requestCookies.StoreRequestCookie = true;
            requestCookies.StoreResponseCookie = false;
            var responseCookies = Subject.Get<HttpCookieResource>(requestCookies);

            responseCookies.Resource.Cookies.Should().HaveCount(1).And.Contain("my", "cookie");

            var requestDelete = new HttpRequest($"https://{_httpBinHost}/cookies/delete?my");
            requestDelete.AllowAutoRedirect = false;
            requestDelete.StoreRequestCookie = false;
            requestDelete.StoreResponseCookie = false;

            var responseDelete = Subject.Get(requestDelete);

            requestCookies = new HttpRequest($"https://{_httpBinHost}/cookies");
            requestCookies.StoreRequestCookie = false;
            requestCookies.StoreResponseCookie = false;

            responseCookies = Subject.Get<HttpCookieResource>(requestCookies);

            responseCookies.Resource.Cookies.Should().HaveCount(1).And.Contain("my", "cookie");

            ExceptionVerification.IgnoreErrors();
        }

        [Test]
        public void should_delete_response_cookie()
        {
            var requestCookies = new HttpRequest($"https://{_httpBinHost}/cookies");
            requestCookies.Cookies.Add("my", "cookie");
            requestCookies.AllowAutoRedirect = false;
            requestCookies.StoreRequestCookie = true;
            requestCookies.StoreResponseCookie = false;
            var responseCookies = Subject.Get<HttpCookieResource>(requestCookies);

            responseCookies.Resource.Cookies.Should().HaveCount(1).And.Contain("my", "cookie");

            var requestDelete = new HttpRequest($"https://{_httpBinHost}/cookies/delete?my");
            requestDelete.AllowAutoRedirect = false;
            requestDelete.StoreRequestCookie = false;
            requestDelete.StoreResponseCookie = true;

            var responseDelete = Subject.Get(requestDelete);

            requestCookies = new HttpRequest($"https://{_httpBinHost}/cookies");
            requestCookies.StoreRequestCookie = false;
            requestCookies.StoreResponseCookie = false;

            responseCookies = Subject.Get<HttpCookieResource>(requestCookies);

            responseCookies.Resource.Cookies.Should().BeEmpty();

            ExceptionVerification.IgnoreErrors();
        }

        [Test]
        public void should_delete_temp_response_cookie()
        {
            var requestCookies = new HttpRequest($"https://{_httpBinHost}/cookies");
            requestCookies.Cookies.Add("my", "cookie");
            requestCookies.AllowAutoRedirect = false;
            requestCookies.StoreRequestCookie = true;
            requestCookies.StoreResponseCookie = false;
            var responseCookies = Subject.Get<HttpCookieResource>(requestCookies);

            responseCookies.Resource.Cookies.Should().HaveCount(1).And.Contain("my", "cookie");

            var requestDelete = new HttpRequest($"https://{_httpBinHost}/cookies/delete?my");
            requestDelete.AllowAutoRedirect = true;
            requestDelete.StoreRequestCookie = false;
            requestDelete.StoreResponseCookie = false;
            var responseDelete = Subject.Get<HttpCookieResource>(requestDelete);

            responseDelete.Resource.Cookies.Should().BeEmpty();

            requestCookies = new HttpRequest($"https://{_httpBinHost}/cookies");
            requestCookies.StoreRequestCookie = false;
            requestCookies.StoreResponseCookie = false;

            responseCookies.Resource.Cookies.Should().HaveCount(1).And.Contain("my", "cookie");

            ExceptionVerification.IgnoreErrors();
        }

        [Test]
        public void should_throw_on_http429_too_many_requests()
        {
            var request = new HttpRequest($"https://{_httpBinHost}/status/429");

            Assert.Throws<TooManyRequestsException>(() => Subject.Get(request));

            ExceptionVerification.IgnoreWarns();
        }

        [Test]
        public void should_call_interceptor()
        {
            Mocker.SetConstant<IEnumerable<IHttpRequestInterceptor>>(new[] { Mocker.GetMock<IHttpRequestInterceptor>().Object });

            Mocker.GetMock<IHttpRequestInterceptor>()
                .Setup(v => v.PreRequest(It.IsAny<HttpRequest>()))
                .Returns<HttpRequest>(r => r);

            Mocker.GetMock<IHttpRequestInterceptor>()
                .Setup(v => v.PostResponse(It.IsAny<HttpResponse>()))
                .Returns<HttpResponse>(r => r);

            var request = new HttpRequest($"https://{_httpBinHost}/get");

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
                // the date is bad in the below - should be 13-Jul-2026
                string malformedCookie = @"__cfduid=d29e686a9d65800021c66faca0a29b4261436890790; expires=Mon, 13-Jul-26 16:19:50 GMT; path=/; HttpOnly";
                var requestSet = new HttpRequestBuilder($"https://{_httpBinHost}/response-headers")
                    .AddQueryParam("Set-Cookie", malformedCookie)
                    .Build();

                requestSet.AllowAutoRedirect = false;
                requestSet.StoreResponseCookie = true;

                var responseSet = Subject.Get(requestSet);

                var request = new HttpRequest($"https://{_httpBinHost}/get");

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

        [TestCase("lang_code=en; expires=Wed, 23-Dec-2026 18:09:14 GMT; Max-Age=31536000; path=/; domain=.abc.com")]
        public void should_reject_malformed_domain_cookie(string malformedCookie)
        {
            try
            {
                string url = $"https://{_httpBinHost}/response-headers?Set-Cookie={Uri.EscapeDataString(malformedCookie)}";

                var requestSet = new HttpRequest(url);
                requestSet.AllowAutoRedirect = false;
                requestSet.StoreResponseCookie = true;

                var responseSet = Subject.Get(requestSet);

                var request = new HttpRequest($"https://{_httpBinHost}/get");

                var response = Subject.Get<HttpBinResource>(request);

                response.Resource.Headers.Should().NotContainKey("Cookie");

                ExceptionVerification.IgnoreErrors();
            }
            finally
            {
            }
        }

        [Test]
        public void should_correctly_use_basic_auth()
        {
            var request = new HttpRequest($"https://{_httpBinHost}/basic-auth/username/password");
            request.Credentials = new BasicNetworkCredential("username", "password");

            var response = Subject.Execute(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }

    public class HttpBinResource
    {
        public Dictionary<string, object> Args { get; set; }
        public Dictionary<string, object> Headers { get; set; }
        public string Origin { get; set; }
        public string Url { get; set; }
        public string Data { get; set; }
        public bool Gzipped { get; set; }
        public bool Brotli { get; set; }
    }

    public class HttpCookieResource
    {
        public Dictionary<string, string> Cookies { get; set; }
    }
}
