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
    [TestFixture]
    public class LlmSeriesMatchingServiceFixture
    {
        private Mock<IConfigService> _configService;
        private Mock<ILlmSeriesMatchingService> _mockLlmService;
        private List<Series> _testSeries;

        [SetUp]
        public void Setup()
        {
            _configService = new Mock<IConfigService>();
            _mockLlmService = new Mock<ILlmSeriesMatchingService>();

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
                },
                new Series
                {
                    Id = 4,
                    TvdbId = 153021,
                    Title = "The Walking Dead",
                    CleanTitle = "thewalkingdead",
                    Year = 2010,
                    SeriesType = SeriesTypes.Standard
                }
            };
        }

        [Test]
        public void IsEnabled_should_return_false_when_llm_matching_disabled()
        {
            _configService.Setup(s => s.LlmMatchingEnabled).Returns(false);
            _configService.Setup(s => s.OpenAiApiKey).Returns("test-key");

            _mockLlmService.Setup(s => s.IsEnabled).Returns(false);

            _mockLlmService.Object.IsEnabled.Should().BeFalse();
        }

        [Test]
        public void IsEnabled_should_return_false_when_api_key_is_empty()
        {
            _configService.Setup(s => s.LlmMatchingEnabled).Returns(true);
            _configService.Setup(s => s.OpenAiApiKey).Returns(string.Empty);

            _mockLlmService.Setup(s => s.IsEnabled).Returns(false);

            _mockLlmService.Object.IsEnabled.Should().BeFalse();
        }

        [Test]
        public void IsEnabled_should_return_false_when_api_key_is_null()
        {
            _configService.Setup(s => s.LlmMatchingEnabled).Returns(true);
            _configService.Setup(s => s.OpenAiApiKey).Returns((string)null);

            _mockLlmService.Setup(s => s.IsEnabled).Returns(false);

            _mockLlmService.Object.IsEnabled.Should().BeFalse();
        }

        [Test]
        public void IsEnabled_should_return_true_when_properly_configured()
        {
            _configService.Setup(s => s.LlmMatchingEnabled).Returns(true);
            _configService.Setup(s => s.OpenAiApiKey).Returns("sk-test-key-12345");

            _mockLlmService.Setup(s => s.IsEnabled).Returns(true);

            _mockLlmService.Object.IsEnabled.Should().BeTrue();
        }

        [Test]
        public async Task TryMatchSeriesAsync_should_return_null_when_disabled()
        {
            _mockLlmService.Setup(s => s.IsEnabled).Returns(false);
            _mockLlmService
                .Setup(s => s.TryMatchSeriesAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Series>>()))
                .ReturnsAsync((LlmMatchResult)null);

            var result = await _mockLlmService.Object.TryMatchSeriesAsync("Breaking.Bad.S01E01", _testSeries);

            result.Should().BeNull();
        }

        [Test]
        public async Task TryMatchSeriesAsync_should_return_null_when_no_series_available()
        {
            _mockLlmService.Setup(s => s.IsEnabled).Returns(true);
            _mockLlmService
                .Setup(s => s.TryMatchSeriesAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Series>>()))
                .ReturnsAsync((LlmMatchResult)null);

            var result = await _mockLlmService.Object.TryMatchSeriesAsync("Breaking.Bad.S01E01", new List<Series>());

            result.Should().BeNull();
        }

        [Test]
        public async Task TryMatchSeriesAsync_should_return_null_when_title_is_empty()
        {
            _mockLlmService.Setup(s => s.IsEnabled).Returns(true);
            _mockLlmService
                .Setup(s => s.TryMatchSeriesAsync(string.Empty, It.IsAny<IEnumerable<Series>>()))
                .ReturnsAsync((LlmMatchResult)null);

            var result = await _mockLlmService.Object.TryMatchSeriesAsync(string.Empty, _testSeries);

            result.Should().BeNull();
        }

        [Test]
        public async Task TryMatchSeriesAsync_should_return_match_with_high_confidence()
        {
            var expectedSeries = _testSeries.First(s => s.TvdbId == 81189);
            var expectedResult = new LlmMatchResult
            {
                Series = expectedSeries,
                Confidence = 0.95,
                Reasoning = "Direct title match after removing dots and quality tags"
            };

            _mockLlmService.Setup(s => s.IsEnabled).Returns(true);
            _mockLlmService
                .Setup(s => s.TryMatchSeriesAsync("Breaking.Bad.S01E01.720p.WEB-DL", _testSeries))
                .ReturnsAsync(expectedResult);

            var result = await _mockLlmService.Object.TryMatchSeriesAsync("Breaking.Bad.S01E01.720p.WEB-DL", _testSeries);

            result.Should().NotBeNull();
            result.Series.Should().Be(expectedSeries);
            result.Confidence.Should().Be(0.95);
            result.IsSuccessfulMatch.Should().BeTrue();
        }

        [Test]
        public async Task TryMatchSeriesAsync_should_return_unsuccessful_match_with_low_confidence()
        {
            var expectedSeries = _testSeries.First(s => s.TvdbId == 81189);
            var expectedResult = new LlmMatchResult
            {
                Series = expectedSeries,
                Confidence = 0.45,
                Reasoning = "Possible match but title is ambiguous"
            };

            _mockLlmService.Setup(s => s.IsEnabled).Returns(true);
            _mockLlmService
                .Setup(s => s.TryMatchSeriesAsync("Bad.Show.S01E01", _testSeries))
                .ReturnsAsync(expectedResult);

            var result = await _mockLlmService.Object.TryMatchSeriesAsync("Bad.Show.S01E01", _testSeries);

            result.Should().NotBeNull();
            result.Confidence.Should().Be(0.45);
            result.IsSuccessfulMatch.Should().BeFalse();
        }

        [Test]
        public async Task TryMatchSeriesAsync_should_handle_anime_alternate_titles()
        {
            var expectedSeries = _testSeries.First(s => s.TvdbId == 267440);
            var expectedResult = new LlmMatchResult
            {
                Series = expectedSeries,
                Confidence = 0.92,
                Reasoning = "Shingeki no Kyojin is the Japanese title for Attack on Titan"
            };

            _mockLlmService.Setup(s => s.IsEnabled).Returns(true);
            _mockLlmService
                .Setup(s => s.TryMatchSeriesAsync("Shingeki.no.Kyojin.S04E01.1080p", _testSeries))
                .ReturnsAsync(expectedResult);

            var result = await _mockLlmService.Object.TryMatchSeriesAsync("Shingeki.no.Kyojin.S04E01.1080p", _testSeries);

            result.Should().NotBeNull();
            result.Series.TvdbId.Should().Be(267440);
            result.Series.Title.Should().Be("Attack on Titan");
            result.IsSuccessfulMatch.Should().BeTrue();
        }

        [Test]
        public async Task TryMatchSeriesAsync_should_include_alternatives_when_uncertain()
        {
            var primarySeries = _testSeries.First(s => s.TvdbId == 81189);
            var alternativeSeries = _testSeries.First(s => s.TvdbId == 153021);

            var expectedResult = new LlmMatchResult
            {
                Series = primarySeries,
                Confidence = 0.55,
                Reasoning = "Could be Breaking Bad but title is unclear",
                Alternatives = new List<AlternativeMatch>
                {
                    new AlternativeMatch
                    {
                        Series = alternativeSeries,
                        Confidence = 0.35,
                        Reasoning = "Walking Dead also possible"
                    }
                }
            };

            _mockLlmService.Setup(s => s.IsEnabled).Returns(true);
            _mockLlmService
                .Setup(s => s.TryMatchSeriesAsync("The.Bad.Dead.S01E01", _testSeries))
                .ReturnsAsync(expectedResult);

            var result = await _mockLlmService.Object.TryMatchSeriesAsync("The.Bad.Dead.S01E01", _testSeries);

            result.Should().NotBeNull();
            result.IsSuccessfulMatch.Should().BeFalse();
            result.Alternatives.Should().HaveCount(1);
            result.Alternatives.First().Series.TvdbId.Should().Be(153021);
        }

        [Test]
        public async Task TryMatchSeriesAsync_with_ParsedEpisodeInfo_should_return_null_when_disabled()
        {
            var parsedInfo = new ParsedEpisodeInfo
            {
                SeriesTitle = "Breaking Bad",
                ReleaseTitle = "Breaking.Bad.S01E01.720p.WEB-DL",
                SeasonNumber = 1,
                EpisodeNumbers = new[] { 1 }
            };

            _mockLlmService.Setup(s => s.IsEnabled).Returns(false);
            _mockLlmService
                .Setup(s => s.TryMatchSeriesAsync(It.IsAny<ParsedEpisodeInfo>(), It.IsAny<IEnumerable<Series>>()))
                .ReturnsAsync((LlmMatchResult)null);

            var result = await _mockLlmService.Object.TryMatchSeriesAsync(parsedInfo, _testSeries);

            result.Should().BeNull();
        }

        [Test]
        public async Task TryMatchSeriesAsync_with_ParsedEpisodeInfo_should_use_parsed_metadata()
        {
            // Note: IsAbsoluteNumbering and IsDaily are computed properties, we don't set them directly
            var parsedInfo = new ParsedEpisodeInfo
            {
                SeriesTitle = "Breaking Bad German",
                ReleaseTitle = "Breaking.Bad.German.S01E01.720p.WEB-DL",
                SeasonNumber = 1,
                EpisodeNumbers = new[] { 1 }
            };

            var expectedSeries = _testSeries.First(s => s.TvdbId == 81189);
            var expectedResult = new LlmMatchResult
            {
                Series = expectedSeries,
                Confidence = 0.88,
                Reasoning = "Recognized 'German' as language tag, matched to Breaking Bad"
            };

            _mockLlmService.Setup(s => s.IsEnabled).Returns(true);
            _mockLlmService
                .Setup(s => s.TryMatchSeriesAsync(parsedInfo, _testSeries))
                .ReturnsAsync(expectedResult);

            var result = await _mockLlmService.Object.TryMatchSeriesAsync(parsedInfo, _testSeries);

            result.Should().NotBeNull();
            result.Series.TvdbId.Should().Be(81189);
            result.IsSuccessfulMatch.Should().BeTrue();
        }

        [Test]
        public async Task TryMatchSeriesAsync_with_anime_ParsedEpisodeInfo_should_consider_series_type()
        {
            // Note: IsAbsoluteNumbering is computed from AbsoluteEpisodeNumbers
            var parsedInfo = new ParsedEpisodeInfo
            {
                SeriesTitle = "Shingeki no Kyojin",
                ReleaseTitle = "[SubGroup] Shingeki no Kyojin - 01 [1080p]",
                SeasonNumber = 1,
                AbsoluteEpisodeNumbers = new[] { 1 }
            };

            var expectedSeries = _testSeries.First(s => s.TvdbId == 267440);
            var expectedResult = new LlmMatchResult
            {
                Series = expectedSeries,
                Confidence = 0.94,
                Reasoning = "Japanese anime title matched to Attack on Titan"
            };

            _mockLlmService.Setup(s => s.IsEnabled).Returns(true);
            _mockLlmService
                .Setup(s => s.TryMatchSeriesAsync(parsedInfo, _testSeries))
                .ReturnsAsync(expectedResult);

            var result = await _mockLlmService.Object.TryMatchSeriesAsync(parsedInfo, _testSeries);

            result.Should().NotBeNull();
            result.Series.SeriesType.Should().Be(SeriesTypes.Anime);
            result.IsSuccessfulMatch.Should().BeTrue();
        }

        [Test]
        public void LlmMatchResult_IsSuccessfulMatch_should_return_true_when_confidence_at_threshold()
        {
            var result = new LlmMatchResult
            {
                Series = _testSeries.First(),
                Confidence = 0.7
            };

            result.IsSuccessfulMatch.Should().BeTrue();
        }

        [Test]
        public void LlmMatchResult_IsSuccessfulMatch_should_return_true_when_confidence_above_threshold()
        {
            var result = new LlmMatchResult
            {
                Series = _testSeries.First(),
                Confidence = 0.95
            };

            result.IsSuccessfulMatch.Should().BeTrue();
        }

        [Test]
        public void LlmMatchResult_IsSuccessfulMatch_should_return_false_when_confidence_below_threshold()
        {
            var result = new LlmMatchResult
            {
                Series = _testSeries.First(),
                Confidence = 0.69
            };

            result.IsSuccessfulMatch.Should().BeFalse();
        }

        [Test]
        public void LlmMatchResult_IsSuccessfulMatch_should_return_false_when_series_is_null()
        {
            var result = new LlmMatchResult
            {
                Series = null,
                Confidence = 0.95
            };

            result.IsSuccessfulMatch.Should().BeFalse();
        }

        [Test]
        public void LlmMatchResult_should_initialize_alternatives_as_empty_list()
        {
            var result = new LlmMatchResult();

            result.Alternatives.Should().NotBeNull();
            result.Alternatives.Should().BeEmpty();
        }
    }

    [TestFixture]
    public class CachedLlmSeriesMatchingServiceFixture
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

            _innerService = new Mock<OpenAiSeriesMatchingService>(
                _configService.Object,
                _httpClientFactory.Object,
                Mock.Of<Logger>());

            _configService.Setup(s => s.LlmCacheEnabled).Returns(true);
            _configService.Setup(s => s.LlmCacheDurationHours).Returns(24);

            _subject = new CachedLlmSeriesMatchingService(
                _innerService.Object,
                _configService.Object,
                Mock.Of<Logger>());

            _testSeries = new List<Series>
            {
                new Series { Id = 1, TvdbId = 81189, Title = "Breaking Bad" },
                new Series { Id = 2, TvdbId = 121361, Title = "Game of Thrones" }
            };
        }

        [Test]
        public void IsEnabled_should_delegate_to_inner_service()
        {
            _innerService.Setup(s => s.IsEnabled).Returns(true);

            _subject.IsEnabled.Should().BeTrue();
        }

        [Test]
        public async Task TryMatchSeriesAsync_should_bypass_cache_when_caching_disabled()
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

            // Call twice
            await _subject.TryMatchSeriesAsync("Breaking.Bad.S01E01", _testSeries);
            await _subject.TryMatchSeriesAsync("Breaking.Bad.S01E01", _testSeries);

            // Inner service should be called twice (no caching)
            _innerService.Verify(
                s => s.TryMatchSeriesAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Series>>()),
                Times.Exactly(2));
        }

        [Test]
        public async Task TryMatchSeriesAsync_should_cache_results_when_enabled()
        {
            var expectedResult = new LlmMatchResult
            {
                Series = _testSeries.First(),
                Confidence = 0.9,
                Reasoning = "Test match"
            };

            _innerService
                .Setup(s => s.TryMatchSeriesAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Series>>()))
                .ReturnsAsync(expectedResult);

            // Call twice with same title
            var result1 = await _subject.TryMatchSeriesAsync("Breaking.Bad.S01E01", _testSeries);
            var result2 = await _subject.TryMatchSeriesAsync("Breaking.Bad.S01E01", _testSeries);

            // Inner service should only be called once (second call uses cache)
            _innerService.Verify(
                s => s.TryMatchSeriesAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Series>>()),
                Times.Once);

            result1.Should().NotBeNull();
            result2.Should().NotBeNull();
        }

        [Test]
        public async Task TryMatchSeriesAsync_should_call_inner_service_for_different_titles()
        {
            var result1 = new LlmMatchResult
            {
                Series = _testSeries[0],
                Confidence = 0.9
            };

            var result2 = new LlmMatchResult
            {
                Series = _testSeries[1],
                Confidence = 0.85
            };

            _innerService
                .SetupSequence(s => s.TryMatchSeriesAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Series>>()))
                .ReturnsAsync(result1)
                .ReturnsAsync(result2);

            await _subject.TryMatchSeriesAsync("Breaking.Bad.S01E01", _testSeries);
            await _subject.TryMatchSeriesAsync("Game.of.Thrones.S01E01", _testSeries);

            // Inner service should be called twice (different titles)
            _innerService.Verify(
                s => s.TryMatchSeriesAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Series>>()),
                Times.Exactly(2));
        }
    }

    [TestFixture]
    public class RateLimitedLlmSeriesMatchingServiceFixture
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

            var openAiService = new Mock<OpenAiSeriesMatchingService>(
                _configService.Object,
                _httpClientFactory.Object,
                Mock.Of<Logger>());

            _innerService = new Mock<CachedLlmSeriesMatchingService>(
                openAiService.Object,
                _configService.Object,
                Mock.Of<Logger>());

            _configService.Setup(s => s.LlmMaxCallsPerHour).Returns(60);

            _subject = new RateLimitedLlmSeriesMatchingService(
                _innerService.Object,
                _configService.Object,
                Mock.Of<Logger>());

            _testSeries = new List<Series>
            {
                new Series { Id = 1, TvdbId = 81189, Title = "Breaking Bad" }
            };
        }

        [Test]
        public void IsEnabled_should_delegate_to_inner_service()
        {
            _innerService.Setup(s => s.IsEnabled).Returns(true);

            _subject.IsEnabled.Should().BeTrue();
        }

        [Test]
        public async Task TryMatchSeriesAsync_should_allow_calls_within_rate_limit()
        {
            _configService.Setup(s => s.LlmMaxCallsPerHour).Returns(5);

            var expectedResult = new LlmMatchResult
            {
                Series = _testSeries.First(),
                Confidence = 0.9
            };

            _innerService
                .Setup(s => s.TryMatchSeriesAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Series>>()))
                .ReturnsAsync(expectedResult);

            // Make 5 calls (within limit)
            for (var i = 0; i < 5; i++)
            {
                var result = await _subject.TryMatchSeriesAsync($"Title{i}.S01E01", _testSeries);
                result.Should().NotBeNull();
            }

            _innerService.Verify(
                s => s.TryMatchSeriesAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Series>>()),
                Times.Exactly(5));
        }

        [Test]
        public async Task TryMatchSeriesAsync_should_return_null_when_rate_limit_exceeded()
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

            // Make 2 calls (at limit)
            await _subject.TryMatchSeriesAsync("Title1.S01E01", _testSeries);
            await _subject.TryMatchSeriesAsync("Title2.S01E01", _testSeries);

            // Third call should be rate limited
            var result = await _subject.TryMatchSeriesAsync("Title3.S01E01", _testSeries);

            result.Should().BeNull();

            // Inner service should only be called twice
            _innerService.Verify(
                s => s.TryMatchSeriesAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Series>>()),
                Times.Exactly(2));
        }

        [Test]
        public async Task TryMatchSeriesAsync_with_ParsedEpisodeInfo_should_respect_rate_limit()
        {
            _configService.Setup(s => s.LlmMaxCallsPerHour).Returns(1);

            var parsedInfo = new ParsedEpisodeInfo
            {
                SeriesTitle = "Breaking Bad",
                ReleaseTitle = "Breaking.Bad.S01E01"
            };

            var expectedResult = new LlmMatchResult
            {
                Series = _testSeries.First(),
                Confidence = 0.9
            };

            _innerService
                .Setup(s => s.TryMatchSeriesAsync(It.IsAny<ParsedEpisodeInfo>(), It.IsAny<IEnumerable<Series>>()))
                .ReturnsAsync(expectedResult);

            // First call should succeed
            var result1 = await _subject.TryMatchSeriesAsync(parsedInfo, _testSeries);
            result1.Should().NotBeNull();

            // Second call should be rate limited
            var result2 = await _subject.TryMatchSeriesAsync(parsedInfo, _testSeries);
            result2.Should().BeNull();
        }
    }

    [TestFixture]
    public class ParsingServiceLlmIntegrationFixture
    {
        private Mock<ILlmSeriesMatchingService> _llmService;
        private List<Series> _testSeries;

        [SetUp]
        public void Setup()
        {
            _llmService = new Mock<ILlmSeriesMatchingService>();

            _testSeries = new List<Series>
            {
                new Series
                {
                    Id = 1,
                    TvdbId = 81189,
                    Title = "Breaking Bad",
                    CleanTitle = "breakingbad",
                    Year = 2008
                },
                new Series
                {
                    Id = 2,
                    TvdbId = 267440,
                    Title = "Attack on Titan",
                    CleanTitle = "attackontitan",
                    Year = 2013,
                    SeriesType = SeriesTypes.Anime
                }
            };
        }

        [Test]
        public void LlmService_should_not_be_called_when_traditional_matching_succeeds()
        {
            // This tests that LLM is only used as fallback
            _llmService.Setup(s => s.IsEnabled).Returns(true);

            // LLM should never be called if traditional matching works
            _llmService.Verify(
                s => s.TryMatchSeriesAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Series>>()),
                Times.Never);

            _llmService.Verify(
                s => s.TryMatchSeriesAsync(It.IsAny<ParsedEpisodeInfo>(), It.IsAny<IEnumerable<Series>>()),
                Times.Never);
        }

        [Test]
        public async Task LlmService_should_be_called_when_traditional_matching_fails()
        {
            _llmService.Setup(s => s.IsEnabled).Returns(true);

            var expectedResult = new LlmMatchResult
            {
                Series = _testSeries.First(),
                Confidence = 0.88,
                Reasoning = "Matched after removing language tag"
            };

            _llmService
                .Setup(s => s.TryMatchSeriesAsync(It.IsAny<ParsedEpisodeInfo>(), It.IsAny<IEnumerable<Series>>()))
                .ReturnsAsync(expectedResult);

            // Simulate call that would happen from ParsingService
            var parsedInfo = new ParsedEpisodeInfo
            {
                SeriesTitle = "Breaking Bad German",
                ReleaseTitle = "Breaking.Bad.German.S01E01.720p"
            };

            var result = await _llmService.Object.TryMatchSeriesAsync(parsedInfo, _testSeries);

            result.Should().NotBeNull();
            result.IsSuccessfulMatch.Should().BeTrue();
            result.Series.Title.Should().Be("Breaking Bad");
        }

        [Test]
        public async Task LlmService_should_handle_anime_alternate_titles()
        {
            _llmService.Setup(s => s.IsEnabled).Returns(true);

            var expectedResult = new LlmMatchResult
            {
                Series = _testSeries.First(s => s.TvdbId == 267440),
                Confidence = 0.92,
                Reasoning = "Shingeki no Kyojin is the Japanese title for Attack on Titan"
            };

            _llmService
                .Setup(s => s.TryMatchSeriesAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Series>>()))
                .ReturnsAsync(expectedResult);

            var result = await _llmService.Object.TryMatchSeriesAsync(
                "[SubGroup] Shingeki no Kyojin - 01 [1080p].mkv",
                _testSeries);

            result.Should().NotBeNull();
            result.Series.Title.Should().Be("Attack on Titan");
            result.IsSuccessfulMatch.Should().BeTrue();
        }

        [Test]
        public async Task LlmService_should_return_null_when_disabled()
        {
            _llmService.Setup(s => s.IsEnabled).Returns(false);
            _llmService
                .Setup(s => s.TryMatchSeriesAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Series>>()))
                .ReturnsAsync((LlmMatchResult)null);

            var result = await _llmService.Object.TryMatchSeriesAsync("Some.Title.S01E01", _testSeries);

            result.Should().BeNull();
        }

        [Test]
        public async Task LlmService_low_confidence_should_not_auto_match()
        {
            _llmService.Setup(s => s.IsEnabled).Returns(true);

            var expectedResult = new LlmMatchResult
            {
                Series = _testSeries.First(),
                Confidence = 0.45,
                Reasoning = "Title is ambiguous, multiple possible matches",
                Alternatives = new List<AlternativeMatch>
                {
                    new AlternativeMatch
                    {
                        Series = _testSeries[1],
                        Confidence = 0.30,
                        Reasoning = "Could also be this series"
                    }
                }
            };

            _llmService
                .Setup(s => s.TryMatchSeriesAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Series>>()))
                .ReturnsAsync(expectedResult);

            var result = await _llmService.Object.TryMatchSeriesAsync("Ambiguous.Title.S01E01", _testSeries);

            result.Should().NotBeNull();
            result.IsSuccessfulMatch.Should().BeFalse();
            result.Alternatives.Should().HaveCount(1);
        }
    }
}
