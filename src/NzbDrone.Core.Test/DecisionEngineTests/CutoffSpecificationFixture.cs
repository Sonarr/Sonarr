using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.CustomFormats;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Test.Languages;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class CutoffSpecificationFixture : CoreTest<CutoffSpecification>
    {
        private CustomFormat _customFormat;
        private RemoteEpisode _remoteMovie;

        [SetUp]
        public void Setup()
        {
            Mocker.SetConstant<IUpgradableSpecification>(Mocker.Resolve<UpgradableSpecification>());

            _remoteMovie = new RemoteEpisode()
            {
                Series = Builder<Series>.CreateNew().Build(),
                Episodes = new List<Episode> { Builder<Episode>.CreateNew().Build() },
                ParsedEpisodeInfo = Builder<ParsedEpisodeInfo>.CreateNew().With(x => x.Quality = null).Build()
            };

            GivenOldCustomFormats(new List<CustomFormat>());
        }

        private void GivenProfile(QualityProfile profile)
        {
            CustomFormatsFixture.GivenCustomFormats();
            profile.FormatItems = CustomFormatsFixture.GetSampleFormatItems();
            profile.MinFormatScore = 0;
            _remoteMovie.Series.QualityProfile = profile;

            Console.WriteLine(profile.ToJson());
        }

        private void GivenFileQuality(QualityModel quality, Language language)
        {
            _remoteMovie.Episodes.First().EpisodeFile = Builder<EpisodeFile>.CreateNew().With(x => x.Quality = quality).With(x => x.Languages = new List<Language> { language }).Build();
        }

        private void GivenNewQuality(QualityModel quality)
        {
            _remoteMovie.ParsedEpisodeInfo.Quality = quality;
        }

        private void GivenOldCustomFormats(List<CustomFormat> formats)
        {
            Mocker.GetMock<ICustomFormatCalculationService>()
                .Setup(x => x.ParseCustomFormat(It.IsAny<EpisodeFile>()))
                .Returns(formats);
        }

        private void GivenNewCustomFormats(List<CustomFormat> formats)
        {
            _remoteMovie.CustomFormats = formats;
        }

        private void GivenCustomFormatHigher()
        {
            _customFormat = new CustomFormat("My Format", new ResolutionSpecification { Value = (int)Resolution.R1080p }) { Id = 1 };

            CustomFormatsFixture.GivenCustomFormats(_customFormat);
        }

        [Test]
        public void should_return_true_if_current_episode_is_less_than_cutoff()
        {
            GivenProfile(new QualityProfile
            {
                Cutoff = Quality.Bluray1080p.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                UpgradeAllowed = true
            });

            GivenFileQuality(new QualityModel(Quality.DVD, new Revision(version: 2)), Language.English);
            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_current_episode_is_equal_to_cutoff()
        {
            GivenProfile(new QualityProfile
            {
                Cutoff = Quality.HDTV720p.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                UpgradeAllowed = true
            });

            GivenFileQuality(new QualityModel(Quality.HDTV720p, new Revision(version: 2)), Language.English);
            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_current_episode_is_greater_than_cutoff()
        {
            GivenProfile(new QualityProfile
            {
                Cutoff = Quality.HDTV720p.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                UpgradeAllowed = true
            });

            GivenFileQuality(new QualityModel(Quality.Bluray1080p, new Revision(version: 2)), Language.English);
            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_new_episode_is_proper_but_existing_is_not()
        {
            GivenProfile(new QualityProfile
            {
                Cutoff = Quality.HDTV720p.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                UpgradeAllowed = true
            });

            GivenFileQuality(new QualityModel(Quality.HDTV720p, new Revision(version: 1)), Language.English);
            GivenNewQuality(new QualityModel(Quality.HDTV720p, new Revision(version: 2)));
            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_cutoff_is_met_and_quality_is_higher()
        {
            GivenProfile(new QualityProfile
            {
                Cutoff = Quality.HDTV720p.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                UpgradeAllowed = true
            });

            GivenFileQuality(new QualityModel(Quality.HDTV720p, new Revision(version: 2)), Language.English);
            GivenNewQuality(new QualityModel(Quality.Bluray1080p, new Revision(version: 2)));
            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_quality_cutoff_is_met_and_quality_is_higher_but_language_is_met()
        {
            GivenProfile(new QualityProfile
            {
                Cutoff = Quality.HDTV720p.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                UpgradeAllowed = true
            });

            GivenFileQuality(new QualityModel(Quality.HDTV720p, new Revision(version: 2)), Language.Spanish);
            GivenNewQuality(new QualityModel(Quality.Bluray1080p, new Revision(version: 2)));
            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_cutoff_is_met_and_quality_is_higher_and_language_is_higher()
        {
            GivenProfile(new QualityProfile
            {
                Cutoff = Quality.HDTV720p.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                UpgradeAllowed = true
            });

            GivenFileQuality(new QualityModel(Quality.HDTV720p, new Revision(version: 2)), Language.French);
            GivenNewQuality(new QualityModel(Quality.Bluray1080p, new Revision(version: 2)));
            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_cutoff_is_not_met_and_new_quality_is_higher_and_language_is_higher()
        {
            GivenProfile(new QualityProfile
            {
                Cutoff = Quality.HDTV720p.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                UpgradeAllowed = true
            });

            GivenFileQuality(new QualityModel(Quality.SDTV, new Revision(version: 2)), Language.French);
            GivenNewQuality(new QualityModel(Quality.Bluray1080p, new Revision(version: 2)));
            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_cutoff_is_not_met_and_language_is_higher()
        {
            GivenProfile(new QualityProfile
            {
                Cutoff = Quality.HDTV720p.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                UpgradeAllowed = true
            });

            GivenFileQuality(new QualityModel(Quality.SDTV, new Revision(version: 2)), Language.French);
            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_custom_formats_is_met_and_quality_and_format_higher()
        {
            GivenProfile(new QualityProfile
            {
                Cutoff = Quality.HDTV720p.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                MinFormatScore = 0,
                FormatItems = CustomFormatsFixture.GetSampleFormatItems("My Format"),
                UpgradeAllowed = true
            });

            GivenFileQuality(new QualityModel(Quality.HDTV720p), Language.English);
            GivenNewQuality(new QualityModel(Quality.Bluray1080p));

            GivenCustomFormatHigher();

            GivenOldCustomFormats(new List<CustomFormat>());
            GivenNewCustomFormats(new List<CustomFormat> { _customFormat });

            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_cutoffs_are_met_but_is_a_revision_upgrade()
        {
            GivenProfile(new QualityProfile
            {
                Cutoff = Quality.HDTV1080p.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                UpgradeAllowed = true
            });

            GivenFileQuality(new QualityModel(Quality.WEBDL1080p, new Revision(version: 1)), Language.English);
            GivenNewQuality(new QualityModel(Quality.WEBDL1080p, new Revision(version: 2)));

            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_quality_profile_does_not_allow_upgrades_but_cutoff_is_set_to_highest_quality()
        {
            GivenProfile(new QualityProfile
            {
                Cutoff = Quality.RAWHD.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                UpgradeAllowed = false
            });

            GivenFileQuality(new QualityModel(Quality.WEBDL1080p), Language.English);
            GivenNewQuality(new QualityModel(Quality.Bluray1080p));

            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeFalse();
        }
    }
}
