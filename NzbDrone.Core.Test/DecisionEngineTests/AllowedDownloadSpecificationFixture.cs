using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Model;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class AllowedDownloadSpecificationFixture : CoreTest<DownloadDirector>
    {
        private EpisodeParseResult _parseResult;

        private Mock<IFetchableSpecification> _pass1;
        private Mock<IFetchableSpecification> _pass2;
        private Mock<IFetchableSpecification> _pass3;

        private Mock<IFetchableSpecification> _fail1;
        private Mock<IFetchableSpecification> _fail2;
        private Mock<IFetchableSpecification> _fail3;

        [SetUp]
        public void Setup()
        {
            _pass1 = new Mock<IFetchableSpecification>();
            _pass2 = new Mock<IFetchableSpecification>();
            _pass3 = new Mock<IFetchableSpecification>();

            _fail1 = new Mock<IFetchableSpecification>();
            _fail2 = new Mock<IFetchableSpecification>();
            _fail3 = new Mock<IFetchableSpecification>();

            _pass1.Setup(c => c.IsSatisfiedBy(It.IsAny<EpisodeParseResult>())).Returns(true);
            _pass2.Setup(c => c.IsSatisfiedBy(It.IsAny<EpisodeParseResult>())).Returns(true);
            _pass3.Setup(c => c.IsSatisfiedBy(It.IsAny<EpisodeParseResult>())).Returns(true);

            _fail1.Setup(c => c.IsSatisfiedBy(It.IsAny<EpisodeParseResult>())).Returns(false);
            _fail2.Setup(c => c.IsSatisfiedBy(It.IsAny<EpisodeParseResult>())).Returns(false);
            _fail3.Setup(c => c.IsSatisfiedBy(It.IsAny<EpisodeParseResult>())).Returns(false);

            _parseResult = new EpisodeParseResult();

        }

        private void GivenSpecifications(params Mock<IFetchableSpecification>[] mocks)
        {
            Mocker.SetConstant(mocks.Select(c => c.Object));
        }

        [Test]
        public void should_call_all_specifications()
        {
            GivenSpecifications(_pass1, _pass2, _pass3, _fail1, _fail2, _fail3);

            Subject.GetDownloadDecision(_parseResult);

            _fail1.Verify(c => c.IsSatisfiedBy(_parseResult), Times.Once());
            _fail2.Verify(c => c.IsSatisfiedBy(_parseResult), Times.Once());
            _fail3.Verify(c => c.IsSatisfiedBy(_parseResult), Times.Once());
            _pass1.Verify(c => c.IsSatisfiedBy(_parseResult), Times.Once());
            _pass2.Verify(c => c.IsSatisfiedBy(_parseResult), Times.Once());
            _pass3.Verify(c => c.IsSatisfiedBy(_parseResult), Times.Once());
        }

        [Test]
        public void should_return_rejected_if_one_of_specs_fail()
        {
            GivenSpecifications(_pass1, _fail1, _pass2, _pass3);

            var result = Subject.GetDownloadDecision(_parseResult);

            result.Approved.Should().BeFalse();
        }

        [Test]
        public void should_return_pass_if_all_specs_pass()
        {
            GivenSpecifications(_pass1, _pass2, _pass3);

            var result = Subject.GetDownloadDecision(_parseResult);

            result.Approved.Should().BeTrue();
        }

        [Test]
        public void should_have_same_number_of_rejections_as_specs_that_failed()
        {
            GivenSpecifications(_pass1, _pass2, _pass3, _fail1, _fail2, _fail3);

            var result = Subject.GetDownloadDecision(_parseResult);
            result.Rejections.Should().HaveCount(3);
        }

    }
}