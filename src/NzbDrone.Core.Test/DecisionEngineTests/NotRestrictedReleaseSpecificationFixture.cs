using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class NotRestrictedReleaseSpecificationFixture : CoreTest<NotRestrictedReleaseSpecification>
    {
        private RemoteEpisode _parseResult;

        [SetUp]
        public void Setup()
        {
            _parseResult = new RemoteEpisode
                           {
                               Release = new ReleaseInfo
                                         {
                                             Title = "Dexter.S08E01.EDITED.WEBRip.x264-KYR"
                                         }
                           };
        }

        [Test]
        public void should_be_true_when_restrictions_are_empty()
        {
            Subject.IsSatisfiedBy(_parseResult, null).Should().BeTrue();
        }

        [TestCase("KYR")]
        [TestCase("EDITED")]
        [TestCase("edited")]
        [TestCase("2HD\nKYR")]
        [TestCase("2HD\nkyr")]
        public void should_be_false_when_nzb_contains_a_restricted_term(string restrictions)
        {
            Mocker.GetMock<IConfigService>().SetupGet(c => c.ReleaseRestrictions).Returns(restrictions);
            Subject.IsSatisfiedBy(_parseResult, null).Should().BeFalse();
        }

        [TestCase("NotReal")]
        [TestCase("LoL")]
        [TestCase("Hello\nWorld")]
        public void should_be_true_when_nzb_does_not_contain_a_restricted_term(string restrictions)
        {
            Mocker.GetMock<IConfigService>().SetupGet(c => c.ReleaseRestrictions).Returns(restrictions);
            Subject.IsSatisfiedBy(_parseResult, null).Should().BeTrue();
        }

        [Test]
        public void should_not_try_to_find_empty_string_as_a_match()
        {
            Mocker.GetMock<IConfigService>().SetupGet(c => c.ReleaseRestrictions).Returns("test\n");
            Subject.IsSatisfiedBy(_parseResult, null).Should().BeTrue();
        }
    }
}