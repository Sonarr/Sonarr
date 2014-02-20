using System;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class RetentionSpecificationFixture : CoreTest<RetentionSpecification>
    {

        private RemoteEpisode parseResult;

        [SetUp]
        public void Setup()
        {
            parseResult = new RemoteEpisode
            {
                Release = new ReleaseInfo()
            };
        }

        private void WithRetention(int days)
        {
            Mocker.GetMock<IConfigService>().SetupGet(c => c.Retention).Returns(days);
        }

        private void WithAge(int days)
        {
            parseResult.Release.PublishDate = DateTime.Now.AddDays(-days);
        }

        [Test]
        public void should_return_true_when_retention_is_set_to_zero()
        {
            WithRetention(0);
            WithAge(100);

            Subject.IsSatisfiedBy(parseResult, null).Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_release_if_younger_than_retention()
        {
            WithRetention(1000);
            WithAge(100);

            Subject.IsSatisfiedBy(parseResult, null).Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_release_and_retention_are_the_same()
        {
            WithRetention(100);
            WithAge(100);

            Subject.IsSatisfiedBy(parseResult, null).Should().BeTrue();
        }

        [Test]
        public void should_return_false_when_old_than_retention()
        {
            WithRetention(10);
            WithAge(100);

            Subject.IsSatisfiedBy(parseResult, null).Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_release_came_out_today_and_retention_is_zero()
        {
            WithRetention(0);
            WithAge(100);

            Subject.IsSatisfiedBy(parseResult, null).Should().BeTrue();
        }
    }
}