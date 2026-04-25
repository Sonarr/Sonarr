using System;
using System.Collections.Generic;
using FluentValidation.Results;
using NLog;
using NzbDrone.Core.DecisionEngine.ExternalDecisions.Payloads;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.DecisionEngine.ExternalDecisions
{
    public class WebhookExternalDecision : ExternalDecisionBase<WebhookExternalDecisionSettings>
    {
        private readonly IWebhookExternalDecisionProxy _proxy;
        private readonly Logger _logger;

        public WebhookExternalDecision(IWebhookExternalDecisionProxy proxy, Logger logger)
        {
            _proxy = proxy;
            _logger = logger;
        }

        public override string Name => "Webhook";

        public override ExternalRejectionResponse EvaluateRejection(ExternalRejectionRequest request)
        {
            return _proxy.SendRejectionRequest(request, Settings);
        }

        public override ExternalPrioritizationResponse EvaluatePrioritization(ExternalPrioritizationRequest request)
        {
            return _proxy.SendPrioritizationRequest(request, Settings);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            try
            {
                var testSeries = new ExternalSeriesPayload
                {
                    Id = 0,
                    TvdbId = 0,
                    Title = "Test Series",
                    Year = 2024,
                    Status = "Continuing",
                    SeriesType = "Standard",
                    Runtime = 45,
                    Tags = new HashSet<int>(),
                    QualityProfileId = 0
                };

                var testEpisodes = new List<ExternalEpisodePayload>
                {
                    new ExternalEpisodePayload
                    {
                        Id = 0,
                        SeasonNumber = 1,
                        EpisodeNumber = 1,
                        Title = "Test Episode",
                        AirDate = "2024-01-15",
                        Runtime = 45,
                        HasFile = false
                    }
                };

                switch (DecisionDefinition.DecisionType)
                {
                    case ExternalDecisionType.Prioritization:
                        _proxy.SendPrioritizationRequest(BuildTestPrioritizationRequest(testSeries, testEpisodes), Settings);
                        break;
                    case ExternalDecisionType.Rejection:
                        _proxy.SendRejectionRequest(BuildTestRejectionRequest(testSeries, testEpisodes), Settings);
                        break;
                    default:
                        throw new NotImplementedException($"Test not implemented for decision type: {DecisionDefinition.DecisionType}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to send test request to external decision.");
                failures.Add(new ValidationFailure("Url", $"Unable to connect to external decision: {ex.Message}"));
            }

            return new ValidationResult(failures);
        }

        private static ExternalRejectionRequest BuildTestRejectionRequest(ExternalSeriesPayload series, List<ExternalEpisodePayload> episodes)
        {
            return new ExternalRejectionRequest
            {
                DecisionType = nameof(ExternalDecisionType.Rejection),
                Release = BuildTestRelease("test-guid"),
                Series = series,
                Episodes = episodes
            };
        }

        private static ExternalPrioritizationRequest BuildTestPrioritizationRequest(ExternalSeriesPayload series, List<ExternalEpisodePayload> episodes)
        {
            return new ExternalPrioritizationRequest
            {
                DecisionType = nameof(ExternalDecisionType.Prioritization),
                Releases = new List<ExternalReleasePayload>
                {
                    BuildTestRelease("test-guid-1"),
                    BuildTestRelease("test-guid-2", Quality.HDTV720p, "Test.Release.S01E01.720p.HDTV", 536870912, "torrent", 0, 50, 10)
                },
                Series = series,
                Episodes = episodes
            };
        }

        private static ExternalReleasePayload BuildTestRelease(
            string guid,
            Quality quality = null,
            string title = "Test.Release.S01E01.1080p.WEB-DL",
            long size = 1073741824,
            string protocol = "usenet",
            int age = 1,
            int? seeders = null,
            int? peers = null)
        {
            return new ExternalReleasePayload
            {
                Guid = guid,
                Title = title,
                Indexer = "TestIndexer",
                Quality = new QualityModel(quality ?? Quality.WEBDL1080p, new Revision()),
                Size = size,
                Protocol = protocol,
                Age = age,
                IndexerPriority = 25,
                Seeders = seeders,
                Peers = peers,
                ReleaseType = "SingleEpisode",
                CustomFormats = new List<CustomFormatPayload>(),
                Languages = new List<Languages.Language>()
            };
        }
    }
}
