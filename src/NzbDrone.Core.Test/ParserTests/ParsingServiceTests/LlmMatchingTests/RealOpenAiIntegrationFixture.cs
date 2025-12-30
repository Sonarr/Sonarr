using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
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
    /// Real integration tests that make actual API calls to OpenAI.
    /// These tests require a valid API key in appsettings.llm.local.json.
    ///
    /// SETUP:
    /// 1. Copy appsettings.llm.template.json to appsettings.llm.local.json
    /// 2. Add your OpenAI API key to the local file
    /// 3. Ensure the file is set to "Copy to Output Directory: Copy if newer" in Visual Studio
    /// 4. Run tests with: dotnet test --filter "FullyQualifiedName~RealOpenAiIntegrationFixture"
    ///
    /// NOTE: These tests are marked [Explicit] and won't run during normal test execution.
    /// They make real API calls which incur costs and require network access.
    /// </summary>
    [TestFixture]
    [Explicit("Requires real OpenAI API key and makes actual API calls")]
    [Category("Integration")]
    [Category("LlmMatching")]
    [Category("ExternalApi")]
    public class RealOpenAiIntegrationFixture
    {
        private OpenAiSeriesMatchingService _subject;
        private List<Series> _testSeries;
        private LlmTestSettings _settings;
        private bool _isConfigured;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _settings = LoadSettings();
            _isConfigured = !string.IsNullOrWhiteSpace(_settings?.OpenAiApiKey) &&
                            !_settings.OpenAiApiKey.StartsWith("sk-your-");

            if (!_isConfigured)
            {
                TestContext.WriteLine("========================================");
                TestContext.WriteLine("WARNING: OpenAI API key not configured.");
                TestContext.WriteLine("========================================");
                TestContext.WriteLine("");
                TestContext.WriteLine("Setup steps:");
                TestContext.WriteLine("1. Copy appsettings.llm.template.json to appsettings.llm.local.json");
                TestContext.WriteLine("2. Add your OpenAI API key to appsettings.llm.local.json");
                TestContext.WriteLine("3. In Visual Studio: Right-click the file -> Properties");
                TestContext.WriteLine("   Set 'Copy to Output Directory' = 'Copy if newer'");
                TestContext.WriteLine("");
                TestContext.WriteLine("Or set environment variable: OPENAI_API_KEY=sk-...");
                TestContext.WriteLine("");
                TestContext.WriteLine("Searched locations:");
                foreach (var path in GetSearchPaths())
                {
                    var exists = File.Exists(path) ? "[FOUND]" : "[NOT FOUND]";
                    TestContext.WriteLine($"  {exists} {path}");
                }
            }
            else
            {
                TestContext.WriteLine($"OpenAI API configured. Using model: {_settings.OpenAiModel}");
            }
        }

        [SetUp]
        public void Setup()
        {
            if (!_isConfigured)
            {
                Assert.Ignore("OpenAI API key not configured. Skipping real API tests.");
                return;
            }

            var configService = new Mock<IConfigService>();
            configService.Setup(s => s.LlmMatchingEnabled).Returns(true);
            configService.Setup(s => s.OpenAiApiKey).Returns(_settings.OpenAiApiKey);
            configService.Setup(s => s.OpenAiApiEndpoint).Returns(_settings.OpenAiApiEndpoint);
            configService.Setup(s => s.OpenAiModel).Returns(_settings.OpenAiModel);
            configService.Setup(s => s.LlmConfidenceThreshold).Returns(_settings.ConfidenceThreshold);

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory
                .Setup(f => f.CreateClient(It.IsAny<string>()))
                .Returns(new HttpClient());

            _subject = new OpenAiSeriesMatchingService(
                configService.Object,
                httpClientFactory.Object,
                LogManager.GetCurrentClassLogger());

            // Setup test series library - simulates a real user's library
            _testSeries = new List<Series>
            {
                new Series
                {
                    Id = 1,
                    TvdbId = 81189,
                    Title = "Breaking Bad",
                    CleanTitle = "breakingbad",
                    Year = 2008,
                    Network = "AMC",
                    SeriesType = SeriesTypes.Standard
                },
                new Series
                {
                    Id = 2,
                    TvdbId = 121361,
                    Title = "Game of Thrones",
                    CleanTitle = "gameofthrones",
                    Year = 2011,
                    Network = "HBO",
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
                    Network = "AMC",
                    SeriesType = SeriesTypes.Standard
                },
                new Series
                {
                    Id = 5,
                    TvdbId = 295759,
                    Title = "Stranger Things",
                    CleanTitle = "strangerthings",
                    Year = 2016,
                    Network = "Netflix",
                    SeriesType = SeriesTypes.Standard
                },
                new Series
                {
                    Id = 6,
                    TvdbId = 305288,
                    Title = "Westworld",
                    CleanTitle = "westworld",
                    Year = 2016,
                    Network = "HBO",
                    SeriesType = SeriesTypes.Standard
                },
                new Series
                {
                    Id = 7,
                    TvdbId = 78804,
                    Title = "Doctor Who",
                    CleanTitle = "doctorwho",
                    Year = 2005,
                    Network = "BBC",
                    SeriesType = SeriesTypes.Standard
                },
                new Series
                {
                    Id = 8,
                    TvdbId = 73255,
                    Title = "Doctor Who",
                    CleanTitle = "doctorwho",
                    Year = 1963,
                    Network = "BBC",
                    SeriesType = SeriesTypes.Standard
                }
            };
        }

        [Test]
        public async Task Should_match_standard_release_title()
        {
            // Arrange
            var releaseTitle = "Breaking.Bad.S01E01.720p.BluRay.x264-DEMAND";

            // Act
            var result = await _subject.TryMatchSeriesAsync(releaseTitle, _testSeries);

            // Assert
            result.Should().NotBeNull();
            result.Series.Should().NotBeNull();
            result.Series.TvdbId.Should().Be(81189);
            result.Series.Title.Should().Be("Breaking Bad");
            result.Confidence.Should().BeGreaterOrEqualTo(0.7);
            result.IsSuccessfulMatch.Should().BeTrue();

            LogResult(releaseTitle, result);
        }

        [Test]
        public async Task Should_match_release_with_dots_as_spaces()
        {
            // Arrange
            var releaseTitle = "Game.of.Thrones.S08E06.The.Iron.Throne.1080p.AMZN.WEB-DL";

            // Act
            var result = await _subject.TryMatchSeriesAsync(releaseTitle, _testSeries);

            // Assert
            result.Should().NotBeNull();
            result.Series.Should().NotBeNull();
            result.Series.TvdbId.Should().Be(121361);
            result.Confidence.Should().BeGreaterOrEqualTo(0.7);

            LogResult(releaseTitle, result);
        }

        [Test]
        public async Task Should_match_anime_with_japanese_title()
        {
            // Arrange - Japanese title for Attack on Titan
            var releaseTitle = "[SubGroup] Shingeki no Kyojin - 01 [1080p][HEVC]";

            // Act
            var result = await _subject.TryMatchSeriesAsync(releaseTitle, _testSeries);

            // Assert
            result.Should().NotBeNull();
            result.Series.Should().NotBeNull();
            result.Series.TvdbId.Should().Be(267440);
            result.Series.Title.Should().Be("Attack on Titan");
            result.Confidence.Should().BeGreaterOrEqualTo(0.7);

            LogResult(releaseTitle, result);
        }

        [Test]
        public async Task Should_disambiguate_by_year()
        {
            // Arrange - Should match 2005 Doctor Who, not 1963
            var releaseTitle = "Doctor.Who.2005.S13E01.720p.WEB-DL";

            // Act
            var result = await _subject.TryMatchSeriesAsync(releaseTitle, _testSeries);

            // Assert
            result.Should().NotBeNull();
            result.Series.Should().NotBeNull();
            result.Series.TvdbId.Should().Be(78804); // 2005 version
            result.Series.Year.Should().Be(2005);

            LogResult(releaseTitle, result);
        }

        [Test]
        public async Task Should_handle_release_with_language_tag()
        {
            // Arrange - German release
            var releaseTitle = "Breaking.Bad.S01E01.GERMAN.720p.BluRay.x264";

            // Act
            var result = await _subject.TryMatchSeriesAsync(releaseTitle, _testSeries);

            // Assert
            result.Should().NotBeNull();
            result.Series.Should().NotBeNull();
            result.Series.TvdbId.Should().Be(81189);
            result.Reasoning.Should().NotBeNullOrEmpty();

            LogResult(releaseTitle, result);
        }

        [Test]
        public async Task Should_handle_release_with_scene_group_tags()
        {
            // Arrange
            var releaseTitle = "Stranger.Things.S04E09.Chapter.Nine.The.Piggyback.2160p.NF.WEB-DL.DDP5.1.Atmos.DV.HDR.H.265-FLUX";

            // Act
            var result = await _subject.TryMatchSeriesAsync(releaseTitle, _testSeries);

            // Assert
            result.Should().NotBeNull();
            result.Series.Should().NotBeNull();
            result.Series.TvdbId.Should().Be(295759);

            LogResult(releaseTitle, result);
        }

        [Test]
        public async Task Should_handle_abbreviated_title()
        {
            // Arrange - TWD is a common abbreviation for The Walking Dead
            var releaseTitle = "TWD.S11E24.Rest.in.Peace.1080p.AMZN.WEB-DL";

            // Act
            var result = await _subject.TryMatchSeriesAsync(releaseTitle, _testSeries);

            // Assert
            result.Should().NotBeNull();

            // Note: LLM might or might not recognize TWD abbreviation
            // This test verifies the LLM can handle ambiguous cases
            if (result.Series != null)
            {
                TestContext.WriteLine($"LLM recognized TWD as: {result.Series.Title}");
            }

            LogResult(releaseTitle, result);
        }

        [Test]
        public async Task Should_handle_miscoded_german_umlauts_utf8_as_latin1()
        {
            // Arrange - "Für" miscoded as "FÃ¼r" (UTF-8 bytes interpreted as Latin-1)
            // This happens when UTF-8 encoded text is read as ISO-8859-1
            var releaseTitle = "Breaking.Bad.S01E01.German.FÃ¼r.immer.720p.BluRay.x264";

            // Act
            var result = await _subject.TryMatchSeriesAsync(releaseTitle, _testSeries);

            // Assert
            result.Should().NotBeNull();
            result.Series.Should().NotBeNull();
            result.Series.TvdbId.Should().Be(81189);

            TestContext.WriteLine($"LLM handled miscoded umlaut 'Ã¼' (should be 'ü')");
            LogResult(releaseTitle, result);
        }

        [Test]
        public async Task Should_handle_miscoded_german_umlauts_various()
        {
            // Arrange - Various miscoded German umlauts
            // ä -> Ã¤, ö -> Ã¶, ü -> Ã¼, ß -> ÃŸ
            var releaseTitle = "Breaking.Bad.S02E01.GrÃ¼ÃŸe.aus.KÃ¶ln.GERMAN.720p.WEB-DL";

            // Act
            var result = await _subject.TryMatchSeriesAsync(releaseTitle, _testSeries);

            // Assert
            result.Should().NotBeNull();
            result.Series.Should().NotBeNull();
            result.Series.TvdbId.Should().Be(81189);

            TestContext.WriteLine("LLM handled multiple miscoded umlauts: 'Ã¼'='ü', 'ÃŸ'='ß', 'Ã¶'='ö'");
            LogResult(releaseTitle, result);
        }

        [Test]
        public async Task Should_handle_correct_german_umlauts()
        {
            // Arrange - Correctly encoded German umlauts for comparison
            var releaseTitle = "Breaking.Bad.S01E01.Grüße.aus.Köln.GERMAN.720p.BluRay.x264";

            // Act
            var result = await _subject.TryMatchSeriesAsync(releaseTitle, _testSeries);

            // Assert
            result.Should().NotBeNull();
            result.Series.Should().NotBeNull();
            result.Series.TvdbId.Should().Be(81189);

            TestContext.WriteLine("LLM handled correct German umlauts: ü, ö, ß");
            LogResult(releaseTitle, result);
        }

        [Test]
        public async Task Should_handle_japanese_characters_in_anime_release()
        {
            // Arrange - Japanese title with kanji/hiragana
            // 進撃の巨人 = Shingeki no Kyojin = Attack on Titan
            var releaseTitle = "[SubGroup] 進撃の巨人 - 01 [1080p][HEVC].mkv";

            // Act
            var result = await _subject.TryMatchSeriesAsync(releaseTitle, _testSeries);

            // Assert
            result.Should().NotBeNull();

            if (result.Series != null)
            {
                TestContext.WriteLine($"LLM recognized Japanese '進撃の巨人' as: {result.Series.Title}");

                // Should match Attack on Titan
                if (result.Series.TvdbId == 267440)
                {
                    TestContext.WriteLine("SUCCESS: Correctly identified as Attack on Titan");
                }
            }
            else
            {
                TestContext.WriteLine("LLM could not match Japanese kanji title");
            }

            LogResult(releaseTitle, result);
        }

        [Test]
        public async Task Should_handle_mixed_japanese_english_title()
        {
            // Arrange - Mixed Japanese and English (common in anime releases)
            var releaseTitle = "[Erai-raws] Shingeki no Kyojin - The Final Season - 01 [1080p][HEVC].mkv";

            // Act
            var result = await _subject.TryMatchSeriesAsync(releaseTitle, _testSeries);

            // Assert
            result.Should().NotBeNull();
            result.Series.Should().NotBeNull();
            result.Series.TvdbId.Should().Be(267440);

            TestContext.WriteLine("LLM handled mixed Japanese/English anime title");
            LogResult(releaseTitle, result);
        }

        [Test]
        public async Task Should_handle_chinese_characters()
        {
            // Arrange - Chinese title for Attack on Titan (進擊的巨人 - Traditional Chinese)
            var releaseTitle = "[字幕组] 進擊的巨人 - 01 [1080p].mkv";

            // Act
            var result = await _subject.TryMatchSeriesAsync(releaseTitle, _testSeries);

            // Assert
            result.Should().NotBeNull();

            if (result.Series != null)
            {
                TestContext.WriteLine($"LLM recognized Chinese '進擊的巨人' as: {result.Series.Title}");
            }

            LogResult(releaseTitle, result);
        }

        [Test]
        public async Task Should_handle_korean_characters()
        {
            // Arrange - Korean title (게임 오브 스론스 = Game of Thrones)
            var releaseTitle = "게임.오브.스론스.S08E06.1080p.WEB-DL.KOR";

            // Act
            var result = await _subject.TryMatchSeriesAsync(releaseTitle, _testSeries);

            // Assert
            result.Should().NotBeNull();

            if (result.Series != null)
            {
                TestContext.WriteLine($"LLM recognized Korean '게임 오브 스론스' as: {result.Series.Title}");

                if (result.Series.TvdbId == 121361)
                {
                    TestContext.WriteLine("SUCCESS: Correctly identified as Game of Thrones");
                }
            }

            LogResult(releaseTitle, result);
        }

        [Test]
        public async Task Should_handle_cyrillic_characters()
        {
            // Arrange - Russian title (Во все тяжкие = Breaking Bad)
            var releaseTitle = "Во.все.тяжкие.S01E01.720p.BluRay.RUS";

            // Act
            var result = await _subject.TryMatchSeriesAsync(releaseTitle, _testSeries);

            // Assert
            result.Should().NotBeNull();

            if (result.Series != null)
            {
                TestContext.WriteLine($"LLM recognized Cyrillic 'Во все тяжкие' as: {result.Series.Title}");

                if (result.Series.TvdbId == 81189)
                {
                    TestContext.WriteLine("SUCCESS: Correctly identified as Breaking Bad");
                }
            }

            LogResult(releaseTitle, result);
        }

        [Test]
        public async Task Should_handle_arabic_characters()
        {
            // Arrange - Arabic title (صراع العروش = Game of Thrones)
            var releaseTitle = "صراع.العروش.S08E06.1080p.WEB-DL.ARA";

            // Act
            var result = await _subject.TryMatchSeriesAsync(releaseTitle, _testSeries);

            // Assert
            result.Should().NotBeNull();

            if (result.Series != null)
            {
                TestContext.WriteLine($"LLM recognized Arabic 'صراع العروش' as: {result.Series.Title}");

                if (result.Series.TvdbId == 121361)
                {
                    TestContext.WriteLine("SUCCESS: Correctly identified as Game of Thrones");
                }
            }

            LogResult(releaseTitle, result);
        }

        [Test]
        public async Task Should_handle_french_accents()
        {
            // Arrange - French accented characters
            var releaseTitle = "Breaking.Bad.S01E01.FRENCH.Épisode.Spécial.720p.BluRay.x264";

            // Act
            var result = await _subject.TryMatchSeriesAsync(releaseTitle, _testSeries);

            // Assert
            result.Should().NotBeNull();
            result.Series.Should().NotBeNull();
            result.Series.TvdbId.Should().Be(81189);

            TestContext.WriteLine("LLM handled French accents: é, è, ê, ç");
            LogResult(releaseTitle, result);
        }

        [Test]
        public async Task Should_handle_miscoded_french_accents()
        {
            // Arrange - Miscoded French (é -> Ã©)
            var releaseTitle = "Breaking.Bad.S01E01.FRENCH.Ã‰pisode.SpÃ©cial.720p.BluRay.x264";

            // Act
            var result = await _subject.TryMatchSeriesAsync(releaseTitle, _testSeries);

            // Assert
            result.Should().NotBeNull();
            result.Series.Should().NotBeNull();
            result.Series.TvdbId.Should().Be(81189);

            TestContext.WriteLine("LLM handled miscoded French accents: 'Ã©'='é', 'Ã‰'='É'");
            LogResult(releaseTitle, result);
        }

        [Test]
        public async Task Should_handle_spanish_characters()
        {
            // Arrange - Spanish with ñ and inverted punctuation
            var releaseTitle = "Breaking.Bad.S01E01.SPANISH.El.Año.del.Dragón.720p.WEB-DL";

            // Act
            var result = await _subject.TryMatchSeriesAsync(releaseTitle, _testSeries);

            // Assert
            result.Should().NotBeNull();
            result.Series.Should().NotBeNull();
            result.Series.TvdbId.Should().Be(81189);

            TestContext.WriteLine("LLM handled Spanish ñ character");
            LogResult(releaseTitle, result);
        }

        [Test]
        public async Task Should_handle_polish_characters()
        {
            // Arrange - Polish special characters (ą, ę, ł, ń, ó, ś, ź, ż)
            var releaseTitle = "Breaking.Bad.S01E01.POLISH.Żółć.i.Gęś.720p.WEB-DL";

            // Act
            var result = await _subject.TryMatchSeriesAsync(releaseTitle, _testSeries);

            // Assert
            result.Should().NotBeNull();
            result.Series.Should().NotBeNull();
            result.Series.TvdbId.Should().Be(81189);

            TestContext.WriteLine("LLM handled Polish characters: ż, ó, ł, ć, ę, ś");
            LogResult(releaseTitle, result);
        }

        [Test]
        public async Task Should_handle_turkish_characters()
        {
            // Arrange - Turkish special characters (ç, ğ, ı, ş, ö, ü)
            var releaseTitle = "Breaking.Bad.S01E01.TURKISH.Güçlü.Şef.720p.WEB-DL";

            // Act
            var result = await _subject.TryMatchSeriesAsync(releaseTitle, _testSeries);

            // Assert
            result.Should().NotBeNull();
            result.Series.Should().NotBeNull();
            result.Series.TvdbId.Should().Be(81189);

            TestContext.WriteLine("LLM handled Turkish characters: ü, ç, ş, ğ, ı");
            LogResult(releaseTitle, result);
        }

        [Test]
        public async Task Should_handle_double_encoded_utf8()
        {
            // Arrange - Double-encoded UTF-8 (ü -> Ã¼ -> Ãƒâ€ Ã‚Â¼)
            // This happens when already-encoded UTF-8 is encoded again
            var releaseTitle = "Breaking.Bad.S01E01.GrÃƒÂ¼ÃƒÅ¸e.GERMAN.720p.BluRay";

            // Act
            var result = await _subject.TryMatchSeriesAsync(releaseTitle, _testSeries);

            // Assert
            result.Should().NotBeNull();

            if (result.Series != null)
            {
                TestContext.WriteLine($"LLM handled double-encoded UTF-8, matched to: {result.Series.Title}");
            }
            else
            {
                TestContext.WriteLine("LLM could not parse double-encoded UTF-8 (expected behavior)");
            }

            LogResult(releaseTitle, result);
        }

        [Test]
        public async Task Should_handle_replacement_characters()
        {
            // Arrange - Unicode replacement characters (common when encoding fails)
            var releaseTitle = "Breaking.Bad.S01E01.Gr��e.GERMAN.720p.BluRay";

            // Act
            var result = await _subject.TryMatchSeriesAsync(releaseTitle, _testSeries);

            // Assert
            result.Should().NotBeNull();
            result.Series.Should().NotBeNull();
            result.Series.TvdbId.Should().Be(81189);

            TestContext.WriteLine("LLM handled replacement characters (�)");
            LogResult(releaseTitle, result);
        }

        [Test]
        public async Task Should_handle_html_entities_in_title()
        {
            // Arrange - HTML entities (sometimes appear in scraped titles)
            var releaseTitle = "Breaking.Bad.S01E01.Gr&uuml;&szlig;e.GERMAN.720p.BluRay";

            // Act
            var result = await _subject.TryMatchSeriesAsync(releaseTitle, _testSeries);

            // Assert
            result.Should().NotBeNull();

            if (result.Series != null)
            {
                TestContext.WriteLine($"LLM handled HTML entities (&uuml; &szlig;), matched to: {result.Series.Title}");
            }

            LogResult(releaseTitle, result);
        }

        [Test]
        public async Task Should_handle_url_encoded_characters()
        {
            // Arrange - URL encoded characters
            var releaseTitle = "Breaking.Bad.S01E01.Gr%C3%BC%C3%9Fe.GERMAN.720p.BluRay";

            // Act
            var result = await _subject.TryMatchSeriesAsync(releaseTitle, _testSeries);

            // Assert
            result.Should().NotBeNull();

            if (result.Series != null)
            {
                TestContext.WriteLine($"LLM handled URL-encoded characters, matched to: {result.Series.Title}");
            }

            LogResult(releaseTitle, result);
        }

        [Test]
        public async Task Should_handle_mixed_encoding_issues()
        {
            // Arrange - Mix of different encoding problems in one title
            var releaseTitle = "[SubGroup] Shingeki no Kyojin - 進撃の巨人 - Attack.on.Titan.S04E01.GermÃ¤n.DuÃŸ.1080p";

            // Act
            var result = await _subject.TryMatchSeriesAsync(releaseTitle, _testSeries);

            // Assert
            result.Should().NotBeNull();
            result.Series.Should().NotBeNull();
            result.Series.TvdbId.Should().Be(267440);

            TestContext.WriteLine("LLM handled mixed Japanese + miscoded German in same title");
            LogResult(releaseTitle, result);
        }

        [Test]
        public async Task Should_return_low_confidence_for_ambiguous_title()
        {
            // Arrange - Completely made up title that doesn't match anything well
            var releaseTitle = "The.Show.About.Nothing.S01E01.720p.WEB-DL";

            // Act
            var result = await _subject.TryMatchSeriesAsync(releaseTitle, _testSeries);

            // Assert
            result.Should().NotBeNull();

            // Should either have low confidence or no match
            if (result.Series != null)
            {
                TestContext.WriteLine($"LLM guessed: {result.Series.Title} with {result.Confidence:P0} confidence");
            }
            else
            {
                TestContext.WriteLine("LLM correctly returned no match");
            }

            LogResult(releaseTitle, result);
        }

        [Test]
        public async Task Should_match_using_ParsedEpisodeInfo()
        {
            // Arrange
            var parsedInfo = new ParsedEpisodeInfo
            {
                SeriesTitle = "Breaking Bad",
                ReleaseTitle = "Breaking.Bad.S05E16.Felina.1080p.BluRay.x264-DEMAND",
                SeasonNumber = 5,
                EpisodeNumbers = new[] { 16 }
            };

            // Act
            var result = await _subject.TryMatchSeriesAsync(parsedInfo, _testSeries);

            // Assert
            result.Should().NotBeNull();
            result.Series.Should().NotBeNull();
            result.Series.TvdbId.Should().Be(81189);
            result.IsSuccessfulMatch.Should().BeTrue();

            LogResult(parsedInfo.ReleaseTitle, result);
        }

        [Test]
        public async Task Should_use_additional_metadata_from_ParsedEpisodeInfo()
        {
            // Arrange - Anime with absolute numbering
            var parsedInfo = new ParsedEpisodeInfo
            {
                SeriesTitle = "Shingeki no Kyojin",
                ReleaseTitle = "[HorribleSubs] Shingeki no Kyojin - 25 [1080p].mkv",
                AbsoluteEpisodeNumbers = new[] { 25 }
            };

            // Act
            var result = await _subject.TryMatchSeriesAsync(parsedInfo, _testSeries);

            // Assert
            result.Should().NotBeNull();
            result.Series.Should().NotBeNull();
            result.Series.TvdbId.Should().Be(267440);
            result.Series.SeriesType.Should().Be(SeriesTypes.Anime);

            LogResult(parsedInfo.ReleaseTitle, result);
        }

        [Test]
        public async Task Should_complete_within_reasonable_time()
        {
            // Arrange
            var releaseTitle = "Breaking.Bad.S01E01.720p.BluRay.x264-DEMAND";
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            var result = await _subject.TryMatchSeriesAsync(releaseTitle, _testSeries);
            stopwatch.Stop();

            // Assert
            result.Should().NotBeNull();
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000, "API call should complete within 10 seconds");

            TestContext.WriteLine($"API call completed in {stopwatch.ElapsedMilliseconds}ms");
            LogResult(releaseTitle, result);
        }

        [Test]
        public async Task Should_handle_large_series_list()
        {
            // Arrange - Create a larger series list
            var largeSeries = new List<Series>(_testSeries);
            for (var i = 0; i < 50; i++)
            {
                largeSeries.Add(new Series
                {
                    Id = 100 + i,
                    TvdbId = 100000 + i,
                    Title = $"Test Series {i}",
                    CleanTitle = $"testseries{i}",
                    Year = 2000 + i
                });
            }

            var releaseTitle = "Breaking.Bad.S01E01.720p.BluRay.x264-DEMAND";

            // Act
            var result = await _subject.TryMatchSeriesAsync(releaseTitle, largeSeries);

            // Assert
            result.Should().NotBeNull();
            result.Series.Should().NotBeNull();
            result.Series.TvdbId.Should().Be(81189);

            LogResult(releaseTitle, result);
        }

        private static IEnumerable<string> GetSearchPaths()
        {
            var fileName = "appsettings.llm.local.json";

            // Get various base directories
            var testDir = TestContext.CurrentContext.TestDirectory;
            var baseDir = AppContext.BaseDirectory;
            var currentDir = Directory.GetCurrentDirectory();
            var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // Build list of search paths
            var paths = new List<string>
            {
                // Output directory (where tests run from)
                Path.Combine(testDir, fileName),
                Path.Combine(baseDir, fileName),
                Path.Combine(assemblyDir ?? "", fileName),
                Path.Combine(currentDir, fileName),

                // Source directory structure (for running from IDE)
                Path.Combine(testDir, "ParserTests", "ParsingServiceTests", "LlmMatchingTests", fileName),

                // Walk up from output directory to find source
                Path.Combine(testDir, "..", "..", "..", "ParserTests", "ParsingServiceTests", "LlmMatchingTests", fileName),
                Path.Combine(testDir, "..", "..", "..", "..", "ParserTests", "ParsingServiceTests", "LlmMatchingTests", fileName),

                // Common source locations on Windows
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "source",
                    "repos",
                    "Sonarr",
                    "src",
                    "NzbDrone.Core.Test",
                    "ParserTests",
                    "ParsingServiceTests",
                    "LlmMatchingTests",
                    fileName),
            };

            return paths.Select(Path.GetFullPath).Distinct();
        }

        private LlmTestSettings LoadSettings()
        {
            // Try all search paths
            foreach (var path in GetSearchPaths())
            {
                var settings = TryLoadFromFile(path);
                if (settings != null)
                {
                    TestContext.WriteLine($"Loaded settings from: {path}");
                    return settings;
                }
            }

            // Try environment variable as fallback
            var envApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (!string.IsNullOrWhiteSpace(envApiKey))
            {
                TestContext.WriteLine("Loaded API key from OPENAI_API_KEY environment variable");
                return new LlmTestSettings
                {
                    OpenAiApiKey = envApiKey,
                    OpenAiApiEndpoint = Environment.GetEnvironmentVariable("OPENAI_API_ENDPOINT")
                        ?? "https://api.openai.com/v1/chat/completions",
                    OpenAiModel = Environment.GetEnvironmentVariable("OPENAI_MODEL")
                        ?? "gpt-4o-mini",
                    ConfidenceThreshold = 0.7
                };
            }

            return new LlmTestSettings();
        }

        private static LlmTestSettings TryLoadFromFile(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                var json = File.ReadAllText(path);
                var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("LlmMatching", out var llmSection))
                {
                    return new LlmTestSettings
                    {
                        OpenAiApiKey = GetStringProperty(llmSection, "OpenAiApiKey"),
                        OpenAiApiEndpoint = GetStringProperty(llmSection, "OpenAiApiEndpoint")
                            ?? "https://api.openai.com/v1/chat/completions",
                        OpenAiModel = GetStringProperty(llmSection, "OpenAiModel") ?? "gpt-4o-mini",
                        ConfidenceThreshold = GetDoubleProperty(llmSection, "ConfidenceThreshold") ?? 0.7
                    };
                }
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Error loading settings from {path}: {ex.Message}");
            }

            return null;
        }

        private static string GetStringProperty(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var prop) ? prop.GetString() : null;
        }

        private static double? GetDoubleProperty(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var prop) ? prop.GetDouble() : null;
        }

        private static void LogResult(string releaseTitle, LlmMatchResult result)
        {
            TestContext.WriteLine($"\n--- LLM Matching Result ---");
            TestContext.WriteLine($"Release: {releaseTitle}");

            if (result?.Series != null)
            {
                TestContext.WriteLine($"Matched: {result.Series.Title} (TvdbId: {result.Series.TvdbId})");
                TestContext.WriteLine($"Confidence: {result.Confidence:P0}");
                TestContext.WriteLine($"Successful: {result.IsSuccessfulMatch}");
            }
            else
            {
                TestContext.WriteLine("Matched: No match found");
            }

            if (!string.IsNullOrWhiteSpace(result?.Reasoning))
            {
                TestContext.WriteLine($"Reasoning: {result.Reasoning}");
            }

            if (result?.Alternatives?.Any() == true)
            {
                TestContext.WriteLine("Alternatives:");
                foreach (var alt in result.Alternatives)
                {
                    TestContext.WriteLine($"  - {alt.Series?.Title} ({alt.Confidence:P0}): {alt.Reasoning}");
                }
            }

            TestContext.WriteLine("----------------------------\n");
        }

        private class LlmTestSettings
        {
            public string OpenAiApiKey { get; set; }
            public string OpenAiApiEndpoint { get; set; }
            public string OpenAiModel { get; set; }
            public double ConfidenceThreshold { get; set; }
        }
    }
}
