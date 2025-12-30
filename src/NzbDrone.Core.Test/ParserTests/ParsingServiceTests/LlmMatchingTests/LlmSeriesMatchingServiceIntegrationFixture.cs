using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NLog;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser.LlmMatching;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.ParserTests.ParsingServiceTests.LlmMatchingTests
{
    /// <summary>
    /// Integration tests for OpenAiSeriesMatchingService.
    /// These tests verify service logic without making actual API calls.
    /// </summary>
    [TestFixture]
    public class OpenAiSeriesMatchingServiceIntegrationFixture
    {
        private Mock<IConfigService> _configService;
        private Mock<IHttpClientFactory> _httpClientFactory;
        private OpenAiSeriesMatchingService _subject;
        private List<Series> _testSeries;

        [SetUp]
        public void Setup()
        {
            _configService = new Mock<IConfigService>();
            _httpClientFactory = new Mock<IHttpClientFactory>();

            _configService.Setup(s => s.LlmMatchingEnabled).Returns(true);
            _configService.Setup(s => s.OpenAiApiKey).Returns("test-api-key-12345");
            _configService.Setup(s => s.OpenAiApiEndpoint).Returns("https://api.openai.com/v1/chat/completions");
            _configService.Setup(s => s.OpenAiModel).Returns("gpt-4o-mini");
            _configService.Setup(s => s.LlmConfidenceThreshold).Returns(0.7);

            _subject = new OpenAiSeriesMatchingService(
                _configService.Object,
                _httpClientFactory.Object,
                LogManager.GetCurrentClassLogger());

            _testSeries = new List<Series>
            {
                new Series
                {
                    Id = 1,
                    TvdbId = 81189,
                    Title = "Breaking Bad",
                    CleanTitle = "breakingbad",
                    Year = 2008,
                    SeriesType = SeriesTypes.Standard
                },
                new Series
                {
                    Id = 2,
                    TvdbId = 121361,
                    Title = "Game of Thrones",
                    CleanTitle = "gameofthrones",
                    Year = 2011,
                    SeriesType = SeriesTypes.Standard
                },
                new Series
                {
                    Id = 3,
                    TvdbId = 267440,
                    Title = "Attack on Titan",
                    CleanTitle = "attackontitan",
                    Year = 2013,
                    SeriesType = SeriesTypes.Anime
                }
            };
        }

        [Test]
        public void IsEnabled_returns_true_when_properly_configured()
        {
            _subject.IsEnabled.Should().BeTrue();
        }

        [Test]
        public void IsEnabled_returns_false_when_disabled_in_config()
        {
            _configService.Setup(s => s.LlmMatchingEnabled).Returns(false);

            var subject = new OpenAiSeriesMatchingService(
                _configService.Object,
                _httpClientFactory.Object,
                LogManager.GetCurrentClassLogger());

            subject.IsEnabled.Should().BeFalse();
        }

        [Test]
        public void IsEnabled_returns_false_when_api_key_empty()
        {
            _configService.Setup(s => s.OpenAiApiKey).Returns(string.Empty);

            var subject = new OpenAiSeriesMatchingService(
                _configService.Object,
                _httpClientFactory.Object,
                LogManager.GetCurrentClassLogger());

            subject.IsEnabled.Should().BeFalse();
        }

        [Test]
        public void IsEnabled_returns_false_when_api_key_null()
        {
            _configService.Setup(s => s.OpenAiApiKey).Returns((string)null);

            var subject = new OpenAiSeriesMatchingService(
                _configService.Object,
                _httpClientFactory.Object,
                LogManager.GetCurrentClassLogger());

            subject.IsEnabled.Should().BeFalse();
        }

        [Test]
        public void IsEnabled_returns_false_when_api_key_whitespace()
        {
            _configService.Setup(s => s.OpenAiApiKey).Returns("   ");

            var subject = new OpenAiSeriesMatchingService(
                _configService.Object,
                _httpClientFactory.Object,
                LogManager.GetCurrentClassLogger());

            subject.IsEnabled.Should().BeFalse();
        }

        [Test]
        public async Task TryMatchSeriesAsync_string_returns_null_when_disabled()
        {
            _configService.Setup(s => s.LlmMatchingEnabled).Returns(false);

            var subject = new OpenAiSeriesMatchingService(
                _configService.Object,
                _httpClientFactory.Object,
                LogManager.GetCurrentClassLogger());

            var result = await subject.TryMatchSeriesAsync("Breaking.Bad.S01E01", _testSeries);

            result.Should().BeNull();
        }

        [Test]
        public async Task TryMatchSeriesAsync_string_returns_null_when_title_empty()
        {
            var result = await _subject.TryMatchSeriesAsync(string.Empty, _testSeries);

            result.Should().BeNull();
        }

        [Test]
        public async Task TryMatchSeriesAsync_string_returns_null_when_title_null()
        {
            var result = await _subject.TryMatchSeriesAsync((string)null, _testSeries);

            result.Should().BeNull();
        }

        [Test]
        public async Task TryMatchSeriesAsync_string_returns_null_when_series_list_empty()
        {
            var result = await _subject.TryMatchSeriesAsync("Breaking.Bad.S01E01", new List<Series>());

            result.Should().BeNull();
        }

        [Test]
        public async Task TryMatchSeriesAsync_string_returns_null_when_series_list_null()
        {
            var result = await _subject.TryMatchSeriesAsync("Breaking.Bad.S01E01", null);

            result.Should().BeNull();
        }

        [Test]
        public async Task TryMatchSeriesAsync_ParsedEpisodeInfo_returns_null_when_disabled()
        {
            _configService.Setup(s => s.LlmMatchingEnabled).Returns(false);

            var subject = new OpenAiSeriesMatchingService(
                _configService.Object,
                _httpClientFactory.Object,
                LogManager.GetCurrentClassLogger());

            var parsedInfo = new ParsedEpisodeInfo
            {
                SeriesTitle = "Breaking Bad",
                ReleaseTitle = "Breaking.Bad.S01E01"
            };

            var result = await subject.TryMatchSeriesAsync(parsedInfo, _testSeries);

            result.Should().BeNull();
        }

        [Test]
        public async Task TryMatchSeriesAsync_ParsedEpisodeInfo_returns_null_when_no_title()
        {
            var parsedInfo = new ParsedEpisodeInfo
            {
                SeriesTitle = null,
                ReleaseTitle = null
            };

            var result = await _subject.TryMatchSeriesAsync(parsedInfo, _testSeries);

            result.Should().BeNull();
        }

        [Test]
        public void LlmMatchResult_IsSuccessfulMatch_true_at_threshold()
        {
            var result = new LlmMatchResult
            {
                Series = _testSeries.First(),
                Confidence = 0.7
            };

            result.IsSuccessfulMatch.Should().BeTrue();
        }

        [Test]
        public void LlmMatchResult_IsSuccessfulMatch_true_above_threshold()
        {
            var result = new LlmMatchResult
            {
                Series = _testSeries.First(),
                Confidence = 0.95
            };

            result.IsSuccessfulMatch.Should().BeTrue();
        }

        [Test]
        public void LlmMatchResult_IsSuccessfulMatch_false_below_threshold()
        {
            var result = new LlmMatchResult
            {
                Series = _testSeries.First(),
                Confidence = 0.69
            };

            result.IsSuccessfulMatch.Should().BeFalse();
        }

        [Test]
        public void LlmMatchResult_IsSuccessfulMatch_false_when_series_null()
        {
            var result = new LlmMatchResult
            {
                Series = null,
                Confidence = 0.95
            };

            result.IsSuccessfulMatch.Should().BeFalse();
        }

        [Test]
        public void LlmMatchResult_Alternatives_initialized_empty()
        {
            var result = new LlmMatchResult();

            result.Alternatives.Should().NotBeNull();
            result.Alternatives.Should().BeEmpty();
        }

        [Test]
        public void LlmMatchResult_can_hold_multiple_alternatives()
        {
            var result = new LlmMatchResult
            {
                Series = _testSeries[0],
                Confidence = 0.6,
                Alternatives = new List<AlternativeMatch>
                {
                    new AlternativeMatch { Series = _testSeries[1], Confidence = 0.4 },
                    new AlternativeMatch { Series = _testSeries[2], Confidence = 0.3 }
                }
            };

            result.Alternatives.Should().HaveCount(2);
        }
    }

    /// <summary>
    /// Tests for the caching decorator service.
    /// </summary>
    [TestFixture]
    public class CachedLlmSeriesMatchingServiceIntegrationFixture
    {
        private Mock<IConfigService> _configService;
        private Mock<IHttpClientFactory> _httpClientFactory;
        private Mock<OpenAiSeriesMatchingService> _innerService;
        private CachedLlmSeriesMatchingService _subject;
        private List<Series> _testSeries;

        [SetUp]
        public void Setup()
        {
            _configService = new Mock<IConfigService>();
            _httpClientFactory = new Mock<IHttpClientFactory>();

            _configService.Setup(s => s.LlmCacheEnabled).Returns(true);
            _configService.Setup(s => s.LlmCacheDurationHours).Returns(24);

            _innerService = new Mock<OpenAiSeriesMatchingService>(
                _configService.Object,
                _httpClientFactory.Object,
                LogManager.GetCurrentClassLogger());

            _innerService.Setup(s => s.IsEnabled).Returns(true);

            _subject = new CachedLlmSeriesMatchingService(
                _innerService.Object,
                _configService.Object,
                LogManager.GetCurrentClassLogger());

            _testSeries = new List<Series>
            {
                new Series { Id = 1, TvdbId = 81189, Title = "Breaking Bad" },
                new Series { Id = 2, TvdbId = 121361, Title = "Game of Thrones" }
            };
        }

        [Test]
        public void IsEnabled_delegates_to_inner_service()
        {
            _subject.IsEnabled.Should().BeTrue();

            _innerService.Verify(s => s.IsEnabled, Times.Once);
        }

        [Test]
        public async Task TryMatchSeriesAsync_calls_inner_service_on_cache_miss()
        {
            var expectedResult = new LlmMatchResult
            {
                Series = _testSeries.First(),
                Confidence = 0.9,
                Reasoning = "Test"
            };

            _innerService
                .Setup(s => s.TryMatchSeriesAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Series>>()))
                .ReturnsAsync(expectedResult);

            var result = await _subject.TryMatchSeriesAsync("Breaking.Bad.S01E01", _testSeries);

            result.Should().NotBeNull();
            _innerService.Verify(
                s => s.TryMatchSeriesAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Series>>()),
                Times.Once);
        }

        [Test]
        public async Task TryMatchSeriesAsync_returns_cached_result_on_cache_hit()
        {
            var expectedResult = new LlmMatchResult
            {
                Series = _testSeries.First(),
                Confidence = 0.9,
                Reasoning = "Test"
            };

            _innerService
                .Setup(s => s.TryMatchSeriesAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Series>>()))
                .ReturnsAsync(expectedResult);

            // First call - cache miss
            await _subject.TryMatchSeriesAsync("Breaking.Bad.S01E01", _testSeries);

            // Second call - cache hit
            await _subject.TryMatchSeriesAsync("Breaking.Bad.S01E01", _testSeries);

            // Inner service should only be called once
            _innerService.Verify(
                s => s.TryMatchSeriesAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Series>>()),
                Times.Once);
        }

        [Test]
        public async Task TryMatchSeriesAsync_bypasses_cache_when_disabled()
        {
            _configService.Setup(s => s.LlmCacheEnabled).Returns(false);

            var expectedResult = new LlmMatchResult
            {
                Series = _testSeries.First(),
                Confidence = 0.9
            };

            _innerService
                .Setup(s => s.TryMatchSeriesAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Series>>()))
                .ReturnsAsync(expectedResult);

            await _subject.TryMatchSeriesAsync("Breaking.Bad.S01E01", _testSeries);
            await _subject.TryMatchSeriesAsync("Breaking.Bad.S01E01", _testSeries);

            // Inner service should be called twice (no caching)
            _innerService.Verify(
                s => s.TryMatchSeriesAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Series>>()),
                Times.Exactly(2));
        }

        [Test]
        public async Task TryMatchSeriesAsync_uses_different_cache_keys_for_different_titles()
        {
            var result1 = new LlmMatchResult { Series = _testSeries[0], Confidence = 0.9 };
            var result2 = new LlmMatchResult { Series = _testSeries[1], Confidence = 0.85 };

            _innerService
                .SetupSequence(s => s.TryMatchSeriesAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Series>>()))
                .ReturnsAsync(result1)
                .ReturnsAsync(result2);

            await _subject.TryMatchSeriesAsync("Breaking.Bad.S01E01", _testSeries);
            await _subject.TryMatchSeriesAsync("Game.of.Thrones.S01E01", _testSeries);

            // Both should result in calls to inner service
            _innerService.Verify(
                s => s.TryMatchSeriesAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Series>>()),
                Times.Exactly(2));
        }
    }

    /// <summary>
    /// Tests for the rate limiting decorator service.
    /// </summary>
    [TestFixture]
    public class RateLimitedLlmSeriesMatchingServiceIntegrationFixture
    {
        private Mock<IConfigService> _configService;
        private Mock<IHttpClientFactory> _httpClientFactory;
        private Mock<CachedLlmSeriesMatchingService> _innerService;
        private RateLimitedLlmSeriesMatchingService _subject;
        private List<Series> _testSeries;

        [SetUp]
        public void Setup()
        {
            _configService = new Mock<IConfigService>();
            _httpClientFactory = new Mock<IHttpClientFactory>();
            _configService.Setup(s => s.LlmMaxCallsPerHour).Returns(60);
            _configService.Setup(s => s.LlmCacheEnabled).Returns(true);
            _configService.Setup(s => s.LlmCacheDurationHours).Returns(24);

            var openAiMock = new Mock<OpenAiSeriesMatchingService>(
                _configService.Object,
                _httpClientFactory.Object,
                LogManager.GetCurrentClassLogger());

            _innerService = new Mock<CachedLlmSeriesMatchingService>(
                openAiMock.Object,
                _configService.Object,
                LogManager.GetCurrentClassLogger());

            _innerService.Setup(s => s.IsEnabled).Returns(true);

            _subject = new RateLimitedLlmSeriesMatchingService(
                _innerService.Object,
                _configService.Object,
                LogManager.GetCurrentClassLogger());

            _testSeries = new List<Series>
            {
                new Series { Id = 1, TvdbId = 81189, Title = "Breaking Bad" }
            };
        }

        [Test]
        public void IsEnabled_delegates_to_inner_service()
        {
            _subject.IsEnabled.Should().BeTrue();
        }

        [Test]
        public async Task TryMatchSeriesAsync_allows_calls_within_limit()
        {
            _configService.Setup(s => s.LlmMaxCallsPerHour).Returns(10);

            var expectedResult = new LlmMatchResult
            {
                Series = _testSeries.First(),
                Confidence = 0.9
            };

            _innerService
                .Setup(s => s.TryMatchSeriesAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Series>>()))
                .ReturnsAsync(expectedResult);

            var results = new List<LlmMatchResult>();

            for (var i = 0; i < 10; i++)
            {
                var result = await _subject.TryMatchSeriesAsync($"Title{i}.S01E01", _testSeries);
                results.Add(result);
            }

            results.Should().AllSatisfy(r => r.Should().NotBeNull());
        }

        [Test]
        public async Task TryMatchSeriesAsync_blocks_calls_over_limit()
        {
            _configService.Setup(s => s.LlmMaxCallsPerHour).Returns(3);

            var expectedResult = new LlmMatchResult
            {
                Series = _testSeries.First(),
                Confidence = 0.9
            };

            _innerService
                .Setup(s => s.TryMatchSeriesAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Series>>()))
                .ReturnsAsync(expectedResult);

            // Make 3 calls (at limit)
            await _subject.TryMatchSeriesAsync("Title1.S01E01", _testSeries);
            await _subject.TryMatchSeriesAsync("Title2.S01E01", _testSeries);
            await _subject.TryMatchSeriesAsync("Title3.S01E01", _testSeries);

            // 4th call should be blocked
            var result = await _subject.TryMatchSeriesAsync("Title4.S01E01", _testSeries);

            result.Should().BeNull();

            // Only 3 calls should have reached inner service
            _innerService.Verify(
                s => s.TryMatchSeriesAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Series>>()),
                Times.Exactly(3));
        }

        [Test]
        public async Task TryMatchSeriesAsync_with_ParsedEpisodeInfo_respects_rate_limit()
        {
            _configService.Setup(s => s.LlmMaxCallsPerHour).Returns(2);

            var expectedResult = new LlmMatchResult
            {
                Series = _testSeries.First(),
                Confidence = 0.9
            };

            _innerService
                .Setup(s => s.TryMatchSeriesAsync(It.IsAny<ParsedEpisodeInfo>(), It.IsAny<IEnumerable<Series>>()))
                .ReturnsAsync(expectedResult);

            var parsedInfo = new ParsedEpisodeInfo
            {
                SeriesTitle = "Breaking Bad",
                ReleaseTitle = "Breaking.Bad.S01E01"
            };

            // First two calls succeed
            var result1 = await _subject.TryMatchSeriesAsync(parsedInfo, _testSeries);
            var result2 = await _subject.TryMatchSeriesAsync(parsedInfo, _testSeries);

            // Third call blocked
            var result3 = await _subject.TryMatchSeriesAsync(parsedInfo, _testSeries);

            result1.Should().NotBeNull();
            result2.Should().NotBeNull();
            result3.Should().BeNull();
        }

        [Test]
        public async Task TryMatchSeriesAsync_rate_limit_applies_across_both_methods()
        {
            _configService.Setup(s => s.LlmMaxCallsPerHour).Returns(2);

            var expectedResult = new LlmMatchResult
            {
                Series = _testSeries.First(),
                Confidence = 0.9
            };

            _innerService
                .Setup(s => s.TryMatchSeriesAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Series>>()))
                .ReturnsAsync(expectedResult);

            _innerService
                .Setup(s => s.TryMatchSeriesAsync(It.IsAny<ParsedEpisodeInfo>(), It.IsAny<IEnumerable<Series>>()))
                .ReturnsAsync(expectedResult);

            // One call with string
            await _subject.TryMatchSeriesAsync("Breaking.Bad.S01E01", _testSeries);

            // One call with ParsedEpisodeInfo
            var parsedInfo = new ParsedEpisodeInfo { SeriesTitle = "Test", ReleaseTitle = "Test.S01E01" };
            await _subject.TryMatchSeriesAsync(parsedInfo, _testSeries);

            // Third call (either type) should be blocked
            var result = await _subject.TryMatchSeriesAsync("Another.Title.S01E01", _testSeries);

            result.Should().BeNull();
        }
    }
}
