

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
    public class UpgradeHistorySpecificationFixture : CoreTest
    {
        private UpgradeHistorySpecification _upgradeHistory;

        private EpisodeParseResult _parseResultMulti;
        private EpisodeParseResult _parseResultSingle;
        private QualityModel _upgradableQuality;
        private QualityModel _notupgradableQuality;
        private Series _fakeSeries;

        [SetUp]
        public void Setup()
        {
            Mocker.Resolve<QualityUpgradableSpecification>();
            _upgradeHistory = Mocker.Resolve<UpgradeHistorySpecification>();

            var singleEpisodeList = new List<Episode> { new Episode { Id = 1, SeasonNumber = 12, EpisodeNumber = 3 } };
            var doubleEpisodeList = new List<Episode> { 
                                                            new Episode {Id = 1, SeasonNumber = 12, EpisodeNumber = 3 }, 
                                                            new Episode {Id = 2, SeasonNumber = 12, EpisodeNumber = 4 }, 
                                                            new Episode {Id = 3, SeasonNumber = 12, EpisodeNumber = 5 }
                                                       };

            _fakeSeries = Builder<Series>.CreateNew()
                         .With(c => c.QualityProfile = new QualityProfile { Cutoff = Quality.Bluray1080p })
                         .Build();

            _parseResultMulti = new EpisodeParseResult
            {
                Series = _fakeSeries,
                Quality = new QualityModel(Quality.DVD, true),
                EpisodeNumbers = new List<int> { 3, 4 },
                SeasonNumber = 12,
                Episodes = doubleEpisodeList
            };

            _parseResultSingle = new EpisodeParseResult
            {
                Series = _fakeSeries,
                Quality = new QualityModel(Quality.DVD, true),
                EpisodeNumbers = new List<int> { 3 },
                SeasonNumber = 12,
                Episodes = singleEpisodeList
            };

            _upgradableQuality = new QualityModel(Quality.SDTV, false);
            _notupgradableQuality = new QualityModel(Quality.HDTV1080p, true);



            Mocker.GetMock<IHistoryService>().Setup(c => c.GetBestQualityInHistory(1)).Returns(_notupgradableQuality);
            Mocker.GetMock<IHistoryService>().Setup(c => c.GetBestQualityInHistory(2)).Returns(_notupgradableQuality);
            Mocker.GetMock<IHistoryService>().Setup(c => c.GetBestQualityInHistory(3)).Returns<QualityModel>(null);
        }

        private void WithFirstReportUpgradable()
        {
            Mocker.GetMock<IHistoryService>().Setup(c => c.GetBestQualityInHistory(1)).Returns(_upgradableQuality);
        }

        private void WithSecondReportUpgradable()
        {
            Mocker.GetMock<IHistoryService>().Setup(c => c.GetBestQualityInHistory(2)).Returns(_upgradableQuality);
        }


        [Test]
        public void should_be_upgradable_if_only_episode_is_upgradable()
        {
            WithFirstReportUpgradable();
            _upgradeHistory.IsSatisfiedBy(_parseResultSingle).Should().BeTrue();
        }

        [Test]
        public void should_be_upgradable_if_both_episodes_are_upgradable()
        {
            WithFirstReportUpgradable();
            WithSecondReportUpgradable();
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti).Should().BeTrue();
        }

        [Test]
        public void should_not_be_upgradable_if_both_episodes_are_not_upgradable()
        {
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti).Should().BeFalse();
        }

        [Test]
        public void should_be_not_upgradable_if_only_first_episodes_is_upgradable()
        {
            WithFirstReportUpgradable();
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti).Should().BeFalse();
        }

        [Test]
        public void should_be_not_upgradable_if_only_second_episodes_is_upgradable()
        {
            WithSecondReportUpgradable();
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti).Should().BeFalse();
        }

        [Test]
        public void should_not_be_upgradable_if_episode_is_of_same_quality_as_existing()
        {
            _fakeSeries.QualityProfile = new QualityProfile { Cutoff = Quality.WEBDL1080p };
            _parseResultSingle.Quality = new QualityModel(Quality.WEBDL1080p, false);
            _upgradableQuality = new QualityModel(Quality.WEBDL1080p, false);

            Mocker.GetMock<IHistoryService>().Setup(c => c.GetBestQualityInHistory(1)).Returns(_upgradableQuality);

            _upgradeHistory.IsSatisfiedBy(_parseResultSingle).Should().BeFalse();
        }
    }
}