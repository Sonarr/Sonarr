using System;
using System.Net;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.HealthCheck;
using NzbDrone.Core.HealthCheck.Checks;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class ProxyCheckFixture : CoreTest<ProxyCheck>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.SetConstant<ISonarrCloudRequestBuilder>(new SonarrCloudRequestBuilder());

            Mocker.GetMock<ILocalizationService>()
                  .Setup(s => s.GetLocalizedString(It.IsAny<string>()))
                  .Returns("Some Warning Message");

            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.ProxyEnabled)
                  .Returns(true);

            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.ProxyHostname)
                  .Returns("127.0.0.1");
        }

        private void GivenPingResponse(HttpStatusCode statusCode)
        {
            // Mirrors HttpClient: a 4xx/5xx throws unless the request suppresses HTTP errors
            Mocker.GetMock<IHttpClient>()
                  .Setup(s => s.Execute(It.IsAny<HttpRequest>()))
                  .Returns<HttpRequest>(r =>
                  {
                      var response = new HttpResponse(r, new HttpHeader(), Array.Empty<byte>(), statusCode);

                      if (!r.SuppressHttpError && response.HasHttpError)
                      {
                          throw new HttpException(r, response);
                      }

                      return response;
                  });
        }

        [Test]
        public void should_return_ok_when_ping_succeeds()
        {
            GivenPingResponse(HttpStatusCode.OK);

            Subject.Check().ShouldBeOk();
        }

        [TestCase(HttpStatusCode.NotFound)]
        [TestCase(HttpStatusCode.ServiceUnavailable)]
        [TestCase((HttpStatusCode)521)]
        public void should_return_ok_when_services_return_an_error_other_than_bad_request(HttpStatusCode statusCode)
        {
            GivenPingResponse(statusCode);

            Subject.Check().ShouldBeOk();
        }

        [Test]
        public void should_return_error_when_ping_returns_bad_request()
        {
            GivenPingResponse(HttpStatusCode.BadRequest);

            var result = Subject.Check();

            result.ShouldBeError();
            result.Reason.Should().Be(HealthCheckReason.ProxyBadRequest);
            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_return_error_when_request_through_proxy_fails()
        {
            Mocker.GetMock<IHttpClient>()
                  .Setup(s => s.Execute(It.IsAny<HttpRequest>()))
                  .Throws(new WebException("Proxy connection failed"));

            var result = Subject.Check();

            result.ShouldBeError();
            result.Reason.Should().Be(HealthCheckReason.ProxyFailed);
            ExceptionVerification.ExpectedErrors(1);
        }
    }
}
