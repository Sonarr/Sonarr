using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;
using NzbDrone.Core.DecisionEngine;

using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class UpgradeDiskSpecificationFixture : CoreTest<UpgradeDiskSpecification>
    {
        private UpgradeDiskSpecification _upgradeDisk;

        private RemoteEpisode parseResultMulti;
        private RemoteEpisode parseResultSingle;
        private EpisodeFile firstFile;
        private EpisodeFile secondFile;

        [SetUp]
        public void Setup()
        {
            Mocker.Resolve<QualityUpgradableSpecification>();
            _upgradeDisk = Mocker.Resolve<UpgradeDiskSpecification>();

            firstFile = new EpisodeFile { Quality = new QualityModel(Quality.Bluray1080p, true), DateAdded = DateTime.Now };
            secondFile = new EpisodeFile { Quality = new QualityModel(Quality.Bluray1080p, true), DateAdded = DateTime.Now };

            var singleEpisodeList = new List<Episode> { new Episode { EpisodeFile = firstFile, EpisodeFileId = 1 }, new Episode { EpisodeFile = null } };
            var doubleEpisodeList = new List<Episode> { new Episode { EpisodeFile = firstFile, EpisodeFileId = 1 }, new Episode { EpisodeFile = secondFile, EpisodeFileId = 1 }, new Episode { EpisodeFile = null } };

            var fakeSeries = Builder<Series>.CreateNew()
                         .With(c => c.QualityProfile = new QualityProfile { Cutoff = Quality.Bluray1080p })
                         .Build();

            parseResultMulti = new RemoteEpisode
            {
                Series = fakeSeries,
                ParsedEpisodeInfo = new ParsedEpisodeInfo { Quality = new QualityModel(Quality.DVD, true) },
                Episodes = doubleEpisodeList
            };

            parseResultSingle = new RemoteEpisode
            {
                Series = fakeSeries,
                ParsedEpisodeInfo = new ParsedEpisodeInfo { Quality = new QualityModel(Quality.DVD, true) },
                Episodes = singleEpisodeList
            };
        }

        private void WithFirstFileUpgradable()
        {
            firstFile.Quality = new QualityModel(Quality.SDTV);
        }

        private void WithSecondFileUpgradable()
        {
            secondFile.Quality = new QualityModel(Quality.SDTV);
        }

        [Test]
        public void should_return_true_if_episode_has_no_existing_file()
        {
            parseResultSingle.Episodes.ForEach(c => c.EpisodeFileId = 0);
            _upgradeDisk.IsSatisfiedBy(parseResultSingle, null).Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_single_episode_doesnt_exist_on_disk()
        {
            parseResultSingle.Episodes = new List<Episode>();

            _upgradeDisk.IsSatisfiedBy(parseResultSingle, null).Should().BeTrue();
        }

        [Test]
        public void should_be_upgradable_if_only_episode_is_upgradable()
        {
            WithFirstFileUpgradable();
            _upgradeDisk.IsSatisfiedBy(parseResultSingle, null).Should().BeTrue();
        }

        [Test]
        public void should_be_upgradable_if_both_episodes_are_upgradable()
        {
            WithFirstFileUpgradable();
            WithSecondFileUpgradable();
            _upgradeDisk.IsSatisfiedBy(parseResultMulti, null).Should().BeTrue();
        }

        [Test]
        public void should_be_not_upgradable_if_both_episodes_are_not_upgradable()
        {
            _upgradeDisk.IsSatisfiedBy(parseResultMulti, null).Should().BeFalse();
        }

        [Test]
        public void should_be_not_upgradable_if_only_first_episodes_is_upgradable()
        {
            WithFirstFileUpgradable();
            _upgradeDisk.IsSatisfiedBy(parseResultMulti, null).Should().BeFalse();
        }

        [Test]
        public void should_be_not_upgradable_if_only_second_episodes_is_upgradable()
        {
            WithSecondFileUpgradable();
            _upgradeDisk.IsSatisfiedBy(parseResultMulti, null).Should().BeFalse();
        }

        [Test]
        public void should_not_be_upgradable_if_qualities_are_the_same()
        {
            firstFile.Quality = new QualityModel(Quality.WEBDL1080p);
            parseResultSingle.ParsedEpisodeInfo.Quality = new QualityModel(Quality.WEBDL1080p, false);
            _upgradeDisk.IsSatisfiedBy(parseResultSingle, null).Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_episodeFile_was_added_more_than_7_days_ago()
        {
            firstFile.DateAdded = DateTime.Today.AddDays(-30);
            _upgradeDisk.IsSatisfiedBy(parseResultSingle, null).Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_first_episodeFile_was_added_more_than_7_days_ago()
        {
            firstFile.DateAdded = DateTime.Today.AddDays(-30);
            _upgradeDisk.IsSatisfiedBy(parseResultMulti, null).Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_second_episodeFile_was_added_more_than_7_days_ago()
        {
            secondFile.DateAdded = DateTime.Today.AddDays(-30);
            _upgradeDisk.IsSatisfiedBy(parseResultMulti, null).Should().BeFalse();
        }
    }
}