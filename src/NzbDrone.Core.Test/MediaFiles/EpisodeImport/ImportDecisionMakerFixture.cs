using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.EpisodeImport;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;
using FizzWare.NBuilder;
using NzbDrone.Core.Download;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Profiles.Languages;

namespace NzbDrone.Core.Test.MediaFiles.EpisodeImport
{
    [TestFixture]
    public class ImportDecisionMakerFixture : CoreTest<ImportDecisionMaker>
    {
        private List<string> _videoFiles;
        private LocalEpisode _localEpisode;
        private Series _series;
        private QualityModel _quality;

        private Mock<IImportDecisionEngineSpecification> _pass1;
        private Mock<IImportDecisionEngineSpecification> _pass2;
        private Mock<IImportDecisionEngineSpecification> _pass3;

        private Mock<IImportDecisionEngineSpecification> _fail1;
        private Mock<IImportDecisionEngineSpecification> _fail2;
        private Mock<IImportDecisionEngineSpecification> _fail3;

        [SetUp]
        public void Setup()
        {
            _pass1 = new Mock<IImportDecisionEngineSpecification>();
            _pass2 = new Mock<IImportDecisionEngineSpecification>();
            _pass3 = new Mock<IImportDecisionEngineSpecification>();

            _fail1 = new Mock<IImportDecisionEngineSpecification>();
            _fail2 = new Mock<IImportDecisionEngineSpecification>();
            _fail3 = new Mock<IImportDecisionEngineSpecification>();

            _pass1.Setup(c => c.IsSatisfiedBy(It.IsAny<LocalEpisode>(), It.IsAny<DownloadClientItem>())).Returns(Decision.Accept());
            _pass2.Setup(c => c.IsSatisfiedBy(It.IsAny<LocalEpisode>(), It.IsAny<DownloadClientItem>())).Returns(Decision.Accept());
            _pass3.Setup(c => c.IsSatisfiedBy(It.IsAny<LocalEpisode>(), It.IsAny<DownloadClientItem>())).Returns(Decision.Accept());

            _fail1.Setup(c => c.IsSatisfiedBy(It.IsAny<LocalEpisode>(), It.IsAny<DownloadClientItem>())).Returns(Decision.Reject("_fail1"));
            _fail2.Setup(c => c.IsSatisfiedBy(It.IsAny<LocalEpisode>(), It.IsAny<DownloadClientItem>())).Returns(Decision.Reject("_fail2"));
            _fail3.Setup(c => c.IsSatisfiedBy(It.IsAny<LocalEpisode>(), It.IsAny<DownloadClientItem>())).Returns(Decision.Reject("_fail3"));

            _series = Builder<Series>.CreateNew()
                                     .With(e => e.Profile = new Profile { Items = Qualities.QualityFixture.GetDefaultQualities() })
                                     .With(e => e.LanguageProfile = new LanguageProfile { Languages = Languages.LanguageFixture.GetDefaultLanguages() })
                                     .Build();

            _quality = new QualityModel(Quality.DVD);

            _localEpisode = new LocalEpisode
            { 
                Series = _series,
                Quality = _quality,
                Language = Language.Spanish,
                Episodes = new List<Episode> { new Episode() },
                Path = @"C:\Test\Unsorted\The.Office.S03E115.DVDRip.Spanish.XviD-OSiTV.avi"
            };

            Mocker.GetMock<IParsingService>()
                  .Setup(c => c.GetLocalEpisode(It.IsAny<string>(), It.IsAny<Series>(), It.IsAny<ParsedEpisodeInfo>(), It.IsAny<bool>()))
                  .Returns(_localEpisode);

            GivenVideoFiles(new List<string> { @"C:\Test\Unsorted\The.Office.S03E115.DVDRip.Spanish.XviD-OSiTV.avi".AsOsAgnostic() });
        }

        private void GivenSpecifications(params Mock<IImportDecisionEngineSpecification>[] mocks)
        {
            Mocker.SetConstant(mocks.Select(c => c.Object));
        }

        private void GivenVideoFiles(IEnumerable<string> videoFiles)
        {
            _videoFiles = videoFiles.ToList();

            Mocker.GetMock<IMediaFileService>()
                  .Setup(c => c.FilterExistingFiles(_videoFiles, It.IsAny<Series>()))
                  .Returns(_videoFiles);
        }

        [Test]
        public void should_call_all_specifications()
        {
            var downloadClientItem = Builder<DownloadClientItem>.CreateNew().Build();
            GivenSpecifications(_pass1, _pass2, _pass3, _fail1, _fail2, _fail3);

            Subject.GetImportDecisions(_videoFiles, new Series(), downloadClientItem, null, false);

            _fail1.Verify(c => c.IsSatisfiedBy(_localEpisode, downloadClientItem), Times.Once());
            _fail2.Verify(c => c.IsSatisfiedBy(_localEpisode, downloadClientItem), Times.Once());
            _fail3.Verify(c => c.IsSatisfiedBy(_localEpisode, downloadClientItem), Times.Once());
            _pass1.Verify(c => c.IsSatisfiedBy(_localEpisode, downloadClientItem), Times.Once());
            _pass2.Verify(c => c.IsSatisfiedBy(_localEpisode, downloadClientItem), Times.Once());
            _pass3.Verify(c => c.IsSatisfiedBy(_localEpisode, downloadClientItem), Times.Once());
        }

        [Test]
        public void should_return_rejected_if_single_specs_fail()
        {
            GivenSpecifications(_fail1);

            var result = Subject.GetImportDecisions(_videoFiles, new Series());

            result.Single().Approved.Should().BeFalse();
        }

        [Test]
        public void should_return_rejected_if_one_of_specs_fail()
        {
            GivenSpecifications(_pass1, _fail1, _pass2, _pass3);

            var result = Subject.GetImportDecisions(_videoFiles, new Series());

            result.Single().Approved.Should().BeFalse();
        }

        [Test]
        public void should_return_pass_if_all_specs_pass()
        {
            GivenSpecifications(_pass1, _pass2, _pass3);

            var result = Subject.GetImportDecisions(_videoFiles, new Series());

            result.Single().Approved.Should().BeTrue();
        }

        [Test]
        public void should_have_same_number_of_rejections_as_specs_that_failed()
        {
            GivenSpecifications(_pass1, _pass2, _pass3, _fail1, _fail2, _fail3);

            var result = Subject.GetImportDecisions(_videoFiles, new Series());
            result.Single().Rejections.Should().HaveCount(3);
        }

        [Test]
        public void should_not_blowup_the_process_due_to_failed_parse()
        {
            GivenSpecifications(_pass1);

            Mocker.GetMock<IParsingService>()
                  .Setup(c => c.GetLocalEpisode(It.IsAny<string>(), It.IsAny<Series>(), It.IsAny<ParsedEpisodeInfo>(), It.IsAny<bool>()))
                  .Throws<TestException>();

            _videoFiles = new List<string>
                {
                    "The.Office.S03E115.DVDRip.XviD-OSiTV",
                    "The.Office.S03E115.DVDRip.XviD-OSiTV",
                    "The.Office.S03E115.DVDRip.XviD-OSiTV"
                };

            GivenVideoFiles(_videoFiles);

            Subject.GetImportDecisions(_videoFiles, _series);

            Mocker.GetMock<IParsingService>()
                  .Verify(c => c.GetLocalEpisode(It.IsAny<string>(), It.IsAny<Series>(), It.IsAny<ParsedEpisodeInfo>(), It.IsAny<bool>()), Times.Exactly(_videoFiles.Count));

            ExceptionVerification.ExpectedErrors(3);
        }

        [Test]
        public void should_use_file_quality_if_folder_quality_is_null()
        {
            GivenSpecifications(_pass1, _pass2, _pass3);
            var expectedQuality = QualityParser.ParseQuality(_videoFiles.Single());

            var result = Subject.GetImportDecisions(_videoFiles, _series);

            result.Single().LocalEpisode.Quality.Should().Be(expectedQuality);
        }

        [Test]
        public void should_use_file_language_if_folder_language_is_null()
        {
            GivenSpecifications(_pass1, _pass2, _pass3);
            var expectedLanguage = Parser.Parser.ParseLanguage(_videoFiles.Single());

            var result = Subject.GetImportDecisions(_videoFiles, _series);

            result.Single().LocalEpisode.Language.Should().Be(expectedLanguage);
        }

        [Test]
        public void should_use_file_quality_if_file_quality_was_determined_by_name()
        {
            GivenSpecifications(_pass1, _pass2, _pass3);
            var expectedQuality = QualityParser.ParseQuality(_videoFiles.Single());

            var result = Subject.GetImportDecisions(_videoFiles, _series, null, new ParsedEpisodeInfo{Quality = new QualityModel(Quality.SDTV)}, true);

            result.Single().LocalEpisode.Quality.Should().Be(expectedQuality);
        }

        [Test]
        public void should_use_folder_quality_when_file_quality_was_determined_by_the_extension()
        {
            GivenSpecifications(_pass1, _pass2, _pass3);
            GivenVideoFiles(new string[] { @"C:\Test\Unsorted\The.Office.S03E115.mkv".AsOsAgnostic() });

            _localEpisode.Path = _videoFiles.Single();
            _localEpisode.Quality.QualitySource = QualitySource.Extension;
            _localEpisode.Quality.Quality = Quality.HDTV720p;

            var expectedQuality = new QualityModel(Quality.SDTV);

            var result = Subject.GetImportDecisions(_videoFiles, _series, null, new ParsedEpisodeInfo { Quality = expectedQuality }, true);

            result.Single().LocalEpisode.Quality.Should().Be(expectedQuality);
        }

        [Test]
        public void should_use_folder_quality_when_greater_than_file_quality()
        {
            GivenSpecifications(_pass1, _pass2, _pass3);
            GivenVideoFiles(new string[] { @"C:\Test\Unsorted\The.Office.S03E115.mkv".AsOsAgnostic() });

            _localEpisode.Path = _videoFiles.Single();
            _localEpisode.Quality.Quality = Quality.HDTV720p;

            var expectedQuality = new QualityModel(Quality.Bluray720p);

            var result = Subject.GetImportDecisions(_videoFiles, _series, null, new ParsedEpisodeInfo { Quality = expectedQuality }, true);

            result.Single().LocalEpisode.Quality.Should().Be(expectedQuality);
        }

        [Test]
        public void should_use_folder_language_when_greater_than_file_language()
        {
            GivenSpecifications(_pass1, _pass2, _pass3);
            GivenVideoFiles(new string[] { @"C:\Test\Unsorted\The.Office.S03E115.Spanish.mkv".AsOsAgnostic() });

            _localEpisode.Path = _videoFiles.Single();
            _localEpisode.Quality.Quality = Quality.HDTV720p;
            _localEpisode.Language = Language.Spanish;

            var expectedLanguage = Language.French;

            var result = Subject.GetImportDecisions(_videoFiles, _series, null, new ParsedEpisodeInfo { Language = expectedLanguage, Quality = new QualityModel (Quality.SDTV) }, true);

            result.Single().LocalEpisode.Language.Should().Be(expectedLanguage);
        }

        [Test]
        public void should_not_throw_if_episodes_are_not_found()
        {
            GivenSpecifications(_pass1);

            Mocker.GetMock<IParsingService>()
                  .Setup(c => c.GetLocalEpisode(It.IsAny<string>(), It.IsAny<Series>(), It.IsAny<ParsedEpisodeInfo>(), It.IsAny<bool>()))
                  .Returns(new LocalEpisode() { Path = "test" });

            _videoFiles = new List<string>
                {
                    "The.Office.S03E115.DVDRip.XviD-OSiTV",
                    "The.Office.S03E115.DVDRip.XviD-OSiTV",
                    "The.Office.S03E115.DVDRip.XviD-OSiTV"
                };

            GivenVideoFiles(_videoFiles);

            var decisions = Subject.GetImportDecisions(_videoFiles, _series);

            Mocker.GetMock<IParsingService>()
                  .Verify(c => c.GetLocalEpisode(It.IsAny<string>(), It.IsAny<Series>(), It.IsAny<ParsedEpisodeInfo>(), It.IsAny<bool>()), Times.Exactly(_videoFiles.Count));

            decisions.Should().HaveCount(3);
            decisions.First().Rejections.Should().NotBeEmpty();
        }

        [Test]
        public void should_not_use_folder_for_full_season()
        {
            var videoFiles = new[]
                             {
                                 @"C:\Test\Unsorted\Series.Title.S01\S01E01.mkv".AsOsAgnostic(),
                                 @"C:\Test\Unsorted\Series.Title.S01\S01E02.mkv".AsOsAgnostic(),
                                 @"C:\Test\Unsorted\Series.Title.S01\S01E03.mkv".AsOsAgnostic()
                             };

            GivenSpecifications(_pass1);
            GivenVideoFiles(videoFiles);

            var folderInfo = Parser.Parser.ParseTitle("Series.Title.S01");

            Subject.GetImportDecisions(_videoFiles, _series, null, folderInfo, true);

            Mocker.GetMock<IParsingService>()
                  .Verify(c => c.GetLocalEpisode(It.IsAny<string>(), It.IsAny<Series>(), null, true), Times.Exactly(3));

            Mocker.GetMock<IParsingService>()
                  .Verify(c => c.GetLocalEpisode(It.IsAny<string>(), It.IsAny<Series>(), It.Is<ParsedEpisodeInfo>(p => p != null), true), Times.Never());
        }

        [Test]
        public void should_not_use_folder_when_it_contains_more_than_one_valid_video_file()
        {
            var videoFiles = new[]
                             {
                                 @"C:\Test\Unsorted\Series.Title.S01E01\S01E01.mkv".AsOsAgnostic(),
                                 @"C:\Test\Unsorted\Series.Title.S01E01\1x01.mkv".AsOsAgnostic()
                             };

            GivenSpecifications(_pass1);
            GivenVideoFiles(videoFiles);

            var folderInfo = Parser.Parser.ParseTitle("Series.Title.S01E01");

            Subject.GetImportDecisions(_videoFiles, _series, null, folderInfo, true);

            Mocker.GetMock<IParsingService>()
                  .Verify(c => c.GetLocalEpisode(It.IsAny<string>(), It.IsAny<Series>(), null, true), Times.Exactly(2));

            Mocker.GetMock<IParsingService>()
                  .Verify(c => c.GetLocalEpisode(It.IsAny<string>(), It.IsAny<Series>(), It.Is<ParsedEpisodeInfo>(p => p != null), true), Times.Never());
        }

        [Test]
        public void should_use_folder_when_only_one_video_file()
        {
            var videoFiles = new[]
                             {
                                 @"C:\Test\Unsorted\Series.Title.S01E01\S01E01.mkv".AsOsAgnostic()
                             };

            GivenSpecifications(_pass1);
            GivenVideoFiles(videoFiles);

            var folderInfo = Parser.Parser.ParseTitle("Series.Title.S01E01");

            Subject.GetImportDecisions(_videoFiles, _series, null, folderInfo, true);

            Mocker.GetMock<IParsingService>()
                  .Verify(c => c.GetLocalEpisode(It.IsAny<string>(), It.IsAny<Series>(), It.IsAny<ParsedEpisodeInfo>(), true), Times.Exactly(1));

            Mocker.GetMock<IParsingService>()
                  .Verify(c => c.GetLocalEpisode(It.IsAny<string>(), It.IsAny<Series>(), null, true), Times.Never());
        }

        [Test]
        public void should_use_folder_when_only_one_video_file_and_a_sample()
        {
            var videoFiles = new[]
                             {
                                 @"C:\Test\Unsorted\Series.Title.S01E01\S01E01.mkv".AsOsAgnostic(),
                                 @"C:\Test\Unsorted\Series.Title.S01E01\S01E01.sample.mkv".AsOsAgnostic()
                             };

            GivenSpecifications(_pass1);
            GivenVideoFiles(videoFiles.ToList());

            Mocker.GetMock<IDetectSample>()
                  .Setup(s => s.IsSample(_series, It.IsAny<string>(), It.IsAny<bool>()))
                  .Returns((Series s, string path, bool special) =>
                  {
                      if (path.Contains("sample"))
                      {
                          return DetectSampleResult.Sample;
                      }

                      return DetectSampleResult.NotSample;
                  });

            var folderInfo = Parser.Parser.ParseTitle("Series.Title.S01E01");

            Subject.GetImportDecisions(_videoFiles, _series, null, folderInfo, true);

            Mocker.GetMock<IParsingService>()
                  .Verify(c => c.GetLocalEpisode(It.IsAny<string>(), It.IsAny<Series>(), It.IsAny<ParsedEpisodeInfo>(), true), Times.Exactly(2));

            Mocker.GetMock<IParsingService>()
                  .Verify(c => c.GetLocalEpisode(It.IsAny<string>(), It.IsAny<Series>(), null, true), Times.Never());
        }

        [Test]
        public void should_not_use_folder_name_if_file_name_is_scene_name()
        {
            var videoFiles = new[]
                             {
                                 @"C:\Test\Unsorted\Series.Title.S01E01.720p.HDTV-LOL\Series.Title.S01E01.720p.HDTV-LOL.mkv".AsOsAgnostic()
                             };

            GivenSpecifications(_pass1);
            GivenVideoFiles(videoFiles);

            var folderInfo = Parser.Parser.ParseTitle("Series.Title.S01E01.720p.HDTV-LOL");

            Subject.GetImportDecisions(_videoFiles, _series, null, folderInfo, true);

            Mocker.GetMock<IParsingService>()
                  .Verify(c => c.GetLocalEpisode(It.IsAny<string>(), It.IsAny<Series>(), null, true), Times.Exactly(1));

            Mocker.GetMock<IParsingService>()
                  .Verify(c => c.GetLocalEpisode(It.IsAny<string>(), It.IsAny<Series>(), It.Is<ParsedEpisodeInfo>(p => p != null), true), Times.Never());
        }

        [Test]
        public void should_not_use_folder_quality_when_it_is_unknown()
        {
            GivenSpecifications(_pass1, _pass2, _pass3);

            _series.Profile = new Profile
                              {
                                  Items = Qualities.QualityFixture.GetDefaultQualities(Quality.DVD, Quality.Unknown)
                              };


            var folderQuality = new QualityModel(Quality.Unknown);

            var result = Subject.GetImportDecisions(_videoFiles, _series, null, new ParsedEpisodeInfo { Quality = folderQuality}, true);

            result.Single().LocalEpisode.Quality.Should().Be(_quality);
        }

        [Test]
        public void should_return_a_decision_when_exception_is_caught()
        {
            Mocker.GetMock<IParsingService>()
                  .Setup(c => c.GetLocalEpisode(It.IsAny<string>(), It.IsAny<Series>(), It.IsAny<ParsedEpisodeInfo>(), It.IsAny<bool>()))
                  .Throws<TestException>();

            _videoFiles = new List<string>
                {
                    "The.Office.S03E115.DVDRip.XviD-OSiTV"
                };

            GivenVideoFiles(_videoFiles);

            Subject.GetImportDecisions(_videoFiles, _series).Should().HaveCount(1);

            ExceptionVerification.ExpectedErrors(1);
        }
    }
}
