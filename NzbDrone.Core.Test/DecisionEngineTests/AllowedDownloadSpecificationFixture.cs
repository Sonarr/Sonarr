using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class AllowedDownloadSpecificationFixture : CoreTest<DownloadDecisionMaker>
    {
        private List<ReportInfo> _reports;
        private RemoteEpisode _remoteEpisode;

        private Mock<IDecisionEngineSpecification> _pass1;
        private Mock<IDecisionEngineSpecification> _pass2;
        private Mock<IDecisionEngineSpecification> _pass3;

        private Mock<IDecisionEngineSpecification> _fail1;
        private Mock<IDecisionEngineSpecification> _fail2;
        private Mock<IDecisionEngineSpecification> _fail3;

        [SetUp]
        public void Setup()
        {
            _pass1 = new Mock<IDecisionEngineSpecification>();
            _pass2 = new Mock<IDecisionEngineSpecification>();
            _pass3 = new Mock<IDecisionEngineSpecification>();

            _fail1 = new Mock<IDecisionEngineSpecification>();
            _fail2 = new Mock<IDecisionEngineSpecification>();
            _fail3 = new Mock<IDecisionEngineSpecification>();

            _pass1.Setup(c => c.IsSatisfiedBy(It.IsAny<RemoteEpisode>())).Returns(true);
            _pass2.Setup(c => c.IsSatisfiedBy(It.IsAny<RemoteEpisode>())).Returns(true);
            _pass3.Setup(c => c.IsSatisfiedBy(It.IsAny<RemoteEpisode>())).Returns(true);

            _fail1.Setup(c => c.IsSatisfiedBy(It.IsAny<RemoteEpisode>())).Returns(false);
            _fail2.Setup(c => c.IsSatisfiedBy(It.IsAny<RemoteEpisode>())).Returns(false);
            _fail3.Setup(c => c.IsSatisfiedBy(It.IsAny<RemoteEpisode>())).Returns(false);

            _reports = new List<ReportInfo> {new ReportInfo()};
            _remoteEpisode = new RemoteEpisode();

            Mocker.GetMock<IParsingService>().Setup(c => c.Map(It.IsAny<ReportInfo>()))
                  .Returns(_remoteEpisode);

        }

        private void GivenSpecifications(params Mock<IDecisionEngineSpecification>[] mocks)
        {
            Mocker.SetConstant<IEnumerable<IRejectWithReason>>(mocks.Select(c => c.Object));
        }

        [Test]
        public void should_call_all_specifications()
        {
            GivenSpecifications(_pass1, _pass2, _pass3, _fail1, _fail2, _fail3);

            Subject.GetRssDecision(_reports).ToList();

            _fail1.Verify(c => c.IsSatisfiedBy(_remoteEpisode), Times.Once());
            _fail2.Verify(c => c.IsSatisfiedBy(_remoteEpisode), Times.Once());
            _fail3.Verify(c => c.IsSatisfiedBy(_remoteEpisode), Times.Once());
            _pass1.Verify(c => c.IsSatisfiedBy(_remoteEpisode), Times.Once());
            _pass2.Verify(c => c.IsSatisfiedBy(_remoteEpisode), Times.Once());
            _pass3.Verify(c => c.IsSatisfiedBy(_remoteEpisode), Times.Once());
        }

        [Test]
        public void should_return_rejected_if_one_of_specs_fail()
        {
            GivenSpecifications(_pass1, _fail1, _pass2, _pass3);

            var result = Subject.GetRssDecision(_reports);

            result.Single().Approved.Should().BeFalse();
        }

        [Test]
        public void should_return_pass_if_all_specs_pass()
        {
            GivenSpecifications(_pass1, _pass2, _pass3);

            var result = Subject.GetRssDecision(_reports);

            result.Single().Approved.Should().BeTrue();
        }

        [Test]
        public void should_have_same_number_of_rejections_as_specs_that_failed()
        {
            GivenSpecifications(_pass1, _pass2, _pass3, _fail1, _fail2, _fail3);

            var result = Subject.GetRssDecision(_reports);
            result.Single().Rejections.Should().HaveCount(3);
        }



    }
}