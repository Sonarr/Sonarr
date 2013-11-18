using System;
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

            _pass1.Setup(c => c.IsSatisfiedBy(It.IsAny<LocalEpisode>())).Returns(true);
            _pass1.Setup(c => c.RejectionReason).Returns("_pass1");

            _pass2.Setup(c => c.IsSatisfiedBy(It.IsAny<LocalEpisode>())).Returns(true);
            _pass2.Setup(c => c.RejectionReason).Returns("_pass2");

            _pass3.Setup(c => c.IsSatisfiedBy(It.IsAny<LocalEpisode>())).Returns(true);
            _pass3.Setup(c => c.RejectionReason).Returns("_pass3");


            _fail1.Setup(c => c.IsSatisfiedBy(It.IsAny<LocalEpisode>())).Returns(false);
            _fail1.Setup(c => c.RejectionReason).Returns("_fail1");

            _fail2.Setup(c => c.IsSatisfiedBy(It.IsAny<LocalEpisode>())).Returns(false);
            _fail2.Setup(c => c.RejectionReason).Returns("_fail2");

            _fail3.Setup(c => c.IsSatisfiedBy(It.IsAny<LocalEpisode>())).Returns(false);
            _fail3.Setup(c => c.RejectionReason).Returns("_fail3");

            _videoFiles = new List<string> { @"C:\Test\Unsorted\The.Office.S03E115.DVDRip.XviD-OSiTV.avi" };
            _series = new Series();
            _quality = new QualityModel(Quality.DVD);
            _localEpisode = new LocalEpisode
            { 
                Series = _series,
                Quality = _quality,
                Path = @"C:\Test\Unsorted\The.Office.S03E115.DVDRip.XviD-OSiTV.avi"
            };

            Mocker.GetMock<IParsingService>()
                  .Setup(c => c.GetEpisodes(It.IsAny<String>(), It.IsAny<Series>(), It.IsAny<Boolean>()))
                  .Returns(_localEpisode);


            Mocker.GetMock<IMediaFileService>()
                .Setup(c => c.FilterExistingFiles(_videoFiles, It.IsAny<int>()))
                .Returns(_videoFiles);

        }

        private void GivenSpecifications(params Mock<IImportDecisionEngineSpecification>[] mocks)
        {
            Mocker.SetConstant<IEnumerable<IRejectWithReason>>(mocks.Select(c => c.Object));
        }

        [Test]
        public void should_call_all_specifications()
        {
            GivenSpecifications(_pass1, _pass2, _pass3, _fail1, _fail2, _fail3);

            Subject.GetImportDecisions(_videoFiles, new Series(), false);

            _fail1.Verify(c => c.IsSatisfiedBy(_localEpisode), Times.Once());
            _fail2.Verify(c => c.IsSatisfiedBy(_localEpisode), Times.Once());
            _fail3.Verify(c => c.IsSatisfiedBy(_localEpisode), Times.Once());
            _pass1.Verify(c => c.IsSatisfiedBy(_localEpisode), Times.Once());
            _pass2.Verify(c => c.IsSatisfiedBy(_localEpisode), Times.Once());
            _pass3.Verify(c => c.IsSatisfiedBy(_localEpisode), Times.Once());
        }

        [Test]
        public void should_return_rejected_if_single_specs_fail()
        {
            GivenSpecifications(_fail1);

            var result = Subject.GetImportDecisions(_videoFiles, new Series(), false);

            result.Single().Approved.Should().BeFalse();
        }

        [Test]
        public void should_return_rejected_if_one_of_specs_fail()
        {
            GivenSpecifications(_pass1, _fail1, _pass2, _pass3);

            var result = Subject.GetImportDecisions(_videoFiles, new Series(), false);

            result.Single().Approved.Should().BeFalse();
        }

        [Test]
        public void should_return_pass_if_all_specs_pass()
        {
            GivenSpecifications(_pass1, _pass2, _pass3);

            var result = Subject.GetImportDecisions(_videoFiles, new Series(), false);

            result.Single().Approved.Should().BeTrue();
        }

        [Test]
        public void should_have_same_number_of_rejections_as_specs_that_failed()
        {
            GivenSpecifications(_pass1, _pass2, _pass3, _fail1, _fail2, _fail3);

            var result = Subject.GetImportDecisions(_videoFiles, new Series(), false);
            result.Single().Rejections.Should().HaveCount(3);
        }

        [Test]
        public void failed_parse_shouldnt_blowup_the_process()
        {
            GivenSpecifications(_pass1);

            Mocker.GetMock<IParsingService>().Setup(c => c.GetEpisodes(It.IsAny<String>(), It.IsAny<Series>(), It.IsAny<Boolean>()))
                     .Throws<TestException>();

            _videoFiles = new List<String>
                {
                    "The.Office.S03E115.DVDRip.XviD-OSiTV",
                    "The.Office.S03E115.DVDRip.XviD-OSiTV",
                    "The.Office.S03E115.DVDRip.XviD-OSiTV"
                };


            Mocker.GetMock<IMediaFileService>()
                .Setup(c => c.FilterExistingFiles(_videoFiles, It.IsAny<int>()))
                .Returns(_videoFiles);

            Subject.GetImportDecisions(_videoFiles, new Series(), false);

            Mocker.GetMock<IParsingService>()
                  .Verify(c => c.GetEpisodes(It.IsAny<String>(), It.IsAny<Series>(), It.IsAny<Boolean>()), Times.Exactly(_videoFiles.Count));

            ExceptionVerification.ExpectedErrors(3);
        }

        [Test]
        public void should_use_file_quality_if_folder_quality_is_null()
        {
            GivenSpecifications(_pass1, _pass2, _pass3);
            var expectedQuality = QualityParser.ParseQuality(_videoFiles.Single());

            var result = Subject.GetImportDecisions(_videoFiles, new Series(), false, null);

            result.Single().LocalEpisode.Quality.Should().Be(expectedQuality);
        }

        [Test]
        public void should_use_file_quality_if_folder_quality_is_lower_than_file_quality()
        {
            GivenSpecifications(_pass1, _pass2, _pass3);
            var expectedQuality = QualityParser.ParseQuality(_videoFiles.Single());

            var result = Subject.GetImportDecisions(_videoFiles, new Series(), false, new QualityModel(Quality.SDTV));

            result.Single().LocalEpisode.Quality.Should().Be(expectedQuality);
        }

        [Test]
        public void should_use_folder_quality_when_it_is_greater_than_file_quality()
        {
            GivenSpecifications(_pass1, _pass2, _pass3);
            var expectedQuality = new QualityModel(Quality.Bluray1080p);

            var result = Subject.GetImportDecisions(_videoFiles, new Series(), false, expectedQuality);

            result.Single().LocalEpisode.Quality.Should().Be(expectedQuality);
        }
    }
}