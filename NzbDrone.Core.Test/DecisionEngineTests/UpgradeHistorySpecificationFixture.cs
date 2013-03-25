// ReSharper disable RedundantUsingDirective

using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.History;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.DecisionEngine;

using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class UpgradeHistorySpecificationFixture : CoreTest
    {
        private UpgradeHistorySpecification _upgradeHistory;

        private EpisodeParseResult parseResultMulti;
        private EpisodeParseResult parseResultSingle;
        private QualityModel firstQuality;
        private QualityModel secondQuality;
        private Series fakeSeries;

        [SetUp]
        public void Setup()
        {
            Mocker.Resolve<QualityUpgradableSpecification>();
            _upgradeHistory = Mocker.Resolve<UpgradeHistorySpecification>();

            var singleEpisodeList = new List<Episode> { new Episode { SeasonNumber = 12, EpisodeNumber = 3 } };
            var doubleEpisodeList = new List<Episode> { 
                                                            new Episode { SeasonNumber = 12, EpisodeNumber = 3 }, 
                                                            new Episode { SeasonNumber = 12, EpisodeNumber = 4 }, 
                                                            new Episode { SeasonNumber = 12, EpisodeNumber = 5 }
                                                       };

            fakeSeries = Builder<Series>.CreateNew()
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

            firstQuality = new QualityModel(Quality.Bluray1080p, true);
            secondQuality = new QualityModel(Quality.Bluray1080p, true);

            Mocker.GetMock<IHistoryService>().Setup(c => c.GetBestQualityInHistory(1)).Returns(firstQuality);
            Mocker.GetMock<IHistoryService>().Setup(c => c.GetBestQualityInHistory(2)).Returns(secondQuality);
            Mocker.GetMock<IHistoryService>().Setup(c => c.GetBestQualityInHistory(3)).Returns<QualityModel>(null);
        }

        private void WithFirstReportUpgradable()
        {
            firstQuality.Quality = Quality.SDTV;
        }

        private void WithSecondReportUpgradable()
        {
            secondQuality.Quality = Quality.SDTV;
        }


        [Test]
        public void should_be_upgradable_if_only_episode_is_upgradable()
        {
            WithFirstReportUpgradable();
            _upgradeHistory.IsSatisfiedBy(parseResultSingle).Should().BeTrue();
        }

        [Test]
        public void should_be_upgradable_if_both_episodes_are_upgradable()
        {
            WithFirstReportUpgradable();
            WithSecondReportUpgradable();
            _upgradeHistory.IsSatisfiedBy(parseResultMulti).Should().BeTrue();
        }

        [Test]
        public void should_not_be_upgradable_if_both_episodes_are_not_upgradable()
        {
            _upgradeHistory.IsSatisfiedBy(parseResultMulti).Should().BeFalse();
        }

        [Test]
        public void should_be_not_upgradable_if_only_first_episodes_is_upgradable()
        {
            WithFirstReportUpgradable();
            _upgradeHistory.IsSatisfiedBy(parseResultMulti).Should().BeFalse();
        }

        [Test]
        public void should_be_not_upgradable_if_only_second_episodes_is_upgradable()
        {
            WithSecondReportUpgradable();
            _upgradeHistory.IsSatisfiedBy(parseResultMulti).Should().BeFalse();
        }

        [Test]
        public void should_not_be_upgradable_if_episode_is_of_same_quality_as_existing()
        {
            fakeSeries.QualityProfile = new QualityProfile { Cutoff = Quality.WEBDL1080p };
            parseResultSingle.Quality = new QualityModel(Quality.WEBDL1080p, false);
            firstQuality = new QualityModel(Quality.WEBDL1080p, false);

            Mocker.GetMock<IHistoryService>().Setup(c => c.GetBestQualityInHistory(1)).Returns(firstQuality);

            _upgradeHistory.IsSatisfiedBy(parseResultSingle).Should().BeFalse();
        }
    }
}