using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Parser.LlmMatching
{
    /// <summary>
    /// OpenAI-based implementation of ILlmSeriesMatchingService.
    /// Uses GPT models to intelligently match release titles to series when traditional parsing fails.
    /// </summary>
    public class OpenAiSeriesMatchingService : ILlmSeriesMatchingService
    {
        private const string DefaultApiEndpoint = "https://api.openai.com/v1/chat/completions";
        private const string DefaultModel = "gpt-4o-mini";
        private const int MaxSeriesInPrompt = 100;

        private readonly IConfigService _configService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Logger _logger;

        public OpenAiSeriesMatchingService(
            IConfigService configService,
            IHttpClientFactory httpClientFactory,
            Logger logger)
        {
            _configService = configService;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public virtual bool IsEnabled => _configService.LlmMatchingEnabled &&
                                          _configService.OpenAiApiKey.IsNotNullOrWhiteSpace();

        public virtual async Task<LlmMatchResult> TryMatchSeriesAsync(
            ParsedEpisodeInfo parsedEpisodeInfo,
            IEnumerable<Series> availableSeries)
        {
            if (!IsEnabled)
            {
                _logger.Trace("LLM matching is disabled or not configured");
                return null;
            }

            var releaseTitle = parsedEpisodeInfo?.ReleaseTitle ?? parsedEpisodeInfo?.SeriesTitle;

            if (releaseTitle.IsNullOrWhiteSpace())
            {
                _logger.Debug("No release title available for LLM matching");
                return null;
            }

            return await TryMatchSeriesInternalAsync(releaseTitle, parsedEpisodeInfo, availableSeries);
        }

        public virtual async Task<LlmMatchResult> TryMatchSeriesAsync(
            string releaseTitle,
            IEnumerable<Series> availableSeries)
        {
            if (!IsEnabled)
            {
                _logger.Trace("LLM matching is disabled or not configured");
                return null;
            }

            if (releaseTitle.IsNullOrWhiteSpace())
            {
                _logger.Debug("No release title provided for LLM matching");
                return null;
            }

            return await TryMatchSeriesInternalAsync(releaseTitle, null, availableSeries);
        }

        private async Task<LlmMatchResult> TryMatchSeriesInternalAsync(
            string releaseTitle,
            ParsedEpisodeInfo parsedEpisodeInfo,
            IEnumerable<Series> availableSeries)
        {
            var seriesList = availableSeries?.ToList() ?? new List<Series>();

            if (!seriesList.Any())
            {
                _logger.Debug("No series available for LLM matching");
                return null;
            }

            try
            {
                var prompt = BuildMatchingPrompt(releaseTitle, parsedEpisodeInfo, seriesList);
                var response = await CallOpenAiAsync(prompt);
                var result = ParseLlmResponse(response, seriesList);

                if (result?.IsSuccessfulMatch == true)
                {
                    _logger.Info(
                        "LLM matched '{0}' to series '{1}' (TvdbId: {2}) with {3:P0} confidence. Reasoning: {4}",
                        releaseTitle,
                        result.Series.Title,
                        result.Series.TvdbId,
                        result.Confidence,
                        result.Reasoning);
                }
                else if (result != null)
                {
                    _logger.Debug(
                        "LLM could not confidently match '{0}'. Best guess: {1} ({2:P0}). Reasoning: {3}",
                        releaseTitle,
                        result.Series?.Title ?? "None",
                        result.Confidence,
                        result.Reasoning);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error during LLM series matching for '{0}'", releaseTitle);
                return null;
            }
        }

        private string BuildMatchingPrompt(
            string releaseTitle,
            ParsedEpisodeInfo parsedEpisodeInfo,
            List<Series> seriesList)
        {
            var seriesInfo = seriesList
                .Take(MaxSeriesInPrompt)
                .Select((s, i) => new
                {
                    Index = i + 1,
                    s.TvdbId,
                    s.Title,
                    s.CleanTitle,
                    s.Year,
                    s.Network,
                    s.SeriesType
                })
                .ToList();

            var additionalContext = string.Empty;

            if (parsedEpisodeInfo != null)
            {
                additionalContext = $@"
Additional parsed information:
- Parsed Series Title: {parsedEpisodeInfo.SeriesTitle}
- Season Number: {parsedEpisodeInfo.SeasonNumber}
- Episode Numbers: {string.Join(", ", parsedEpisodeInfo.EpisodeNumbers ?? Array.Empty<int>())}
- Is Anime: {parsedEpisodeInfo.IsAbsoluteNumbering}
- Is Daily Show: {parsedEpisodeInfo.IsDaily}
- Year (if detected): {parsedEpisodeInfo.SeriesTitleInfo?.Year}";
            }

            return $@"You are a TV series matching assistant for a media management system. Your task is to match a release title to the correct series from a user's library.

RELEASE TITLE TO MATCH:
""{releaseTitle}""
{additionalContext}

AVAILABLE SERIES IN LIBRARY (JSON format):
{JsonSerializer.Serialize(seriesInfo, new JsonSerializerOptions { WriteIndented = true })}

INSTRUCTIONS:
1. Analyze the release title to identify the series name, handling common scene naming conventions:
   - Dots/underscores replacing spaces (e.g., ""Breaking.Bad"" = ""Breaking Bad"")
   - Year suffixes for disambiguation (e.g., ""Doctor.Who.2005"")
   - Alternate/localized titles (e.g., Japanese/Korean titles for anime)
   - Common abbreviations and scene group tags
   
2. Match the title against the available series, considering:
   - Exact title matches
   - Partial matches with high similarity
   - Year matching for disambiguation
   - Series type (Anime vs Standard) matching patterns
   
3. Return your response in the following JSON format ONLY (no additional text):
{{
    ""matchedTvdbId"": <number or null if no confident match>,
    ""confidence"": <number from 0.0 to 1.0>,
    ""reasoning"": ""<brief explanation of your matching logic>"",
    ""alternatives"": [
        {{
            ""tvdbId"": <number>,
            ""confidence"": <number>,
            ""reasoning"": ""<why this could also be a match>""
        }}
    ]
}}

CONFIDENCE GUIDELINES:
- 0.9-1.0: Exact or near-exact match
- 0.7-0.9: High confidence match with minor differences
- 0.5-0.7: Moderate confidence, may need verification
- 0.0-0.5: Low confidence, likely wrong

If you cannot find a confident match (< 0.5), set matchedTvdbId to null.";
        }

        private async Task<string> CallOpenAiAsync(string prompt)
        {
            var apiKey = _configService.OpenAiApiKey;
            var endpoint = _configService.OpenAiApiEndpoint.IsNotNullOrWhiteSpace()
                ? _configService.OpenAiApiEndpoint
                : DefaultApiEndpoint;
            var model = _configService.OpenAiModel.IsNotNullOrWhiteSpace()
                ? _configService.OpenAiModel
                : DefaultModel;

            var client = _httpClientFactory.CreateClient("OpenAI");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var requestBody = new
            {
                model,
                messages = new[]
                {
                    new { role = "system", content = "You are a precise TV series matching assistant. Always respond with valid JSON only." },
                    new { role = "user", content = prompt }
                },
                temperature = 0.1,
                max_tokens = 500,
                response_format = new { type = "json_object" }
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            _logger.Trace("Calling OpenAI API for series matching");

            var response = await client.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var responseObj = JsonSerializer.Deserialize<OpenAiResponse>(responseJson);

            return responseObj?.Choices?.FirstOrDefault()?.Message?.Content;
        }

        private LlmMatchResult ParseLlmResponse(string responseJson, List<Series> seriesList)
        {
            if (responseJson.IsNullOrWhiteSpace())
            {
                return null;
            }

            try
            {
                var response = JsonSerializer.Deserialize<LlmMatchResponse>(
                    responseJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (response == null)
                {
                    return null;
                }

                var result = new LlmMatchResult
                {
                    Confidence = response.Confidence,
                    Reasoning = response.Reasoning
                };

                if (response.MatchedTvdbId.HasValue)
                {
                    result.Series = seriesList.FirstOrDefault(s => s.TvdbId == response.MatchedTvdbId.Value);
                }

                if (response.Alternatives != null)
                {
                    foreach (var alt in response.Alternatives)
                    {
                        var altSeries = seriesList.FirstOrDefault(s => s.TvdbId == alt.TvdbId);

                        if (altSeries != null)
                        {
                            result.Alternatives.Add(new AlternativeMatch
                            {
                                Series = altSeries,
                                Confidence = alt.Confidence,
                                Reasoning = alt.Reasoning
                            });
                        }
                    }
                }

                return result;
            }
            catch (JsonException ex)
            {
                _logger.Warn(ex, "Failed to parse LLM response JSON: {0}", responseJson);
                return null;
            }
        }

        private class OpenAiResponse
        {
            [JsonPropertyName("choices")]
            public List<Choice> Choices { get; set; }
        }

        private class Choice
        {
            [JsonPropertyName("message")]
            public Message Message { get; set; }
        }

        private class Message
        {
            [JsonPropertyName("content")]
            public string Content { get; set; }
        }

        private class LlmMatchResponse
        {
            [JsonPropertyName("matchedTvdbId")]
            public int? MatchedTvdbId { get; set; }

            [JsonPropertyName("confidence")]
            public double Confidence { get; set; }

            [JsonPropertyName("reasoning")]
            public string Reasoning { get; set; }

            [JsonPropertyName("alternatives")]
            public List<AlternativeMatchResponse> Alternatives { get; set; }
        }

        private class AlternativeMatchResponse
        {
            [JsonPropertyName("tvdbId")]
            public int TvdbId { get; set; }

            [JsonPropertyName("confidence")]
            public double Confidence { get; set; }

            [JsonPropertyName("reasoning")]
            public string Reasoning { get; set; }
        }
    }
}
