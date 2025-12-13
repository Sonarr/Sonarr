using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.EpisodeImport;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.EpisodeImport
{
    [TestFixture]
    public class LocalEpisodeCustomFormatCalculationServiceFixture : CoreTest<LocalEpisodeCustomFormatCalculationService>
    {
        private const int EnglishCustomFormatScore = 10;
        private const int SpanishCustomFormatScore = 2;
        private LocalEpisode _localEpisode;
        private Series _series;
        private QualityModel _quality;
        private CustomFormat _englishCustomFormat;
        private CustomFormat _spanishCustomFormat;

        [SetUp]
        public void Setup()
        {
            _englishCustomFormat = new CustomFormat("HasEnglish") { Id = 1 };
            _spanishCustomFormat = new CustomFormat("HasSpanish") { Id = 2 };
            _series = Builder<Series>.CreateNew()
                                     .With(e => e.Path = @"C:\Test\Series".AsOsAgnostic())
                                     .With(e => e.QualityProfile = new QualityProfile
                                     {
                                         Items = Qualities.QualityFixture.GetDefaultQualities(),
                                         FormatItems = [
                                             new ProfileFormatItem { Format = _englishCustomFormat, Score = EnglishCustomFormatScore },
                                             new ProfileFormatItem { Format = _spanishCustomFormat, Score = SpanishCustomFormatScore }
                                         ]
                                     })
                                     .Build();

            _quality = new QualityModel(Quality.DVD);

            _localEpisode = new LocalEpisode
            {
                Series = _series,
                Quality = _quality,
                Languages = new List<Language> { Language.Spanish },
                Episodes = new List<Episode> { new Episode() },
                Path = @"C:\Test\Unsorted\The.Office.S03E115.DVDRip.Spanish.XviD-OSiTV.avi"
            };

            Mocker.GetMock<ICustomFormatCalculationService>()
                .Setup(s => s.ParseCustomFormat(It.IsAny<LocalEpisode>(), It.Is<string>(x => x.Contains("English"))))
                .Returns([_englishCustomFormat]);

            Mocker.GetMock<ICustomFormatCalculationService>()
                .Setup(s => s.ParseCustomFormat(It.IsAny<LocalEpisode>(), It.Is<string>(x => x.Contains("Spanish"))))
                .Returns([_spanishCustomFormat]);
        }

        [Test]
        public void should_build_a_filename_and_use_it_to_calculate_custom_score()
        {
            var renamedFileName = @"C:\Test\Unsorted\The.Office.S03E115.DVDRip.English.XviD-OSiTV.avi";

            Mocker.GetMock<IBuildFileNames>()
                .Setup(s => s.BuildFileName(It.IsAny<List<Episode>>(), It.IsAny<Series>(), It.IsAny<EpisodeFile>(), "", null, null))
                .Returns(renamedFileName);

            Subject.ParseEpisodeCustomFormats(_localEpisode, out var fileName).Should().BeEquivalentTo([_englishCustomFormat]);
        }

        [Test]
        public void should_update_custom_formats_on_local_episode()
        {
            var renamedFileName = @"C:\Test\Unsorted\The.Office.S03E115.DVDRip.English.XviD-OSiTV.avi";

            Mocker.GetMock<IBuildFileNames>()
                .Setup(s => s.BuildFileName(It.IsAny<List<Episode>>(), It.IsAny<Series>(), It.IsAny<EpisodeFile>(), "", null, null))
                .Returns(renamedFileName);

            Subject.UpdateEpisodeCustomFormats(_localEpisode);
            _localEpisode.FileNameUsedForCustomFormatCalculation.Should().Be(renamedFileName);

            _localEpisode.OriginalFileNameCustomFormats.Should().BeEquivalentTo([_spanishCustomFormat]);
            _localEpisode.OriginalFileNameCustomFormatScore.Should().Be(SpanishCustomFormatScore);

            _localEpisode.CustomFormats.Should().BeEquivalentTo([_englishCustomFormat]);
            _localEpisode.CustomFormatScore.Should().Be(EnglishCustomFormatScore);
        }
    }
}
