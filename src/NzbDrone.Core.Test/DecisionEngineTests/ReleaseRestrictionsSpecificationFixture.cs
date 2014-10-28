using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Restrictions;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class ReleaseRestrictionsSpecificationFixture : CoreTest<ReleaseRestrictionsSpecification>
    {
        private RemoteEpisode _parseResult;

        [SetUp]
        public void Setup()
        {
            _parseResult = new RemoteEpisode
                           {
                               Series = new Series
                                        {
                                            Tags = new HashSet<Int32>()
                                        },
                               Release = new ReleaseInfo
                                         {
                                             Title = "Dexter.S08E01.EDITED.WEBRip.x264-KYR"
                                         }
                           };
        }

        private void GivenRestictions(String required, String ignored)
        {
            Mocker.GetMock<IRestrictionService>()
                  .Setup(s => s.AllForTags(It.IsAny<HashSet<Int32>>()))
                  .Returns(new List<Restriction>
                           {
                               new Restriction
                               {
                                   Required = required,
                                   Ignored = ignored
                               }
                           });
        }

        [Test]
        public void should_be_true_when_restrictions_are_empty()
        {
            Mocker.GetMock<IRestrictionService>()
                  .Setup(s => s.AllForTags(It.IsAny<HashSet<Int32>>()))
                  .Returns(new List<Restriction>());

            Subject.IsSatisfiedBy(_parseResult, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_true_when_title_contains_one_required_term()
        {
            GivenRestictions("WEBRip", null);

            Subject.IsSatisfiedBy(_parseResult, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_false_when_title_does_not_contain_any_required_terms()
        {
            GivenRestictions("doesnt,exist", null);

            Subject.IsSatisfiedBy(_parseResult, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_true_when_title_does_not_contain_any_ignored_terms()
        {
            GivenRestictions(null, "ignored");

            Subject.IsSatisfiedBy(_parseResult, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_false_when_title_contains_one_anded_ignored_terms()
        {
            GivenRestictions(null, "edited");

            Subject.IsSatisfiedBy(_parseResult, null).Accepted.Should().BeFalse();
        }

        [TestCase("EdiTED")]
        [TestCase("webrip")]
        [TestCase("X264")]
        [TestCase("X264,NOTTHERE")]
        public void should_ignore_case_when_matching_required(String required)
        {
            GivenRestictions(required, null);

            Subject.IsSatisfiedBy(_parseResult, null).Accepted.Should().BeTrue();
        }

        [TestCase("EdiTED")]
        [TestCase("webrip")]
        [TestCase("X264")]
        [TestCase("X264,NOTTHERE")]
        public void should_ignore_case_when_matching_ignored(String ignored)
        {
            GivenRestictions(null, ignored);

            Subject.IsSatisfiedBy(_parseResult, null).Accepted.Should().BeFalse();
        }
    }
}
