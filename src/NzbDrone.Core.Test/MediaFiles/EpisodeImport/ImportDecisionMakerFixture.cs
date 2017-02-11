using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.EpisodeImport;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;
using FizzWare.NBuilder;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles.EpisodeImport.Aggregation;

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
                                     .With(e => e.Path = @"C:\Test\Series".AsOsAgnostic())
                                     .With(e => e.Profile = new Profile { Items = Qualities.QualityFixture.GetDefaultQualities() })
                                     .Build();

            _quality = new QualityModel(Quality.DVD);

            _localEpisode = new LocalEpisode
            {
                Series = _series,
                Quality = _quality,
                Episodes = new List<Episode> { new Episode() },
                Path = @"C:\Test\Unsorted\The.Office.S03E115.DVDRip.XviD-OSiTV.avi"
            };

            GivenVideoFiles(new List<string> { @"C:\Test\Unsorted\The.Office.S03E115.DVDRip.XviD-OSiTV.avi".AsOsAgnostic() });
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

        private void GivenAugmentationSuccess()
        {
            Mocker.GetMock<IAugmentingService>()
                  .Setup(s => s.Augment(It.IsAny<LocalEpisode>(), It.IsAny<bool>()))
                  .Callback<LocalEpisode, bool>((localEpisode, otherFiles) =>
                  {
                      localEpisode.Episodes = _localEpisode.Episodes;
                  });
        }

        [Test]
        public void should_call_all_specifications()
        {
            var downloadClientItem = Builder<DownloadClientItem>.CreateNew().Build();
            GivenAugmentationSuccess();
            GivenSpecifications(_pass1, _pass2, _pass3, _fail1, _fail2, _fail3);

            Subject.GetImportDecisions(_videoFiles, _series, downloadClientItem, null, false, true);

            _fail1.Verify(c => c.IsSatisfiedBy(It.IsAny<LocalEpisode>(), downloadClientItem), Times.Once());
            _fail2.Verify(c => c.IsSatisfiedBy(It.IsAny<LocalEpisode>(), downloadClientItem), Times.Once());
            _fail3.Verify(c => c.IsSatisfiedBy(It.IsAny<LocalEpisode>(), downloadClientItem), Times.Once());
            _pass1.Verify(c => c.IsSatisfiedBy(It.IsAny<LocalEpisode>(), downloadClientItem), Times.Once());
            _pass2.Verify(c => c.IsSatisfiedBy(It.IsAny<LocalEpisode>(), downloadClientItem), Times.Once());
            _pass3.Verify(c => c.IsSatisfiedBy(It.IsAny<LocalEpisode>(), downloadClientItem), Times.Once());
        }

        [Test]
        public void should_return_rejected_if_single_specs_fail()
        {
            GivenSpecifications(_fail1);

            var result = Subject.GetImportDecisions(_videoFiles, _series);

            result.Single().Approved.Should().BeFalse();
        }

        [Test]
        public void should_return_rejected_if_one_of_specs_fail()
        {
            GivenSpecifications(_pass1, _fail1, _pass2, _pass3);

            var result = Subject.GetImportDecisions(_videoFiles, _series);

            result.Single().Approved.Should().BeFalse();
        }

        [Test]
        public void should_return_approved_if_all_specs_pass()
        {
            GivenAugmentationSuccess();
            GivenSpecifications(_pass1, _pass2, _pass3);

            var result = Subject.GetImportDecisions(_videoFiles, _series);

            result.Single().Approved.Should().BeTrue();
        }

        [Test]
        public void should_have_same_number_of_rejections_as_specs_that_failed()
        {
            GivenAugmentationSuccess();
            GivenSpecifications(_pass1, _pass2, _pass3, _fail1, _fail2, _fail3);

            var result = Subject.GetImportDecisions(_videoFiles, _series);
            result.Single().Rejections.Should().HaveCount(3);
        }

        [Test]
        public void should_not_blowup_the_process_due_to_failed_parse()
        {
            GivenSpecifications(_pass1);

            Mocker.GetMock<IAugmentingService>()
                  .Setup(c => c.Augment(It.IsAny<LocalEpisode>(), It.IsAny<bool>()))
                  .Throws<TestException>();

            _videoFiles = new List<string>
                {
                    "The.Office.S03E115.DVDRip.XviD-OSiTV",
                    "The.Office.S03E115.DVDRip.XviD-OSiTV",
                    "The.Office.S03E115.DVDRip.XviD-OSiTV"
                };

            GivenVideoFiles(_videoFiles);

            Subject.GetImportDecisions(_videoFiles, _series);

            Mocker.GetMock<IAugmentingService>()
                  .Verify(c => c.Augment(It.IsAny<LocalEpisode>(), It.IsAny<bool>()), Times.Exactly(_videoFiles.Count));

            ExceptionVerification.ExpectedErrors(3);
        }

        [Test]
        public void should_not_throw_if_episodes_are_not_found()
        {
            GivenSpecifications(_pass1);

            _videoFiles = new List<string>
                {
                    "The.Office.S03E115.DVDRip.XviD-OSiTV",
                    "The.Office.S03E115.DVDRip.XviD-OSiTV",
                    "The.Office.S03E115.DVDRip.XviD-OSiTV"
                };

            GivenVideoFiles(_videoFiles);

            var decisions = Subject.GetImportDecisions(_videoFiles, _series);

            Mocker.GetMock<IAugmentingService>()
                  .Verify(c => c.Augment(It.IsAny<LocalEpisode>(), It.IsAny<bool>()), Times.Exactly(_videoFiles.Count));

            decisions.Should().HaveCount(3);
            decisions.First().Rejections.Should().NotBeEmpty();
        }

        [Test]
        public void should_return_a_decision_when_exception_is_caught()
        {
            Mocker.GetMock<IAugmentingService>()
                  .Setup(c => c.Augment(It.IsAny<LocalEpisode>(), It.IsAny<bool>()))
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
