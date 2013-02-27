// ReSharper disable RedundantUsingDirective

using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class UpgradeDiskSpecificationFixture : CoreTest
    {
        private UpgradeDiskSpecification _upgradeDisk;

        private EpisodeParseResult parseResultMulti;
        private EpisodeParseResult parseResultSingle;
        private EpisodeFile firstFile;
        private EpisodeFile secondFile;

        [SetUp]
        public void Setup()
        {
            Mocker.Resolve<QualityUpgradeSpecification>();
            _upgradeDisk = Mocker.Resolve<UpgradeDiskSpecification>();

            firstFile = new EpisodeFile { Quality = Quality.Bluray1080p, Proper = true, DateAdded = DateTime.Now };
            secondFile = new EpisodeFile { Quality = Quality.Bluray1080p, Proper = true, DateAdded = DateTime.Now };

            var singleEpisodeList = new List<Episode> { new Episode { EpisodeFile = firstFile }, new Episode { EpisodeFile = null } };
            var doubleEpisodeList = new List<Episode> { new Episode { EpisodeFile = firstFile }, new Episode { EpisodeFile = secondFile }, new Episode { EpisodeFile = null } };

            var fakeSeries = Builder<Series>.CreateNew()
                         .With(c => c.QualityProfile = new QualityProfile { Cutoff = Quality.Bluray1080p })
                         .Build();

            parseResultMulti = new EpisodeParseResult
            {
                Series = fakeSeries,
                Quality = new QualityModel(Quality.DVD, true),
                EpisodeNumbers = new List<int> { 3, 4 },
                SeasonNumber = 12,
                Episodes = doubleEpisodeList
            };

            parseResultSingle = new EpisodeParseResult
            {
                Series = fakeSeries,
                Quality = new QualityModel(Quality.DVD, true),
                EpisodeNumbers = new List<int> { 3 },
                SeasonNumber = 12,
                Episodes = singleEpisodeList
            };
        }

        private void WithFirstFileUpgradable()
        {
            firstFile.Quality = Quality.SDTV;
        }

        private void WithSecondFileUpgradable()
        {
            secondFile.Quality = Quality.SDTV;
        }

        [Test]
        public void should_return_true_if_single_episode_doesnt_exist_on_disk()
        {
            parseResultSingle.Episodes = new List<Episode>();

            _upgradeDisk.IsSatisfiedBy(parseResultSingle).Should().BeTrue();
        }

        [Test]
        public void should_be_upgradable_if_only_episode_is_upgradable()
        {
            WithFirstFileUpgradable();
            _upgradeDisk.IsSatisfiedBy(parseResultSingle).Should().BeTrue();
        }

        [Test]
        public void should_be_upgradable_if_both_episodes_are_upgradable()
        {
            WithFirstFileUpgradable();
            WithSecondFileUpgradable();
            _upgradeDisk.IsSatisfiedBy(parseResultMulti).Should().BeTrue();
        }

        [Test]
        public void should_be_not_upgradable_if_both_episodes_are_not_upgradable()
        {
            _upgradeDisk.IsSatisfiedBy(parseResultMulti).Should().BeFalse();
        }

        [Test]
        public void should_be_not_upgradable_if_only_first_episodes_is_upgradable()
        {
            WithFirstFileUpgradable();
            _upgradeDisk.IsSatisfiedBy(parseResultMulti).Should().BeFalse();
        }

        [Test]
        public void should_be_not_upgradable_if_only_second_episodes_is_upgradable()
        {
            WithSecondFileUpgradable();
            _upgradeDisk.IsSatisfiedBy(parseResultMulti).Should().BeFalse();
        }

        [Test]
        public void should_not_be_upgradable_if_qualities_are_the_same()
        {
            firstFile.Quality = Quality.WEBDL1080p;
            firstFile.Proper = false;
            parseResultSingle.Quality = new QualityModel(Quality.WEBDL1080p, false);
            _upgradeDisk.IsSatisfiedBy(parseResultSingle).Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_episodeFile_was_added_more_than_7_days_ago()
        {
            firstFile.DateAdded = DateTime.Today.AddDays(-30);
            _upgradeDisk.IsSatisfiedBy(parseResultSingle).Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_first_episodeFile_was_added_more_than_7_days_ago()
        {
            firstFile.DateAdded = DateTime.Today.AddDays(-30);
            _upgradeDisk.IsSatisfiedBy(parseResultMulti).Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_second_episodeFile_was_added_more_than_7_days_ago()
        {
            secondFile.DateAdded = DateTime.Today.AddDays(-30);
            _upgradeDisk.IsSatisfiedBy(parseResultMulti).Should().BeFalse();
        }
    }
}