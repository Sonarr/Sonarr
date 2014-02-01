using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications.RssSync;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients.Sabnzbd;
using NzbDrone.Core.History;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;
using NzbDrone.Core.DecisionEngine;

using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class HistorySpecificationFixture : CoreTest<HistorySpecification>
    {
        private HistorySpecification _upgradeHistory;

        private RemoteEpisode _parseResultMulti;
        private RemoteEpisode _parseResultSingle;
        private QualityModel _upgradableQuality;
        private QualityModel _notupgradableQuality;
        private Series _fakeSeries;

        [SetUp]
        public void Setup()
        {
            Mocker.Resolve<QualityUpgradableSpecification>();
            _upgradeHistory = Mocker.Resolve<HistorySpecification>();

            var singleEpisodeList = new List<Episode> { new Episode { Id = 1, SeasonNumber = 12, EpisodeNumber = 3 } };
            var doubleEpisodeList = new List<Episode> { 
                                                            new Episode {Id = 1, SeasonNumber = 12, EpisodeNumber = 3 }, 
                                                            new Episode {Id = 2, SeasonNumber = 12, EpisodeNumber = 4 }, 
                                                            new Episode {Id = 3, SeasonNumber = 12, EpisodeNumber = 5 }
                                                       };

            _fakeSeries = Builder<Series>.CreateNew()
                         .With(c => c.QualityProfile = new QualityProfile { Cutoff = Quality.Bluray1080p, Items = Qualities.QualityFixture.GetDefaultQualities() })
                         .Build();

            _parseResultMulti = new RemoteEpisode
            {
                Series = _fakeSeries,
                ParsedEpisodeInfo = new ParsedEpisodeInfo { Quality = new QualityModel(Quality.DVD, true) },
                Episodes = doubleEpisodeList
            };

            _parseResultSingle = new RemoteEpisode
            {
                Series = _fakeSeries,
                ParsedEpisodeInfo = new ParsedEpisodeInfo { Quality = new QualityModel(Quality.DVD, true) },
                Episodes = singleEpisodeList
            };

            _upgradableQuality = new QualityModel(Quality.SDTV, false);
            _notupgradableQuality = new QualityModel(Quality.HDTV1080p, true);

            Mocker.GetMock<IHistoryService>().Setup(c => c.GetBestQualityInHistory(It.IsAny<QualityProfile>(), 1)).Returns(_notupgradableQuality);
            Mocker.GetMock<IHistoryService>().Setup(c => c.GetBestQualityInHistory(It.IsAny<QualityProfile>(), 2)).Returns(_notupgradableQuality);
            Mocker.GetMock<IHistoryService>().Setup(c => c.GetBestQualityInHistory(It.IsAny<QualityProfile>(), 3)).Returns<QualityModel>(null);

            Mocker.GetMock<IProvideDownloadClient>()
                  .Setup(c => c.GetDownloadClient()).Returns(Mocker.GetMock<IDownloadClient>().Object);
        }

        private void WithFirstReportUpgradable()
        {
            Mocker.GetMock<IHistoryService>().Setup(c => c.GetBestQualityInHistory(It.IsAny<QualityProfile>(), 1)).Returns(_upgradableQuality);
        }

        private void WithSecondReportUpgradable()
        {
            Mocker.GetMock<IHistoryService>().Setup(c => c.GetBestQualityInHistory(It.IsAny<QualityProfile>(), 2)).Returns(_upgradableQuality);
        }

        private void GivenSabnzbdDownloadClient()
        {
            Mocker.GetMock<IProvideDownloadClient>()
                  .Setup(c => c.GetDownloadClient()).Returns(Mocker.Resolve<SabnzbdClient>());
        }

        private void GivenMostRecentForEpisode(HistoryEventType eventType)
        {
            Mocker.GetMock<IHistoryService>().Setup(s => s.MostRecentForEpisode(It.IsAny<int>()))
                  .Returns(new History.History { EventType = eventType });
        }

        [Test]
        public void should_be_upgradable_if_only_episode_is_upgradable()
        {
            WithFirstReportUpgradable();
            _upgradeHistory.IsSatisfiedBy(_parseResultSingle, null).Should().BeTrue();
        }

        [Test]
        public void should_be_upgradable_if_both_episodes_are_upgradable()
        {
            WithFirstReportUpgradable();
            WithSecondReportUpgradable();
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, null).Should().BeTrue();
        }

        [Test]
        public void should_not_be_upgradable_if_both_episodes_are_not_upgradable()
        {
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, null).Should().BeFalse();
        }

        [Test]
        public void should_be_not_upgradable_if_only_first_episodes_is_upgradable()
        {
            WithFirstReportUpgradable();
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, null).Should().BeFalse();
        }

        [Test]
        public void should_be_not_upgradable_if_only_second_episodes_is_upgradable()
        {
            WithSecondReportUpgradable();
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, null).Should().BeFalse();
        }

        [Test]
        public void should_not_be_upgradable_if_episode_is_of_same_quality_as_existing()
        {
            _fakeSeries.QualityProfile = new QualityProfile { Cutoff = Quality.WEBDL1080p, Items = Qualities.QualityFixture.GetDefaultQualities() };
            _parseResultSingle.ParsedEpisodeInfo.Quality = new QualityModel(Quality.WEBDL1080p, false);
            _upgradableQuality = new QualityModel(Quality.WEBDL1080p, false);

            Mocker.GetMock<IHistoryService>().Setup(c => c.GetBestQualityInHistory(It.IsAny<QualityProfile>(), 1)).Returns(_upgradableQuality);

            _upgradeHistory.IsSatisfiedBy(_parseResultSingle, null).Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_it_is_a_search()
        {
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, new SeasonSearchCriteria()).Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_using_sabnzbd_and_nothing_in_history()
        {
            GivenSabnzbdDownloadClient();

            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, null).Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_most_recent_in_history_is_grabbed()
        {
            GivenSabnzbdDownloadClient();
            GivenMostRecentForEpisode(HistoryEventType.Grabbed);

            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, null).Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_most_recent_in_history_is_failed()
        {
            GivenSabnzbdDownloadClient();
            GivenMostRecentForEpisode(HistoryEventType.DownloadFailed);

            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, null).Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_most_recent_in_history_is_imported()
        {
            GivenSabnzbdDownloadClient();
            GivenMostRecentForEpisode(HistoryEventType.DownloadFolderImported);

            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, null).Should().BeTrue();
        }
    }
}