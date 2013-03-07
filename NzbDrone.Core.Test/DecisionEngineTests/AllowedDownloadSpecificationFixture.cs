using System.ComponentModel;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Model;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class AllowedDownloadSpecificationFixture : CoreTest
    {
        private EpisodeParseResult parseResult;

        private Mock<IFetchableSpecification> pass1;
        private Mock<IFetchableSpecification> pass2;
        private Mock<IFetchableSpecification> pass3;

        private Mock<IFetchableSpecification> fail1;
        private Mock<IFetchableSpecification> fail2;
        private Mock<IFetchableSpecification> fail3;

        [SetUp]
        public void Setup()
        {
            pass1 = new Mock<IFetchableSpecification>();
            pass2 = new Mock<IFetchableSpecification>();
            pass3 = new Mock<IFetchableSpecification>();

            fail1 = new Mock<IFetchableSpecification>();
            fail2 = new Mock<IFetchableSpecification>();
            fail3 = new Mock<IFetchableSpecification>();

            pass1.Setup(c => c.IsSatisfiedBy(It.IsAny<EpisodeParseResult>())).Returns(true);
            pass2.Setup(c => c.IsSatisfiedBy(It.IsAny<EpisodeParseResult>())).Returns(true);
            pass3.Setup(c => c.IsSatisfiedBy(It.IsAny<EpisodeParseResult>())).Returns(true);

            fail1.Setup(c => c.IsSatisfiedBy(It.IsAny<EpisodeParseResult>())).Returns(false);
            fail2.Setup(c => c.IsSatisfiedBy(It.IsAny<EpisodeParseResult>())).Returns(false);
            fail3.Setup(c => c.IsSatisfiedBy(It.IsAny<EpisodeParseResult>())).Returns(false);

            parseResult = new EpisodeParseResult();

        }

        [Test]
        public void should_call_all_specifications()
        {
            throw new InvalidAsynchronousStateException();
        }

    }
}