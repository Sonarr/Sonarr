// ReSharper disable RedundantUsingDirective

using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.DecisionEngine;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ProviderTests.DecisionEngineTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class UpgradeHistorySpecificationFixtrue : CoreTest
    {
        private UpgradeHistorySpecification _upgradeHistory;

        private EpisodeParseResult parseResultMulti;
        private EpisodeParseResult parseResultSingle;
        private Quality firstQuality;
        private Quality secondQuality;

        [SetUp]
        public void Setup()
        {
            Mocker.Resolve<QualityUpgradeSpecification>();
            _upgradeHistory = Mocker.Resolve<UpgradeHistorySpecification>();

            var fakeSeries = Builder<Series>.CreateNew()
                         .With(c => c.QualityProfile = new QualityProfile { Cutoff = QualityTypes.Bluray1080p })
                         .Build();

            parseResultMulti = new EpisodeParseResult
            {
                Series = fakeSeries,
                Quality = new Quality(QualityTypes.DVD, true),
                EpisodeNumbers = new List<int> { 3, 4 },
                SeasonNumber = 12,
            };

            parseResultSingle = new EpisodeParseResult
            {
                Series = fakeSeries,
                Quality = new Quality(QualityTypes.DVD, true),
                EpisodeNumbers = new List<int> { 3 },
                SeasonNumber = 12,
            };

            firstQuality = new Quality(QualityTypes.Bluray1080p, true);
            secondQuality = new Quality(QualityTypes.Bluray1080p, true);

            var singleEpisodeList = new List<Episode> { new Episode { SeasonNumber = 12, EpisodeNumber = 3 } };
            var doubleEpisodeList = new List<Episode> { 
                                                            new Episode { SeasonNumber = 12, EpisodeNumber = 3 }, 
                                                            new Episode { SeasonNumber = 12, EpisodeNumber = 4 }, 
                                                            new Episode { SeasonNumber = 12, EpisodeNumber = 5 }
                                                       };

            Mocker.GetMock<EpisodeProvider>().Setup(c => c.GetEpisodesByParseResult(parseResultSingle)).Returns(singleEpisodeList);
            Mocker.GetMock<EpisodeProvider>().Setup(c => c.GetEpisodesByParseResult(parseResultMulti)).Returns(doubleEpisodeList);

            Mocker.GetMock<HistoryProvider>().Setup(c => c.GetBestQualityInHistory(fakeSeries.SeriesId, 12, 3)).Returns(firstQuality);
            Mocker.GetMock<HistoryProvider>().Setup(c => c.GetBestQualityInHistory(fakeSeries.SeriesId, 12, 4)).Returns(secondQuality);
            Mocker.GetMock<HistoryProvider>().Setup(c => c.GetBestQualityInHistory(fakeSeries.SeriesId, 12, 5)).Returns<Quality>(null);
        }

        private void WithFirstReportUpgradable()
        {
            firstQuality.QualityType = QualityTypes.SDTV;
        }

        private void WithSecondReportUpgradable()
        {
            secondQuality.QualityType = QualityTypes.SDTV;
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
    }
}