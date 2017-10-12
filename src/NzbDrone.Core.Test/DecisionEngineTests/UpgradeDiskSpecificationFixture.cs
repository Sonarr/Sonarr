using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Profiles.Languages;

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

            _firstFile = new EpisodeFile { Quality = new QualityModel(Quality.Bluray1080p, new Revision(version: 2)), DateAdded = DateTime.Now, Language = Language.English };
            _secondFile = new EpisodeFile { Quality = new QualityModel(Quality.Bluray1080p, new Revision(version: 2)), DateAdded = DateTime.Now, Language = Language.English };

            var singleEpisodeList = new List<Episode> { new Episode { EpisodeFile = _firstFile, EpisodeFileId = 1 }, new Episode { EpisodeFile = null } };
            var doubleEpisodeList = new List<Episode> { new Episode { EpisodeFile = _firstFile, EpisodeFileId = 1 }, new Episode { EpisodeFile = _secondFile, EpisodeFileId = 1 }, new Episode { EpisodeFile = null } };

            var languages = Languages.LanguageFixture.GetDefaultLanguages(Language.English, Language.Spanish);

            var fakeSeries = Builder<Series>.CreateNew()
                         .With(c => c.Profile = new Profile { Cutoff = Quality.Bluray1080p.Id, Items = Qualities.QualityFixture.GetDefaultQualities()})
                         .With(l => l.LanguageProfile = new LanguageProfile { Cutoff = Language.Spanish, Languages = languages })
                         .Build();

            _parseResultMulti = new RemoteEpisode
            {
                Series = fakeSeries,
                ParsedEpisodeInfo = new ParsedEpisodeInfo { Quality = new QualityModel(Quality.DVD, new Revision(version: 2)), Language = Language.English },
                Episodes = doubleEpisodeList
            };

            _parseResultSingle = new RemoteEpisode
            {
                Series = fakeSeries,
                ParsedEpisodeInfo = new ParsedEpisodeInfo { Quality = new QualityModel(Quality.DVD, new Revision(version: 2)), Language = Language.English },
                Episodes = singleEpisodeList
            };
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
            _upgradeDisk.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_single_episode_doesnt_exist_on_disk()
        {
            _parseResultSingle.Episodes = new List<Episode>();

            _upgradeDisk.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_upgradable_if_only_episode_is_upgradable()
        {
            WithFirstFileUpgradable();
            _upgradeDisk.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_upgradable_if_both_episodes_are_upgradable()
        {
            WithFirstFileUpgradable();
            WithSecondFileUpgradable();
            _upgradeDisk.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_not_upgradable_if_both_episodes_are_not_upgradable()
        {
            _upgradeDisk.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_not_upgradable_if_only_first_episodes_is_upgradable()
        {
            WithFirstFileUpgradable();
            _upgradeDisk.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_not_upgradable_if_only_second_episodes_is_upgradable()
        {
            WithSecondFileUpgradable();
            _upgradeDisk.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_not_be_upgradable_if_qualities_are_the_same()
        {
            _firstFile.Quality = new QualityModel(Quality.WEBDL1080p);
            _parseResultSingle.ParsedEpisodeInfo.Quality = new QualityModel(Quality.WEBDL1080p);
            _upgradeDisk.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeFalse();
        }
    }
}