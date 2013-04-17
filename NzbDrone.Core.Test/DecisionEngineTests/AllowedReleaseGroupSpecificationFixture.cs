using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class AllowedReleaseGroupSpecificationFixture : CoreTest<AllowedReleaseGroupSpecification>
    {
        private RemoteEpisode _parseResult;

        [SetUp]
        public void Setup()
        {
            _parseResult = new RemoteEpisode
                {
                    Report = new ReportInfo
                        {
                            ReleaseGroup = "2HD"
                        }
                };
        }

        [Test]
        public void should_be_true_when_allowedReleaseGroups_is_empty()
        {

            Subject.IsSatisfiedBy(_parseResult).Should().BeTrue();
        }

        [Test]
        public void should_be_true_when_allowedReleaseGroups_is_nzbs_releaseGroup()
        {
            Subject.IsSatisfiedBy(_parseResult).Should().BeTrue();
        }

        [TestCase("2HD")]
        [TestCase("2hd")]
        [TestCase("other, 2hd, next")]
        [TestCase("other,2hd,next")]
        [TestCase("other,2hd,next,")]
        public void should_be_true_when_allowedReleaseGroups_contains_nzbs_releaseGroup(string allowedList)
        {
            Mocker.GetMock<IConfigService>().SetupGet(c => c.AllowedReleaseGroups).Returns(allowedList);
            Subject.IsSatisfiedBy(_parseResult).Should().BeTrue();
        }

        [Test]
        public void should_be_false_when_allowedReleaseGroups_does_not_contain_nzbs_releaseGroup()
        {
            Mocker.GetMock<IConfigService>().SetupGet(c => c.AllowedReleaseGroups).Returns("other");
            Subject.IsSatisfiedBy(_parseResult).Should().BeFalse();
        }
    }
}