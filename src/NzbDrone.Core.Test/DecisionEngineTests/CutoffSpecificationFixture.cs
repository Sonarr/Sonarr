using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Profiles.Languages;
using NzbDrone.Core.Test.Languages;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class CutoffSpecificationFixture : CoreTest<UpgradableSpecification>
    {
        private static readonly int NoPreferredWordScore = 0;

        [Test]
        public void should_return_true_if_current_episode_is_less_than_cutoff()
        {
            Subject.CutoffNotMet(
                new QualityProfile 
                { 
                    Cutoff = Quality.Bluray1080p.Id, 
                    Items = Qualities.QualityFixture.GetDefaultQualities()
                },
                new LanguageProfile
                {
                    Languages = LanguageFixture.GetDefaultLanguages(Language.English),
                    Cutoff = Language.English
                },
                new QualityModel(Quality.DVD, new Revision(version: 2)),
                Language.English,
                NoPreferredWordScore).Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_current_episode_is_equal_to_cutoff()
        {
            Subject.CutoffNotMet(
                new QualityProfile
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
                NoPreferredWordScore).Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_current_episode_is_greater_than_cutoff()
        {
            Subject.CutoffNotMet(
                new QualityProfile 
                { 
                    Cutoff = Quality.HDTV720p.Id, 
                    Items = Qualities.QualityFixture.GetDefaultQualities()
                },
                new LanguageProfile
                {
                    Languages = LanguageFixture.GetDefaultLanguages(Language.English),
                    Cutoff = Language.English
                },
                new QualityModel(Quality.Bluray1080p, new Revision(version: 2)),
                Language.English,
                NoPreferredWordScore).Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_new_episode_is_proper_but_existing_is_not()
        {
            Subject.CutoffNotMet(
                new QualityProfile 
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
                NoPreferredWordScore,
                new QualityModel(Quality.HDTV720p, new Revision(version: 2)),
                NoPreferredWordScore).Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_cutoff_is_met_and_quality_is_higher()
        {
            Subject.CutoffNotMet(
                new QualityProfile 
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
                NoPreferredWordScore,
                new QualityModel(Quality.Bluray1080p, new Revision(version: 2)),
                NoPreferredWordScore).Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_quality_cutoff_is_met_and_quality_is_higher_but_language_is_not_met()
        {
            QualityProfile _profile = new QualityProfile
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
                 NoPreferredWordScore,
                 new QualityModel(Quality.Bluray1080p, new Revision(version: 2)),
                 NoPreferredWordScore).Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_cutoff_is_met_and_quality_is_higher_and_language_is_met()
        {
            QualityProfile _profile = new QualityProfile
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
                NoPreferredWordScore,
                new QualityModel(Quality.Bluray1080p, new Revision(version: 2)),
                NoPreferredWordScore).Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_cutoff_is_met_and_quality_is_higher_and_language_is_higher()
        {
            QualityProfile _profile = new QualityProfile
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
                NoPreferredWordScore,
                new QualityModel(Quality.Bluray1080p, new Revision(version: 2)),
                NoPreferredWordScore).Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_cutoff_is_not_met_and_new_quality_is_higher_and_language_is_higher()
        {
            QualityProfile _profile = new QualityProfile
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
                NoPreferredWordScore,
                new QualityModel(Quality.Bluray1080p, new Revision(version: 2)),
                NoPreferredWordScore).Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_cutoff_is_not_met_and_language_is_higher()
        {
            QualityProfile _profile = new QualityProfile
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
                NoPreferredWordScore).Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_cutoffs_are_met_and_score_is_higher()
        {
            QualityProfile _profile = new QualityProfile
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
                NoPreferredWordScore,
                new QualityModel(Quality.Bluray1080p, new Revision(version: 2)),
                10).Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_cutoffs_are_met_but_is_a_revision_upgrade()
        {
            QualityProfile _profile = new QualityProfile
                                      {
                                          Cutoff = Quality.HDTV1080p.Id,
                                          Items = Qualities.QualityFixture.GetDefaultQualities(),
                                      };

            LanguageProfile _langProfile = new LanguageProfile
                                           {
                                               Cutoff = Language.English,
                                               Languages = LanguageFixture.GetDefaultLanguages()
                                           };

            Subject.CutoffNotMet(
                _profile,
                _langProfile,
                new QualityModel(Quality.WEBDL1080p, new Revision(version: 1)),
                Language.English,
                NoPreferredWordScore,
                new QualityModel(Quality.WEBDL1080p, new Revision(version: 2)),
                NoPreferredWordScore).Should().BeTrue();
        }
    }
}
