using System;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class MinimumAgeSpecificationFixture : CoreTest<MinimumAgeSpecification>
    {
        private RemoteEpisode _remoteEpisode;

        [SetUp]
        public void Setup()
        {
            _remoteEpisode = new RemoteEpisode
            {
                Release = new ReleaseInfo() { DownloadProtocol = DownloadProtocol.Usenet }
            };
        }

        private void WithMinimumAge(int minutes)
        {
            Mocker.GetMock<IConfigService>().SetupGet(c => c.MinimumAge).Returns(minutes);
        }

        private void WithAge(int minutes)
        {
            _remoteEpisode.Release.PublishDate = DateTime.UtcNow.AddMinutes(-minutes);
        }

        [Test]
        public void should_return_true_when_minimum_age_is_set_to_zero()
        {
            WithMinimumAge(0);
            WithAge(100);

            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_age_is_greater_than_minimum_age()
        {
            WithMinimumAge(30);
            WithAge(100);

            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_when_age_is_less_than_minimum_age()
        {
            WithMinimumAge(30);
            WithAge(10);

            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeFalse();
        }
    }
}
