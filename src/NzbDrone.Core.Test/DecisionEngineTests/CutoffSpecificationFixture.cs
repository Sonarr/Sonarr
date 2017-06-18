using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Profiles.Languages;
using NzbDrone.Core.Test.Languages;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class CutoffSpecificationFixture : CoreTest<UpgradableSpecification>
    {
        [Test]
        public void should_return_true_if_current_episode_is_less_than_cutoff()
        {
            Subject.CutoffNotMet(
                new Profile 
                { 
                    Cutoff = Quality.Bluray1080p.Id, 
                    Items = Qualities.QualityFixture.GetDefaultQualities()
                },
                new LanguageProfile
                {
                    Languages = LanguageFixture.GetDefaultLanguages(Language.English),
                    Cutoff = Language.English
                },
                new QualityModel(Quality.DVD, new Revision(version: 2)), Language.English).Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_current_episode_is_equal_to_cutoff()
        {
            Subject.CutoffNotMet(
                new Profile
                {
                    Cutoff = Quality.HDTV720p.Id, 
                    Items = Qualities.QualityFixture.GetDefaultQualities()
                },
                new LanguageProfile
                {
                    Languages = LanguageFixture.GetDefaultLanguages(Language.English),
                    Cutoff = Language.English
                },
                new QualityModel(Quality.HDTV720p, new Revision(version: 2)), Language.English).Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_current_episode_is_greater_than_cutoff()
        {
            Subject.CutoffNotMet(
                new Profile 
                { 
                    Cutoff = Quality.HDTV720p.Id, 
                    Items = Qualities.QualityFixture.GetDefaultQualities()
                },
                new LanguageProfile
                {
                    Languages = LanguageFixture.GetDefaultLanguages(Language.English),
                    Cutoff = Language.English
                },
                new QualityModel(Quality.Bluray1080p, new Revision(version: 2)), Language.English).Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_new_episode_is_proper_but_existing_is_not()
        {
            Subject.CutoffNotMet(
                new Profile 
                { 
                    Cutoff = Quality.HDTV720p.Id, 
                    Items = Qualities.QualityFixture.GetDefaultQualities()
                },
                new LanguageProfile
                {
                    Languages = LanguageFixture.GetDefaultLanguages(Language.English),
                    Cutoff = Language.English
                },
                new QualityModel(Quality.HDTV720p, new Revision(version: 1)),
                Language.English,
                new QualityModel(Quality.HDTV720p, new Revision(version: 2))).Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_cutoff_is_met_and_quality_is_higher()
        {
            Subject.CutoffNotMet(
                new Profile 
                {
                    Cutoff = Quality.HDTV720p.Id, 
                    Items = Qualities.QualityFixture.GetDefaultQualities()
                },
                new LanguageProfile
                {
                    Languages = LanguageFixture.GetDefaultLanguages(Language.English),
                    Cutoff = Language.English
                },
                new QualityModel(Quality.HDTV720p, new Revision(version: 2)),
                Language.English,
                new QualityModel(Quality.Bluray1080p, new Revision(version: 2))).Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_quality_cutoff_is_met_and_quality_is_higher_but_language_is_not_met()
        {

            Profile _profile = new Profile
                {
                    Cutoff = Quality.HDTV720p.Id,
                    Items = Qualities.QualityFixture.GetDefaultQualities(),
                };

            LanguageProfile _langProfile = new LanguageProfile
                {
                    Cutoff = Language.Spanish,
                    Languages = LanguageFixture.GetDefaultLanguages()
                };
            
            Subject.CutoffNotMet(_profile,
                _langProfile,
                 new QualityModel(Quality.HDTV720p, new Revision(version: 2)),
                 Language.English,
                 new QualityModel(Quality.Bluray1080p, new Revision(version: 2))).Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_cutoff_is_met_and_quality_is_higher_and_language_is_met()
        {

            Profile _profile = new Profile
            {
                Cutoff = Quality.HDTV720p.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
            };

            LanguageProfile _langProfile = new LanguageProfile
            {
                Cutoff = Language.Spanish,
                Languages = LanguageFixture.GetDefaultLanguages()
            };

            Subject.CutoffNotMet(
                _profile,
                _langProfile,
                new QualityModel(Quality.HDTV720p, new Revision(version: 2)),
                Language.Spanish,
                new QualityModel(Quality.Bluray1080p, new Revision(version: 2))).Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_cutoff_is_met_and_quality_is_higher_and_language_is_higher()
        {

            Profile _profile = new Profile
            {
                Cutoff = Quality.HDTV720p.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
            };

            LanguageProfile _langProfile = new LanguageProfile
            {
                Cutoff = Language.Spanish,
                Languages = LanguageFixture.GetDefaultLanguages()
            };

            Subject.CutoffNotMet(
                _profile,
                _langProfile,
                new QualityModel(Quality.HDTV720p, new Revision(version: 2)),
                Language.French,
                new QualityModel(Quality.Bluray1080p, new Revision(version: 2))).Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_cutoff_is_not_met_and_new_quality_is_higher_and_language_is_higher()
        {

            Profile _profile = new Profile
            {
                Cutoff = Quality.HDTV720p.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
            };

            LanguageProfile _langProfile = new LanguageProfile
            {
                Cutoff = Language.Spanish,
                Languages = LanguageFixture.GetDefaultLanguages()
            };

            Subject.CutoffNotMet(
                _profile,
                _langProfile,
                new QualityModel(Quality.SDTV, new Revision(version: 2)),
                Language.French,
                new QualityModel(Quality.Bluray1080p, new Revision(version: 2))).Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_cutoff_is_not_met_and_language_is_higher()
        {

            Profile _profile = new Profile
            {
                Cutoff = Quality.HDTV720p.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
            };

            LanguageProfile _langProfile = new LanguageProfile
            {
                Cutoff = Language.Spanish,
                Languages = LanguageFixture.GetDefaultLanguages()
            };

            Subject.CutoffNotMet(
                _profile,
                _langProfile,
                new QualityModel(Quality.SDTV, new Revision(version: 2)),
                Language.French).Should().BeTrue();
        }
    }
}
