using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.DecisionEngine;
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
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class UpgradeDiskSpecificationFixture : CoreTest<UpgradeDiskSpecification>
    {
        private UpgradeDiskSpecification _upgradeDisk;

        private RemoteEpisode _parseResultMulti;
        private RemoteEpisode _parseResultSingle;
        private EpisodeFile _firstFile;
        private EpisodeFile _secondFile;

        [SetUp]
        public void Setup()
        {
            Mocker.Resolve<UpgradableSpecification>();
            _upgradeDisk = Mocker.Resolve<UpgradeDiskSpecification>();

            CustomFormatsTestHelpers.GivenCustomFormats();

            _firstFile = new EpisodeFile { Quality = new QualityModel(Quality.Bluray1080p, new Revision(version: 2)), DateAdded = DateTime.Now, Languages = new List<Language> { Language.English } };
            _secondFile = new EpisodeFile { Quality = new QualityModel(Quality.Bluray1080p, new Revision(version: 2)), DateAdded = DateTime.Now, Languages = new List<Language> { Language.English } };

            var singleEpisodeList = new List<Episode> { new Episode { EpisodeFile = _firstFile, EpisodeFileId = 1 }, new Episode { EpisodeFile = null } };
            var doubleEpisodeList = new List<Episode> { new Episode { EpisodeFile = _firstFile, EpisodeFileId = 1 }, new Episode { EpisodeFile = _secondFile, EpisodeFileId = 1 }, new Episode { EpisodeFile = null } };

            var fakeSeries = Builder<Series>.CreateNew()
                .With(c => c.QualityProfile = new QualityProfile
                {
                    UpgradeAllowed = true,
                    Cutoff = Quality.Bluray1080p.Id,
                    Items = Qualities.QualityFixture.GetDefaultQualities(),
                    FormatItems = CustomFormatsTestHelpers.GetSampleFormatItems("None"),
                    MinFormatScore = 0,
                })
                .Build();

            _parseResultMulti = new RemoteEpisode
            {
                Series = fakeSeries,
                ParsedEpisodeInfo = new ParsedEpisodeInfo { Quality = new QualityModel(Quality.DVD, new Revision(version: 2)), Languages = new List<Language> { Language.English } },
                Episodes = doubleEpisodeList,
                CustomFormats = new List<CustomFormat>()
            };

            _parseResultSingle = new RemoteEpisode
            {
                Series = fakeSeries,
                ParsedEpisodeInfo = new ParsedEpisodeInfo { Quality = new QualityModel(Quality.DVD, new Revision(version: 2)), Languages = new List<Language> { Language.English } },
                Episodes = singleEpisodeList,
                CustomFormats = new List<CustomFormat>()
            };

            Mocker.GetMock<ICustomFormatCalculationService>()
                  .Setup(x => x.ParseCustomFormat(It.IsAny<EpisodeFile>()))
                  .Returns(new List<CustomFormat>());
        }

        private void GivenProfile(QualityProfile profile)
        {
            CustomFormatsTestHelpers.GivenCustomFormats();
            profile.FormatItems = CustomFormatsTestHelpers.GetSampleFormatItems();
            profile.MinFormatScore = 0;
            _parseResultMulti.Series.QualityProfile = profile;
            _parseResultSingle.Series.QualityProfile = profile;

            Console.WriteLine(profile.ToJson());
        }

        private void GivenFileQuality(QualityModel quality)
        {
            _firstFile.Quality = quality;
            _secondFile.Quality = quality;
        }

        private void GivenNewQuality(QualityModel quality)
        {
            _parseResultMulti.ParsedEpisodeInfo.Quality = quality;
            _parseResultSingle.ParsedEpisodeInfo.Quality = quality;
        }

        private void GivenOldCustomFormats(List<CustomFormat> formats)
        {
            Mocker.GetMock<ICustomFormatCalculationService>()
                .Setup(x => x.ParseCustomFormat(It.IsAny<EpisodeFile>()))
                .Returns(formats);
        }

        private void GivenNewCustomFormats(List<CustomFormat> formats)
        {
            _parseResultMulti.CustomFormats = formats;
            _parseResultSingle.CustomFormats = formats;
        }

        private void WithFirstFileUpgradable()
        {
            _firstFile.Quality = new QualityModel(Quality.SDTV);
        }

        private void WithSecondFileUpgradable()
        {
            _secondFile.Quality = new QualityModel(Quality.SDTV);
        }

        [Test]
        public void should_return_true_if_episode_has_no_existing_file()
        {
            _parseResultSingle.Episodes.ForEach(c => c.EpisodeFileId = 0);
            _upgradeDisk.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_single_episode_doesnt_exist_on_disk()
        {
            _parseResultSingle.Episodes = new List<Episode>();

            _upgradeDisk.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_upgradable_if_only_episode_is_upgradable()
        {
            WithFirstFileUpgradable();
            _upgradeDisk.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_upgradable_if_both_episodes_are_upgradable()
        {
            WithFirstFileUpgradable();
            WithSecondFileUpgradable();
            _upgradeDisk.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_not_upgradable_if_both_episodes_are_not_upgradable()
        {
            _upgradeDisk.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_not_upgradable_if_only_first_episodes_is_upgradable()
        {
            WithFirstFileUpgradable();
            _upgradeDisk.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_not_upgradable_if_only_second_episodes_is_upgradable()
        {
            WithSecondFileUpgradable();
            _upgradeDisk.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_not_be_upgradable_if_qualities_are_the_same()
        {
            _firstFile.Quality = new QualityModel(Quality.WEBDL1080p);
            _parseResultSingle.ParsedEpisodeInfo.Quality = new QualityModel(Quality.WEBDL1080p);
            _upgradeDisk.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_not_be_upgradable_if_revision_downgrade_and_preferred_word_upgrade_if_propers_are_preferred()
        {
            Mocker.GetMock<ICustomFormatCalculationService>()
                  .Setup(s => s.ParseCustomFormat(It.IsAny<EpisodeFile>()))
                  .Returns(new List<CustomFormat>());

            _parseResultSingle.CustomFormatScore = 10;

            _firstFile.Quality = new QualityModel(Quality.WEBDL1080p, new Revision(2));
            _parseResultSingle.ParsedEpisodeInfo.Quality = new QualityModel(Quality.WEBDL1080p);
            _upgradeDisk.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeFalse();
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

            GivenFileQuality(new QualityModel(Quality.HDTV720p, new Revision(version: 2)));
            Subject.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeFalse();
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

            GivenFileQuality(new QualityModel(Quality.Bluray1080p, new Revision(version: 2)));
            Subject.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeFalse();
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

            GivenFileQuality(new QualityModel(Quality.HDTV720p, new Revision(version: 1)));
            GivenNewQuality(new QualityModel(Quality.HDTV720p, new Revision(version: 2)));
            Subject.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeTrue();
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

            GivenFileQuality(new QualityModel(Quality.HDTV720p, new Revision(version: 2)));
            GivenNewQuality(new QualityModel(Quality.Bluray1080p, new Revision(version: 2)));
            Subject.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeFalse();
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

            GivenFileQuality(new QualityModel(Quality.HDTV720p, new Revision(version: 2)));
            GivenNewQuality(new QualityModel(Quality.Bluray1080p, new Revision(version: 2)));
            Subject.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeFalse();
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

            GivenFileQuality(new QualityModel(Quality.HDTV720p, new Revision(version: 2)));
            GivenNewQuality(new QualityModel(Quality.Bluray1080p, new Revision(version: 2)));
            Subject.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeFalse();
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

            GivenFileQuality(new QualityModel(Quality.SDTV, new Revision(version: 2)));
            GivenNewQuality(new QualityModel(Quality.Bluray1080p, new Revision(version: 2)));
            Subject.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeTrue();
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

            GivenFileQuality(new QualityModel(Quality.SDTV, new Revision(version: 2)));
            Subject.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_custom_formats_is_met_and_quality_and_format_higher()
        {
            var customFormat = new CustomFormat("My Format", new ResolutionSpecification { Value = (int)Resolution.R1080p }) { Id = 1 };

            GivenProfile(new QualityProfile
            {
                Cutoff = Quality.HDTV720p.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                MinFormatScore = 0,
                FormatItems = CustomFormatsTestHelpers.GetSampleFormatItems("My Format"),
                UpgradeAllowed = true
            });

            GivenFileQuality(new QualityModel(Quality.HDTV720p));
            GivenNewQuality(new QualityModel(Quality.Bluray1080p));

            GivenOldCustomFormats(new List<CustomFormat>());
            GivenNewCustomFormats(new List<CustomFormat> { customFormat });

            Subject.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeFalse();
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

            GivenFileQuality(new QualityModel(Quality.WEBDL1080p, new Revision(version: 1)));
            GivenNewQuality(new QualityModel(Quality.WEBDL1080p, new Revision(version: 2)));

            Subject.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeTrue();
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

            GivenFileQuality(new QualityModel(Quality.WEBDL1080p));
            GivenNewQuality(new QualityModel(Quality.Bluray1080p));

            Subject.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_quality_profile_does_not_allow_upgrades_but_format_cutoff_is_above_current_score()
        {
            var customFormat = new CustomFormat("My Format", new ResolutionSpecification { Value = (int)Resolution.R1080p }) { Id = 1 };

            GivenProfile(new QualityProfile
            {
                Cutoff = Quality.SDTV.Id,
                MinFormatScore = 0,
                CutoffFormatScore = 10000,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                FormatItems = CustomFormatsTestHelpers.GetSampleFormatItems("My Format"),
                UpgradeAllowed = false
            });

            _parseResultSingle.Series.QualityProfile.Value.FormatItems = new List<ProfileFormatItem>
            {
                new ProfileFormatItem
                {
                    Format = customFormat,
                    Score = 50
                }
            };

            GivenFileQuality(new QualityModel(Quality.WEBDL1080p));
            GivenNewQuality(new QualityModel(Quality.WEBDL1080p));

            GivenOldCustomFormats(new List<CustomFormat>());
            GivenNewCustomFormats(new List<CustomFormat> { customFormat });

            Subject.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_quality_profile_does_not_allow_upgrades_but_format_cutoff_is_above_current_score_and_is_revision_upgrade()
        {
            var customFormat = new CustomFormat("My Format", new ResolutionSpecification { Value = (int)Resolution.R1080p }) { Id = 1 };

            Mocker.GetMock<IConfigService>()
                .SetupGet(s => s.DownloadPropersAndRepacks)
                .Returns(ProperDownloadTypes.DoNotPrefer);

            GivenProfile(new QualityProfile
            {
                Cutoff = Quality.SDTV.Id,
                MinFormatScore = 0,
                CutoffFormatScore = 10000,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                FormatItems = CustomFormatsTestHelpers.GetSampleFormatItems("My Format"),
                UpgradeAllowed = false
            });

            _parseResultSingle.Series.QualityProfile.Value.FormatItems = new List<ProfileFormatItem>
            {
                new ProfileFormatItem
                {
                    Format = customFormat,
                    Score = 50
                }
            };

            GivenFileQuality(new QualityModel(Quality.WEBDL1080p, new Revision(version: 1)));
            GivenNewQuality(new QualityModel(Quality.WEBDL1080p, new Revision(version: 2)));

            GivenOldCustomFormats(new List<CustomFormat>());
            GivenNewCustomFormats(new List<CustomFormat> { customFormat });

            Subject.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_reject_season_pack_when_mode_is_all_and_not_all_are_upgradable()
        {
            GivenProfile(new QualityProfile
            {
                Cutoff = Quality.Bluray1080p.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                UpgradeAllowed = true
            });

            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.SeasonPackUpgrade)
                  .Returns(SeasonPackUpgradeType.All);

            _parseResultMulti.ParsedEpisodeInfo.FullSeason = true;
            _parseResultMulti.Episodes = new List<Episode>
                                         {
                                             new Episode { EpisodeFile = new EpisodeFile { Quality = new QualityModel(Quality.SDTV) }, EpisodeFileId = 1 },
                                             new Episode { EpisodeFile = new EpisodeFile { Quality = new QualityModel(Quality.Bluray1080p) }, EpisodeFileId = 2 }
                                         };

            _parseResultMulti.ParsedEpisodeInfo.Quality = new QualityModel(Quality.Bluray1080p);

            var result = Subject.IsSatisfiedBy(_parseResultMulti, new());

            result.Accepted.Should().BeFalse();
        }

        [Test]
        public void should_reject_for_season_pack_not_meeting_threshold()
        {
            GivenProfile(new QualityProfile
            {
                Cutoff = Quality.Bluray1080p.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                UpgradeAllowed = true
            });

            Mocker.GetMock<IConfigService>()
                .SetupGet(s => s.SeasonPackUpgrade)
                .Returns(SeasonPackUpgradeType.Threshold);

            Mocker.GetMock<IConfigService>()
                .SetupGet(s => s.SeasonPackUpgradeThreshold)
                .Returns(90);

            _parseResultMulti.ParsedEpisodeInfo.FullSeason = true;
            _parseResultMulti.Episodes = new List<Episode>
                                         {
                                             new Episode { EpisodeFile = new EpisodeFile { Quality = new QualityModel(Quality.SDTV) }, EpisodeFileId = 1 },
                                             new Episode { EpisodeFile = new EpisodeFile { Quality = new QualityModel(Quality.SDTV) }, EpisodeFileId = 2 },
                                             new Episode { EpisodeFile = new EpisodeFile { Quality = new QualityModel(Quality.SDTV) }, EpisodeFileId = 3 },
                                             new Episode { EpisodeFile = new EpisodeFile { Quality = new QualityModel(Quality.SDTV) }, EpisodeFileId = 4 },
                                             new Episode { EpisodeFile = new EpisodeFile { Quality = new QualityModel(Quality.SDTV) }, EpisodeFileId = 5 },
                                             new Episode { EpisodeFile = new EpisodeFile { Quality = new QualityModel(Quality.SDTV) }, EpisodeFileId = 6 },
                                             new Episode { EpisodeFile = new EpisodeFile { Quality = new QualityModel(Quality.SDTV) }, EpisodeFileId = 7 },
                                             new Episode { EpisodeFile = new EpisodeFile { Quality = new QualityModel(Quality.Bluray1080p) }, EpisodeFileId = 8 },
                                             new Episode { EpisodeFile = new EpisodeFile { Quality = new QualityModel(Quality.Bluray1080p) }, EpisodeFileId = 9 },
                                             new Episode { EpisodeFile = null, EpisodeFileId = 0 }
                                         };

            _parseResultMulti.ParsedEpisodeInfo.Quality = new QualityModel(Quality.Bluray1080p);

            var result = Subject.IsSatisfiedBy(_parseResultMulti, new());

            result.Accepted.Should().BeFalse();
            result.Reason.Should().Be(DownloadRejectionReason.DiskNotUpgrade);
        }

        [Test]
        public void should_accept_season_pack_when_mode_is_any_and_at_least_one_upgradable()
        {
            GivenProfile(new QualityProfile
            {
                Cutoff = Quality.Bluray1080p.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                UpgradeAllowed = true
            });

            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.SeasonPackUpgrade)
                  .Returns(SeasonPackUpgradeType.Any);

            _parseResultMulti.ParsedEpisodeInfo.FullSeason = true;
            _parseResultMulti.Episodes = new List<Episode>
                                         {
                                             new Episode { EpisodeFile = new EpisodeFile { Quality = new QualityModel(Quality.SDTV) }, EpisodeFileId = 1 },
                                             new Episode { EpisodeFile = new EpisodeFile { Quality = new QualityModel(Quality.Bluray1080p) }, EpisodeFileId = 2 }
                                         };

            _parseResultMulti.ParsedEpisodeInfo.Quality = new QualityModel(Quality.Bluray1080p);

            var result = Subject.IsSatisfiedBy(_parseResultMulti, new());

            result.Accepted.Should().BeTrue();
        }
    }
}
