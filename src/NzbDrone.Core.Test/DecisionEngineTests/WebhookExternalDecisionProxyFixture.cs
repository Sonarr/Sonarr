using System;
using System.Net;
using System.Net.Http;
using System.Text;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.DecisionEngine.ExternalDecisions;
using NzbDrone.Core.DecisionEngine.ExternalDecisions.Payloads;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class WebhookExternalDecisionProxyFixture : CoreTest<WebhookExternalDecisionProxy>
    {
        private WebhookExternalDecisionSettings _settings;
        private ExternalRejectionRequest _payload;

        [SetUp]
        public void Setup()
        {
            _settings = new WebhookExternalDecisionSettings
            {
                Url = "http://decision.local/rejection",
                ApiKey = "test-api-key",
                Timeout = 10
            };

            _payload = new ExternalRejectionRequest
            {
                DecisionType = "Rejection",
                Release = new ExternalReleasePayload
                {
                    Guid = "test-guid",
                    Title = "Test.Series.S01E01.1080p.WEB-DL",
                    Indexer = "TestIndexer",
                    Size = 1073741824,
                    Protocol = "usenet",
                    Age = 1,
                    IndexerPriority = 25
                },
                Series = new ExternalSeriesPayload
                {
                    Id = 1,
                    TvdbId = 123456,
                    Title = "Test Series"
                }
            };
        }

        private void GivenSuccessfulResponse(string json)
        {
            Mocker.GetMock<IHttpClient>()
                  .Setup(c => c.Post(It.IsAny<HttpRequest>()))
                  .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), json, HttpStatusCode.OK));
        }

        private void GivenEmptyResponse()
        {
            Mocker.GetMock<IHttpClient>()
                  .Setup(c => c.Post(It.IsAny<HttpRequest>()))
                  .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), string.Empty, HttpStatusCode.OK));
        }

        private void GivenNoContentResponse()
        {
            Mocker.GetMock<IHttpClient>()
                  .Setup(c => c.Post(It.IsAny<HttpRequest>()))
                  .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), Array.Empty<byte>(), HttpStatusCode.NoContent));
        }

        private void GivenErrorResponse(HttpStatusCode statusCode)
        {
            Mocker.GetMock<IHttpClient>()
                  .Setup(c => c.Post(It.IsAny<HttpRequest>()))
                  .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), "error", statusCode));
        }

        [Test]
        public void should_send_post_request()
        {
            GivenSuccessfulResponse("{\"approved\": true}");

            Subject.SendRejectionRequest(_payload, _settings);

            Mocker.GetMock<IHttpClient>()
                  .Verify(c => c.Post(It.IsAny<HttpRequest>()), Times.Once);
        }

        [Test]
        public void should_set_api_key_header()
        {
            HttpRequest capturedRequest = null;

            Mocker.GetMock<IHttpClient>()
                  .Setup(c => c.Post(It.IsAny<HttpRequest>()))
                  .Callback<HttpRequest>(r => capturedRequest = r)
                  .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), "{\"approved\": true}", HttpStatusCode.OK));

            Subject.SendRejectionRequest(_payload, _settings);

            capturedRequest.Should().NotBeNull();
            capturedRequest.Headers.GetSingleValue("X-Api-Key").Should().Be("test-api-key");
        }

        [Test]
        public void should_not_set_api_key_header_when_empty()
        {
            _settings.ApiKey = null;
            HttpRequest capturedRequest = null;

            Mocker.GetMock<IHttpClient>()
                  .Setup(c => c.Post(It.IsAny<HttpRequest>()))
                  .Callback<HttpRequest>(r => capturedRequest = r)
                  .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), "{\"approved\": true}", HttpStatusCode.OK));

            Subject.SendRejectionRequest(_payload, _settings);

            capturedRequest.Should().NotBeNull();
            capturedRequest.Headers.ContainsKey("X-Api-Key").Should().BeFalse();
        }

        [Test]
        public void should_set_request_timeout()
        {
            HttpRequest capturedRequest = null;

            Mocker.GetMock<IHttpClient>()
                  .Setup(c => c.Post(It.IsAny<HttpRequest>()))
                  .Callback<HttpRequest>(r => capturedRequest = r)
                  .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), "{\"approved\": true}", HttpStatusCode.OK));

            Subject.SendRejectionRequest(_payload, _settings);

            capturedRequest.Should().NotBeNull();
            capturedRequest.RequestTimeout.Should().Be(TimeSpan.FromSeconds(10));
        }

        [Test]
        public void should_set_content_type_json()
        {
            HttpRequest capturedRequest = null;

            Mocker.GetMock<IHttpClient>()
                  .Setup(c => c.Post(It.IsAny<HttpRequest>()))
                  .Callback<HttpRequest>(r => capturedRequest = r)
                  .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), "{\"approved\": true}", HttpStatusCode.OK));

            Subject.SendRejectionRequest(_payload, _settings);

            capturedRequest.Should().NotBeNull();
            capturedRequest.Headers.ContentType.Should().Be("application/json");
        }

        [Test]
        public void should_deserialize_approved_response()
        {
            GivenSuccessfulResponse("{\"approved\": true}");

            var result = Subject.SendRejectionRequest(_payload, _settings);

            result.Approved.Should().BeTrue();
        }

        [Test]
        public void should_deserialize_rejected_response_with_reason()
        {
            GivenSuccessfulResponse("{\"approved\": false, \"reason\": \"RAR archive detected\"}");

            var result = Subject.SendRejectionRequest(_payload, _settings);

            result.Approved.Should().BeFalse();
            result.Reason.Should().Be("RAR archive detected");
        }

        [Test]
        public void should_treat_empty_body_as_approved()
        {
            GivenEmptyResponse();

            var result = Subject.SendRejectionRequest(_payload, _settings);

            result.Approved.Should().BeTrue();
        }

        [Test]
        public void should_treat_no_content_response_as_approved()
        {
            GivenNoContentResponse();

            var result = Subject.SendRejectionRequest(_payload, _settings);

            result.Approved.Should().BeTrue();
        }

        [Test]
        public void should_treat_unexpected_status_code_as_approved()
        {
            GivenErrorResponse(HttpStatusCode.InternalServerError);

            var result = Subject.SendRejectionRequest(_payload, _settings);

            result.Approved.Should().BeTrue();

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_treat_bad_request_as_approved()
        {
            GivenErrorResponse(HttpStatusCode.BadRequest);

            var result = Subject.SendRejectionRequest(_payload, _settings);

            result.Approved.Should().BeTrue();

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_send_request_to_configured_url()
        {
            HttpRequest capturedRequest = null;

            Mocker.GetMock<IHttpClient>()
                  .Setup(c => c.Post(It.IsAny<HttpRequest>()))
                  .Callback<HttpRequest>(r => capturedRequest = r)
                  .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), "{\"approved\": true}", HttpStatusCode.OK));

            Subject.SendRejectionRequest(_payload, _settings);

            capturedRequest.Should().NotBeNull();
            capturedRequest.Url.ToString().Should().StartWith("http://decision.local/rejection");
        }

        // Prioritization proxy tests

        [Test]
        public void should_return_scores_from_prioritization_response()
        {
            GivenSuccessfulResponse("{\"scores\": {\"guid-3\": 100, \"guid-1\": 50, \"guid-2\": 75}}");

            var request = new ExternalPrioritizationRequest { DecisionType = "Prioritization" };
            var result = Subject.SendPrioritizationRequest(request, _settings);

            result.Should().NotBeNull();
            result.Scores.Should().ContainKey("guid-3").WhoseValue.Should().Be(100);
            result.Scores.Should().ContainKey("guid-1").WhoseValue.Should().Be(50);
            result.Scores.Should().ContainKey("guid-2").WhoseValue.Should().Be(75);
        }

        [Test]
        public void should_return_null_for_prioritization_on_timeout()
        {
            Mocker.GetMock<IHttpClient>()
                  .Setup(c => c.Post(It.IsAny<HttpRequest>()))
                  .Throws(new HttpRequestException("Connection timeout"));

            var request = new ExternalPrioritizationRequest { DecisionType = "Prioritization" };
            var result = Subject.SendPrioritizationRequest(request, _settings);

            result.Should().BeNull();

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_return_null_for_prioritization_on_server_error()
        {
            GivenErrorResponse(HttpStatusCode.InternalServerError);

            var request = new ExternalPrioritizationRequest { DecisionType = "Prioritization" };
            var result = Subject.SendPrioritizationRequest(request, _settings);

            result.Should().BeNull();

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_return_null_for_prioritization_on_empty_response()
        {
            GivenEmptyResponse();

            var request = new ExternalPrioritizationRequest { DecisionType = "Prioritization" };
            var result = Subject.SendPrioritizationRequest(request, _settings);

            result.Should().BeNull();
        }

        [Test]
        public void should_include_payload_data_in_request_body()
        {
            HttpRequest capturedRequest = null;

            Mocker.GetMock<IHttpClient>()
                  .Setup(c => c.Post(It.IsAny<HttpRequest>()))
                  .Callback<HttpRequest>(r => capturedRequest = r)
                  .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), "{\"approved\": true}", HttpStatusCode.OK));

            Subject.SendRejectionRequest(_payload, _settings);

            capturedRequest.Should().NotBeNull();
            capturedRequest.ContentData.Should().NotBeEmpty();

            var bodyJson = Encoding.UTF8.GetString(capturedRequest.ContentData);
            var deserialized = Json.Deserialize<ExternalRejectionRequest>(bodyJson);

            deserialized.DecisionType.Should().Be("Rejection");
            deserialized.Release.Guid.Should().Be("test-guid");
            deserialized.Release.Title.Should().Be("Test.Series.S01E01.1080p.WEB-DL");
            deserialized.Series.Id.Should().Be(1);
            deserialized.Series.TvdbId.Should().Be(123456);
        }

        [Test]
        public void should_not_set_api_key_header_when_empty_string()
        {
            _settings.ApiKey = "";
            HttpRequest capturedRequest = null;

            Mocker.GetMock<IHttpClient>()
                  .Setup(c => c.Post(It.IsAny<HttpRequest>()))
                  .Callback<HttpRequest>(r => capturedRequest = r)
                  .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), "{\"approved\": true}", HttpStatusCode.OK));

            Subject.SendRejectionRequest(_payload, _settings);

            capturedRequest.Should().NotBeNull();
            capturedRequest.Headers.ContainsKey("X-Api-Key").Should().BeFalse();
        }
    }
}
