using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.OrganizerTests.FileNameBuilderTests
{
    [TestFixture]

    public class TranslatedTitleFixture : CoreTest<FileNameBuilder>
    {
        private Series _series;
        private Episode _episode1;
        private EpisodeFile _episodeFile;
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>
                    .CreateNew()
                    .With(s => s.Title = "South Park")
                    .Build();

            _namingConfig = NamingConfig.Default;
            _namingConfig.RenameEpisodes = true;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);

            _episode1 = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.SeasonNumber = 15)
                            .With(e => e.EpisodeNumber = 6)
                            .With(e => e.AbsoluteEpisodeNumber = 100)
                            .Build();

            _episodeFile = new EpisodeFile { Quality = new QualityModel(Quality.HDTV720p), ReleaseGroup = "SonarrTest" };

            Mocker.GetMock<IQualityDefinitionService>()
                .Setup(v => v.Get(Moq.It.IsAny<Quality>()))
                .Returns<Quality>(v => Quality.DefaultQualityDefinitions.First(c => c.Quality == v));

            Mocker.GetMock<ICustomFormatService>()
                  .Setup(v => v.All())
                  .Returns(new List<CustomFormat>());
        }

        [Test]
        public void should_replace_series_title_with_french_translation_when_token_has_FR()
        {
            _series.Translations.Add(new SeriesTranslation { Language = "FR", Title = "South Park FR" });
            _namingConfig.StandardEpisodeFormat = "{Series Title:FR}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("South Park FR");
        }

        [Test]
        public void should_replace_series_title_with_german_translation_when_token_has_DE()
        {
            _series.Translations.Add(new SeriesTranslation { Language = "DE", Title = "South Park DE" });
            _namingConfig.StandardEpisodeFormat = "{Series Title:DE}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("South Park DE");
        }

        [Test]
        public void should_fallback_to_original_series_title_when_translation_not_found()
        {
            _series.Translations.Add(new SeriesTranslation { Language = "FR", Title = "South Park FR" });
            _namingConfig.StandardEpisodeFormat = "{Series Title:DE}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("South Park");
        }

        [Test]
        public void should_fallback_to_original_series_title_when_translations_empty()
        {
            _namingConfig.StandardEpisodeFormat = "{Series Title:FR}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("South Park");
        }

        [Test]
        public void should_match_language_code_case_insensitively()
        {
            _series.Translations.Add(new SeriesTranslation { Language = "FR", Title = "South Park FR" });
            _namingConfig.StandardEpisodeFormat = "{Series Title:fr}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("South Park FR");
        }

        [Test]
        public void should_replace_episode_title_with_french_translation_when_token_has_FR()
        {
            _episode1.Translations.Add(new EpisodeTranslation { Language = "FR", Title = "City Sushi FR" });
            _namingConfig.StandardEpisodeFormat = "{Episode Title:FR}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("City Sushi FR");
        }

        [Test]
        public void should_fallback_to_original_episode_title_when_translation_not_found()
        {
            _episode1.Translations.Add(new EpisodeTranslation { Language = "FR", Title = "City Sushi FR" });
            _namingConfig.StandardEpisodeFormat = "{Episode Title:DE}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("City Sushi");
        }

        [Test]
        public void should_fallback_to_original_episode_title_when_translations_empty()
        {
            _namingConfig.StandardEpisodeFormat = "{Episode Title:FR}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("City Sushi");
        }

        [Test]
        public void should_use_translated_title_with_Series_CleanTitle_token()
        {
            _series.Translations.Add(new SeriesTranslation { Language = "FR", Title = "South Park FR" });
            _namingConfig.StandardEpisodeFormat = "{Series CleanTitle:FR}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("South.Park.FR");
        }

        [Test]
        public void should_use_translated_title_with_Series_TitleYear_token()
        {
            _series.Translations.Add(new SeriesTranslation { Language = "FR", Title = "South Park FR" });
            _namingConfig.StandardEpisodeFormat = "{Series TitleYear:FR}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("South Park FR (2010)");
        }

        [Test]
        public void should_use_translated_title_with_Series_TitleThe_token()
        {
            _series.Title = "The Series";
            _series.Translations.Add(new SeriesTranslation { Language = "FR", Title = "La Série" });
            _namingConfig.StandardEpisodeFormat = "{Series TitleThe:FR}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("La Série");
        }

        [Test]
        public void should_use_translated_title_with_Series_TitleFirstCharacter_token()
        {
            _series.Translations.Add(new SeriesTranslation { Language = "FR", Title = "South Park FR" });
            _namingConfig.StandardEpisodeFormat = "{Series TitleFirstCharacter:FR}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("S");
        }

        [Test]
        public void should_use_multi_episode_translated_titles()
        {
            var episode2 = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "Episode Two")
                            .With(e => e.SeasonNumber = 15)
                            .With(e => e.EpisodeNumber = 7)
                            .Build();

            _episode1.Translations.Add(new EpisodeTranslation { Language = "FR", Title = "City Sushi FR" });
            episode2.Translations.Add(new EpisodeTranslation { Language = "FR", Title = "Episode Two FR" });

            _namingConfig.StandardEpisodeFormat = "{Episode Title:FR}";

            Subject.BuildFileName(new List<Episode> { _episode1, episode2 }, _series, _episodeFile)
                   .Should().Be("City Sushi FR + Episode Two FR");
        }

        [Test]
        public void should_still_truncate_when_customFormat_is_numeric_instead_of_language()
        {
            _series.Translations.Add(new SeriesTranslation { Language = "FR", Title = "South Park FR" });
            _series.Title = "The Fantastic Life of Mr. Sisko";
            _namingConfig.SeriesFolderFormat = "{Series Title:16}";

            var result = Subject.GetSeriesFolder(_series, _namingConfig);
            result.Should().Be("The Fantasti...");
        }
    }
}
