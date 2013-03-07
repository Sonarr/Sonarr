using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class QualityUpgradableSpecificationFixture : CoreTest<QualityUpgradableSpecification>
    {
        [Test]
        public void IsUpgradePossible_should_return_true_if_current_episode_is_less_than_cutoff()
        {
            Subject.IsUpgradable(new QualityProfile { Cutoff = Quality.Bluray1080p },
                                 new QualityModel(Quality.DVD, true)).Should().BeTrue();
        }

        [Test]
        public void IsUpgradePossible_should_return_false_if_current_episode_is_equal_to_cutoff()
        {
            Subject.IsUpgradable(new QualityProfile { Cutoff = Quality.HDTV720p },
                               new QualityModel(Quality.HDTV720p, true)).Should().BeFalse();
        }

        [Test]
        public void IsUpgradePossible_should_return_false_if_current_episode_is_greater_than_cutoff()
        {
            Subject.IsUpgradable(new QualityProfile { Cutoff = Quality.HDTV720p },
                                new QualityModel(Quality.Bluray1080p, true)).Should().BeFalse();
        }
    }
}