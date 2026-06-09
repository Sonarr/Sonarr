using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.CustomFormats
{
    [TestFixture]
    public class CustomFormatCalculationServiceFixture : CoreTest<CustomFormatCalculationService>
    {
        private Series _series;
        private Episode _episode;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew()
                .With(s => s.Title = "Futurama")
                .Build();

            _episode = Builder<Episode>.CreateNew()
                .With(e => e.Title = "Amazon Women in the Mood")
                .With(e => e.SeasonNumber = 3)
                .With(e => e.EpisodeNumber = 1)
                .Build();
        }

        [Test]
        public void should_not_match_release_title_custom_format_from_episode_title_in_renamed_file()
        {
            var customFormat = GivenReleaseTitleCustomFormat("Amazon", @"\bAmazon\b");
            var localEpisode = GivenLocalEpisode();

            GivenCustomFormats(customFormat);

            var result = Subject.ParseCustomFormat(localEpisode, "Futurama.S03E01.Amazon.Women.in.the.Mood.1080p.WEB-DL.mkv");

            result.Should().BeEmpty();
        }

        [Test]
        public void should_not_match_release_title_custom_format_from_series_title_in_renamed_file()
        {
            var customFormat = GivenReleaseTitleCustomFormat("Futurama", @"\bFuturama\b");
            var localEpisode = GivenLocalEpisode();

            GivenCustomFormats(customFormat);

            var result = Subject.ParseCustomFormat(localEpisode, "Futurama.S03E01.1080p.WEB-DL.mkv");

            result.Should().BeEmpty();
        }

        [Test]
        public void should_still_match_release_title_custom_format_outside_known_titles()
        {
            var customFormat = GivenReleaseTitleCustomFormat("Amazon", @"\bAmazon\b");
            var localEpisode = GivenLocalEpisode();

            GivenCustomFormats(customFormat);

            var result = Subject.ParseCustomFormat(localEpisode, "Futurama.S03E01.Amazon.Women.in.the.Mood.1080p.Amazon.WEB-DL.mkv");

            result.Should().BeEquivalentTo(new[] { customFormat });
        }

        [Test]
        public void should_exclude_episode_title_when_calculating_custom_formats_for_existing_file()
        {
            var customFormat = GivenReleaseTitleCustomFormat("Amazon", @"\bAmazon\b");
            var episodeFile = new EpisodeFile
            {
                RelativePath = "Futurama.S03E01.Amazon.Women.in.the.Mood.1080p.WEB-DL.mkv",
                Quality = new QualityModel(Quality.WEBDL1080p),
                Languages = new List<Language> { Language.English },
                Series = _series,
                Episodes = new List<Episode> { _episode }
            };

            GivenCustomFormats(customFormat);

            var result = Subject.ParseCustomFormat(episodeFile, _series);

            result.Should().BeEmpty();
        }

        private LocalEpisode GivenLocalEpisode()
        {
            return new LocalEpisode
            {
                Series = _series,
                Episodes = new List<Episode> { _episode },
                Path = "/tv/Futurama/Season 03/Futurama.S03E01.Amazon.Women.in.the.Mood.1080p.WEB-DL.mkv",
                Quality = new QualityModel(Quality.WEBDL1080p),
                Languages = new List<Language> { Language.English },
                ReleaseType = ReleaseType.SingleEpisode
            };
        }

        private static CustomFormat GivenReleaseTitleCustomFormat(string name, string regex)
        {
            return new CustomFormat(name, new ReleaseTitleSpecification { Value = regex });
        }

        private void GivenCustomFormats(params CustomFormat[] customFormats)
        {
            Mocker.GetMock<ICustomFormatService>()
                .Setup(s => s.All())
                .Returns(new List<CustomFormat>(customFormats));
        }
    }
}
