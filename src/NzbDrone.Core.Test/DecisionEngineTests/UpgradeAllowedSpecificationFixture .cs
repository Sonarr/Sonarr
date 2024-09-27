using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class UpgradeAllowedSpecificationFixture : CoreTest<UpgradableSpecification>
    {
        private CustomFormat _customFormatOne;
        private CustomFormat _customFormatTwo;
        private QualityProfile _qualityProfile;

        [SetUp]
        public void Setup()
        {
            _customFormatOne = new CustomFormat
            {
                Id = 1,
                Name = "One"
            };
            _customFormatTwo = new CustomFormat
            {
                Id = 2,
                Name = "Two"
            };

            _qualityProfile = new QualityProfile
            {
                Cutoff = Quality.Bluray1080p.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                UpgradeAllowed = false,
                CutoffFormatScore = 100,
                FormatItems = new List<ProfileFormatItem>
                {
                    new ProfileFormatItem
                    {
                        Format = _customFormatOne,
                        Score = 50
                    },
                    new ProfileFormatItem
                    {
                        Format = _customFormatTwo,
                        Score = 100
                    }
                }
            };
        }

        [Test]
        public void should_return_false_when_quality_is_better_custom_formats_are_the_same_and_upgrading_is_not_allowed()
        {
            _qualityProfile.UpgradeAllowed = false;

            Subject.IsUpgradeAllowed(
                _qualityProfile,
                new QualityModel(Quality.DVD),
                new List<CustomFormat>(),
                new QualityModel(Quality.Bluray1080p),
                new List<CustomFormat>())
            .Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_quality_is_same_and_custom_format_is_upgrade_and_upgrading_is_not_allowed()
        {
            _qualityProfile.UpgradeAllowed = false;

            Subject.IsUpgradeAllowed(
                    _qualityProfile,
                    new QualityModel(Quality.DVD),
                    new List<CustomFormat> { _customFormatOne },
                    new QualityModel(Quality.DVD),
                    new List<CustomFormat> { _customFormatTwo })
                .Should().BeFalse();
        }

        [Test]
        public void should_return_true_for_custom_format_upgrade_when_upgrading_is_allowed()
        {
            _qualityProfile.UpgradeAllowed = true;

            Subject.IsUpgradeAllowed(
                _qualityProfile,
                new QualityModel(Quality.DVD),
                new List<CustomFormat> { _customFormatOne },
                new QualityModel(Quality.DVD),
                new List<CustomFormat> { _customFormatTwo })
            .Should().BeTrue();
        }

        [Test]
        public void should_return_true_for_same_custom_format_score_when_upgrading_is_not_allowed()
        {
            _qualityProfile.UpgradeAllowed = false;

            Subject.IsUpgradeAllowed(
                _qualityProfile,
                new QualityModel(Quality.DVD),
                new List<CustomFormat> { _customFormatOne },
                new QualityModel(Quality.DVD),
                new List<CustomFormat> { _customFormatOne })
            .Should().BeTrue();
        }

        [Test]
        public void should_return_true_for_lower_custom_format_score_when_upgrading_is_allowed()
        {
            _qualityProfile.UpgradeAllowed = true;

            Subject.IsUpgradeAllowed(
                _qualityProfile,
                new QualityModel(Quality.DVD),
                new List<CustomFormat> { _customFormatTwo },
                new QualityModel(Quality.DVD),
                new List<CustomFormat> { _customFormatOne })
            .Should().BeTrue();
        }

        [Test]
        public void should_return_true_for_lower_language_when_upgrading_is_not_allowed()
        {
            _qualityProfile.UpgradeAllowed = false;

            Subject.IsUpgradeAllowed(
                _qualityProfile,
                new QualityModel(Quality.DVD),
                new List<CustomFormat> { _customFormatTwo },
                new QualityModel(Quality.DVD),
                new List<CustomFormat> { _customFormatOne })
            .Should().BeTrue();
        }

        [Test]
        public void should_return_true_for_quality_upgrade_when_upgrading_is_allowed()
        {
            _qualityProfile.UpgradeAllowed = true;

            Subject.IsUpgradeAllowed(
                _qualityProfile,
                new QualityModel(Quality.DVD),
                new List<CustomFormat>(),
                new QualityModel(Quality.Bluray1080p),
                new List<CustomFormat>())
            .Should().BeTrue();
        }

        [Test]
        public void should_return_true_for_same_quality_when_upgrading_is_allowed()
        {
            _qualityProfile.UpgradeAllowed = true;

            Subject.IsUpgradeAllowed(
                _qualityProfile,
                new QualityModel(Quality.DVD),
                new List<CustomFormat>(),
                new QualityModel(Quality.DVD),
                new List<CustomFormat>())
            .Should().BeTrue();
        }

        [Test]
        public void should_return_true_for_same_quality_when_upgrading_is_not_allowed()
        {
            _qualityProfile.UpgradeAllowed = false;

            Subject.IsUpgradeAllowed(
                _qualityProfile,
                new QualityModel(Quality.DVD),
                new List<CustomFormat>(),
                new QualityModel(Quality.DVD),
                new List<CustomFormat>())
            .Should().BeTrue();
        }

        [Test]
        public void should_return_true_for_lower_quality_when_upgrading_is_allowed()
        {
            _qualityProfile.UpgradeAllowed = true;

            Subject.IsUpgradeAllowed(
                _qualityProfile,
                new QualityModel(Quality.DVD),
                new List<CustomFormat>(),
                new QualityModel(Quality.SDTV),
                new List<CustomFormat>())
            .Should().BeTrue();
        }

        [Test]
        public void should_return_true_for_lower_quality_when_upgrading_is_not_allowed()
        {
            _qualityProfile.UpgradeAllowed = false;

            Subject.IsUpgradeAllowed(
                _qualityProfile,
                new QualityModel(Quality.DVD),
                new List<CustomFormat>(),
                new QualityModel(Quality.SDTV),
                new List<CustomFormat>())
            .Should().BeTrue();
        }

        [Test]
        public void should_returntrue_when_quality_is_revision_upgrade_for_same_quality()
        {
            _qualityProfile.UpgradeAllowed = false;

            Subject.IsUpgradeAllowed(
                    _qualityProfile,
                    new QualityModel(Quality.DVD, new Revision(1)),
                    new List<CustomFormat> { _customFormatOne },
                    new QualityModel(Quality.DVD, new Revision(2)),
                    new List<CustomFormat> { _customFormatOne })
                .Should().BeTrue();
        }
    }
}
