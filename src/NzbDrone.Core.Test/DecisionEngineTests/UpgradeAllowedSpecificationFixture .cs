using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Test.Languages;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class UpgradeAllowedSpecificationFixture : CoreTest<UpgradableSpecification>
    {
        [Test]
        public void should_return_false_when_quality_is_better_languages_are_the_same_and_upgrade_allowed_is_false_for_quality_profile()
        {
            Subject.IsUpgradeAllowed(
                new QualityProfile
                {
                    Cutoff = Quality.Bluray1080p.Id,
                    Items = Qualities.QualityFixture.GetDefaultQualities(),
                    UpgradeAllowed = false
                },
                new QualityModel(Quality.DVD),
                new QualityModel(Quality.Bluray1080p))
            .Should().BeFalse();
        }

        [Test]
        public void should_return_true_for_language_upgrade_when_upgrading_is_allowed()
        {
            Subject.IsUpgradeAllowed(
                new QualityProfile
                {
                    Cutoff = Quality.Bluray1080p.Id,
                    Items = Qualities.QualityFixture.GetDefaultQualities(),
                    UpgradeAllowed = true
                },
                new QualityModel(Quality.DVD),
                new QualityModel(Quality.DVD))
            .Should().BeTrue();
        }

        [Test]
        public void should_return_true_for_same_language_when_upgrading_is_allowed()
        {
            Subject.IsUpgradeAllowed(
                new QualityProfile
                {
                    Cutoff = Quality.Bluray1080p.Id,
                    Items = Qualities.QualityFixture.GetDefaultQualities(),
                    UpgradeAllowed = true
                },
                new QualityModel(Quality.DVD),
                new QualityModel(Quality.DVD))
            .Should().BeTrue();
        }

        [Test]
        public void should_return_true_for_same_language_when_upgrading_is_not_allowed()
        {
            Subject.IsUpgradeAllowed(
                new QualityProfile
                {
                    Cutoff = Quality.Bluray1080p.Id,
                    Items = Qualities.QualityFixture.GetDefaultQualities(),
                    UpgradeAllowed = true
                },
                new QualityModel(Quality.DVD),
                new QualityModel(Quality.DVD))
            .Should().BeTrue();
        }

        [Test]
        public void should_return_true_for_lower_language_when_upgrading_is_allowed()
        {
            Subject.IsUpgradeAllowed(
                new QualityProfile
                {
                    Cutoff = Quality.Bluray1080p.Id,
                    Items = Qualities.QualityFixture.GetDefaultQualities(),
                    UpgradeAllowed = true
                },
                new QualityModel(Quality.DVD),
                new QualityModel(Quality.DVD))
            .Should().BeTrue();
        }

        [Test]
        public void should_return_true_for_lower_language_when_upgrading_is_not_allowed()
        {
            Subject.IsUpgradeAllowed(
                new QualityProfile
                {
                    Cutoff = Quality.Bluray1080p.Id,
                    Items = Qualities.QualityFixture.GetDefaultQualities(),
                    UpgradeAllowed = true
                },
                new QualityModel(Quality.DVD),
                new QualityModel(Quality.DVD))
            .Should().BeTrue();
        }

        [Test]
        public void should_return_true_for_quality_upgrade_when_upgrading_is_allowed()
        {
            Subject.IsUpgradeAllowed(
                new QualityProfile
                {
                    Cutoff = Quality.Bluray1080p.Id,
                    Items = Qualities.QualityFixture.GetDefaultQualities(),
                    UpgradeAllowed = true
                },
                new QualityModel(Quality.DVD),
                new QualityModel(Quality.Bluray1080p))
            .Should().BeTrue();
        }

        [Test]
        public void should_return_true_for_same_quality_when_upgrading_is_allowed()
        {
            Subject.IsUpgradeAllowed(
                new QualityProfile
                {
                    Cutoff = Quality.Bluray1080p.Id,
                    Items = Qualities.QualityFixture.GetDefaultQualities(),
                    UpgradeAllowed = true
                },
                new QualityModel(Quality.DVD),
                new QualityModel(Quality.DVD))
            .Should().BeTrue();
        }

        [Test]
        public void should_return_true_for_same_quality_when_upgrading_is_not_allowed()
        {
            Subject.IsUpgradeAllowed(
                new QualityProfile
                {
                    Cutoff = Quality.Bluray1080p.Id,
                    Items = Qualities.QualityFixture.GetDefaultQualities(),
                    UpgradeAllowed = false
                },
                new QualityModel(Quality.DVD),
                new QualityModel(Quality.DVD))
            .Should().BeTrue();
        }

        [Test]
        public void should_return_true_for_lower_quality_when_upgrading_is_allowed()
        {
            Subject.IsUpgradeAllowed(
                new QualityProfile
                {
                    Cutoff = Quality.Bluray1080p.Id,
                    Items = Qualities.QualityFixture.GetDefaultQualities(),
                    UpgradeAllowed = true
                },
                new QualityModel(Quality.DVD),
                new QualityModel(Quality.SDTV))
            .Should().BeTrue();
        }

        [Test]
        public void should_return_true_for_lower_quality_when_upgrading_is_not_allowed()
        {
            Subject.IsUpgradeAllowed(
                new QualityProfile
                {
                    Cutoff = Quality.Bluray1080p.Id,
                    Items = Qualities.QualityFixture.GetDefaultQualities(),
                    UpgradeAllowed = false
                },
                new QualityModel(Quality.DVD),
                new QualityModel(Quality.SDTV))
            .Should().BeTrue();
        }
    }
}
