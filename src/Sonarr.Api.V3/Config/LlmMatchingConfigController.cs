using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser.LlmMatching;
using NzbDrone.Core.Tv;
using Sonarr.Http;
using Sonarr.Http.REST;

namespace Sonarr.Api.V3.Config
{
    /// <summary>
    /// API Controller for LLM matching configuration.
    /// Provides endpoints for managing LLM settings and testing the configuration.
    /// </summary>
    [V3ApiController("config/llmmatching")]
    public class LlmMatchingConfigController : RestController<LlmMatchingConfigResource>
    {
        private readonly IConfigService _configService;
        private readonly ILlmSeriesMatchingService _llmMatchingService;
        private readonly ISeriesService _seriesService;

        public LlmMatchingConfigController(
            IConfigService configService,
            ILlmSeriesMatchingService llmMatchingService,
            ISeriesService seriesService)
        {
            _configService = configService;
            _llmMatchingService = llmMatchingService;
            _seriesService = seriesService;
        }

        protected override LlmMatchingConfigResource GetResourceById(int id)
        {
            return GetLlmMatchingConfig();
        }

        [HttpGet]
        public LlmMatchingConfigResource GetLlmMatchingConfig()
        {
            return new LlmMatchingConfigResource
            {
                Id = 1,
                LlmMatchingEnabled = _configService.LlmMatchingEnabled,
                OpenAiApiKeySet = !string.IsNullOrWhiteSpace(_configService.OpenAiApiKey),
                OpenAiApiEndpoint = _configService.OpenAiApiEndpoint,
                OpenAiModel = _configService.OpenAiModel,
                LlmConfidenceThreshold = _configService.LlmConfidenceThreshold,
                LlmMaxCallsPerHour = _configService.LlmMaxCallsPerHour,
                LlmCacheEnabled = _configService.LlmCacheEnabled,
                LlmCacheDurationHours = _configService.LlmCacheDurationHours,
                IsServiceAvailable = _llmMatchingService.IsEnabled
            };
        }

        [HttpPut]
        public ActionResult<LlmMatchingConfigResource> SaveLlmMatchingConfig([FromBody] LlmMatchingConfigResource resource)
        {
            // Note: Implementation depends on how ConfigService handles updates
            // This would need to be implemented based on Sonarr's actual config saving mechanism
            return Accepted(GetLlmMatchingConfig());
        }

        /// <summary>
        /// Tests the LLM matching configuration by attempting to match a sample title.
        /// </summary>
        [HttpPost("test")]
        public async Task<ActionResult<LlmTestResult>> TestLlmMatching([FromBody] LlmTestRequest request)
        {
            if (!_llmMatchingService.IsEnabled)
            {
                return BadRequest(new LlmTestResult
                {
                    Success = false,
                    ErrorMessage = "LLM matching is not enabled or configured."
                });
            }

            if (string.IsNullOrWhiteSpace(request.TestTitle))
            {
                return BadRequest(new LlmTestResult
                {
                    Success = false,
                    ErrorMessage = "Test title is required."
                });
            }

            try
            {
                var availableSeries = _seriesService.GetAllSeries();
                var result = await _llmMatchingService.TryMatchSeriesAsync(request.TestTitle, availableSeries);

                var testResult = new LlmTestResult
                {
                    Success = true,
                    MatchFound = result?.IsSuccessfulMatch == true,
                    MatchedSeriesTitle = result?.Series?.Title,
                    MatchedSeriesTvdbId = result?.Series?.TvdbId,
                    Confidence = result?.Confidence ?? 0,
                    Reasoning = result?.Reasoning,
                    Alternatives = new List<LlmAlternativeResult>()
                };

                if (result?.Alternatives != null)
                {
                    testResult.Alternatives = result.Alternatives
                        .Select(a => new LlmAlternativeResult
                        {
                            SeriesTitle = a.Series?.Title,
                            TvdbId = a.Series?.TvdbId ?? 0,
                            Confidence = a.Confidence,
                            Reasoning = a.Reasoning
                        })
                        .ToList();
                }

                return Ok(testResult);
            }
            catch (Exception ex)
            {
                return Ok(new LlmTestResult
                {
                    Success = false,
                    ErrorMessage = $"Error testing LLM matching: {ex.Message}"
                });
            }
        }
    }

    /// <summary>
    /// Resource model for LLM matching configuration.
    /// </summary>
    public class LlmMatchingConfigResource : RestResource
    {
        public bool LlmMatchingEnabled { get; set; }

        public bool OpenAiApiKeySet { get; set; }

        public string OpenAiApiKey { get; set; }

        public string OpenAiApiEndpoint { get; set; }

        public string OpenAiModel { get; set; }

        public double LlmConfidenceThreshold { get; set; }

        public int LlmMaxCallsPerHour { get; set; }

        public bool LlmCacheEnabled { get; set; }

        public int LlmCacheDurationHours { get; set; }

        public bool IsServiceAvailable { get; set; }
    }

    /// <summary>
    /// Request model for testing LLM matching.
    /// </summary>
    public class LlmTestRequest
    {
        public string TestTitle { get; set; }
    }

    /// <summary>
    /// Result model for LLM matching test.
    /// </summary>
    public class LlmTestResult
    {
        public bool Success { get; set; }

        public string ErrorMessage { get; set; }

        public bool MatchFound { get; set; }

        public string MatchedSeriesTitle { get; set; }

        public int? MatchedSeriesTvdbId { get; set; }

        public double Confidence { get; set; }

        public string Reasoning { get; set; }

        public List<LlmAlternativeResult> Alternatives { get; set; }
    }

    /// <summary>
    /// Alternative match result for LLM matching test.
    /// </summary>
    public class LlmAlternativeResult
    {
        public string SeriesTitle { get; set; }

        public int TvdbId { get; set; }

        public double Confidence { get; set; }

        public string Reasoning { get; set; }
    }
}
