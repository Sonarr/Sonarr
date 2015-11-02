using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine.Specifications.RssSync;
using NzbDrone.Core.History;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
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
        private const int FIRST_EPISODE_ID = 1;
        private const int SECOND_EPISODE_ID = 2;

        [SetUp]
        public void Setup()
        {
            Mocker.Resolve<QualityUpgradableSpecification>();
            _upgradeHistory = Mocker.Resolve<HistorySpecification>();

            var singleEpisodeList = new List<Episode> { new Episode { Id = FIRST_EPISODE_ID, SeasonNumber = 12, EpisodeNumber = 3 } };
            var doubleEpisodeList = new List<Episode> { 
                                                            new Episode {Id = FIRST_EPISODE_ID, SeasonNumber = 12, EpisodeNumber = 3 }, 
                                                            new Episode {Id = SECOND_EPISODE_ID, SeasonNumber = 12, EpisodeNumber = 4 }, 
                                                            new Episode {Id = 3, SeasonNumber = 12, EpisodeNumber = 5 }
                                                       };

            _fakeSeries = Builder<Series>.CreateNew()
                         .With(c => c.Profile = new Profile { Cutoff = Quality.Bluray1080p, Items = Qualities.QualityFixture.GetDefaultQualities() })
                         .Build();

            _parseResultMulti = new RemoteEpisode
            {
                Series = _fakeSeries,
                ParsedEpisodeInfo = new ParsedEpisodeInfo { Quality = new QualityModel(Quality.DVD, new Revision(version: 2)) },
                Episodes = doubleEpisodeList
            };

            _parseResultSingle = new RemoteEpisode
            {
                Series = _fakeSeries,
                ParsedEpisodeInfo = new ParsedEpisodeInfo { Quality = new QualityModel(Quality.DVD, new Revision(version: 2)) },
                Episodes = singleEpisodeList
            };

            _upgradableQuality = new QualityModel(Quality.SDTV, new Revision(version: 1));
            _notupgradableQuality = new QualityModel(Quality.HDTV1080p, new Revision(version: 2));

            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.EnableCompletedDownloadHandling)
                  .Returns(true);
        }

        private void GivenMostRecentForEpisode(int episodeId, string downloadId, QualityModel quality, DateTime date, HistoryEventType eventType)
        {
            Mocker.GetMock<IHistoryService>().Setup(s => s.MostRecentForEpisode(episodeId))
                  .Returns(new History.History { DownloadId = downloadId, Quality = quality, Date = date, EventType = eventType });
        }

        private void GivenCdhDisabled()
        {
            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.EnableCompletedDownloadHandling)
                  .Returns(false);
        }

        [Test]
        public void should_return_true_if_it_is_a_search()
        {
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, new SeasonSearchCriteria()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_latest_history_item_is_null()
        {
            Mocker.GetMock<IHistoryService>().Setup(s => s.MostRecentForEpisode(It.IsAny<int>())).Returns((History.History)null);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_latest_history_item_is_not_grabbed()
        {
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow, HistoryEventType.DownloadFailed);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeTrue();
        }

//        [Test]
//        public void should_return_true_if_latest_history_has_a_download_id_and_cdh_is_enabled()
//        {
//            GivenMostRecentForEpisode(FIRST_EPISODE_ID, "test", _notupgradableQuality, DateTime.UtcNow, HistoryEventType.Grabbed);
//            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeTrue();
//        }

        [Test]
        public void should_return_true_if_latest_history_item_is_older_than_twelve_hours()
        {
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow.AddHours(-13), HistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_upgradable_if_only_episode_is_upgradable()
        {
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _upgradableQuality, DateTime.UtcNow, HistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_upgradable_if_both_episodes_are_upgradable()
        {
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _upgradableQuality, DateTime.UtcNow, HistoryEventType.Grabbed);
            GivenMostRecentForEpisode(SECOND_EPISODE_ID, string.Empty, _upgradableQuality, DateTime.UtcNow, HistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_not_be_upgradable_if_both_episodes_are_not_upgradable()
        {
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow, HistoryEventType.Grabbed);
            GivenMostRecentForEpisode(SECOND_EPISODE_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow, HistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_not_upgradable_if_only_first_episodes_is_upgradable()
        {
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _upgradableQuality, DateTime.UtcNow, HistoryEventType.Grabbed);
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow, HistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_not_upgradable_if_only_second_episodes_is_upgradable()
        {
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow, HistoryEventType.Grabbed);
            GivenMostRecentForEpisode(SECOND_EPISODE_ID, string.Empty, _upgradableQuality, DateTime.UtcNow, HistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_not_be_upgradable_if_episode_is_of_same_quality_as_existing()
        {
            _fakeSeries.Profile = new Profile { Cutoff = Quality.Bluray1080p, Items = Qualities.QualityFixture.GetDefaultQualities() };
            _parseResultSingle.ParsedEpisodeInfo.Quality = new QualityModel(Quality.WEBDL1080p, new Revision(version: 1));
            _upgradableQuality = new QualityModel(Quality.WEBDL1080p, new Revision(version: 1));

            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _upgradableQuality, DateTime.UtcNow, HistoryEventType.Grabbed);

            _upgradeHistory.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_not_be_upgradable_if_cutoff_already_met()
        {
            _fakeSeries.Profile = new Profile { Cutoff = Quality.WEBDL1080p, Items = Qualities.QualityFixture.GetDefaultQualities() };
            _parseResultSingle.ParsedEpisodeInfo.Quality = new QualityModel(Quality.WEBDL1080p, new Revision(version: 1));
            _upgradableQuality = new QualityModel(Quality.Bluray1080p, new Revision(version: 1));

            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _upgradableQuality, DateTime.UtcNow, HistoryEventType.Grabbed);

            _upgradeHistory.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_latest_history_item_is_only_one_hour_old()
        {
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow.AddHours(-1), HistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_latest_history_has_a_download_id_and_cdh_is_disabled()
        {
            GivenCdhDisabled();
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, "test", _upgradableQuality, DateTime.UtcNow.AddDays(-100), HistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_cutoff_already_met_and_cdh_is_disabled()
        {
            GivenCdhDisabled();
            _fakeSeries.Profile = new Profile { Cutoff = Quality.WEBDL1080p, Items = Qualities.QualityFixture.GetDefaultQualities() };
            _parseResultSingle.ParsedEpisodeInfo.Quality = new QualityModel(Quality.Bluray1080p, new Revision(version: 1));
            _upgradableQuality = new QualityModel(Quality.WEBDL1080p, new Revision(version: 1));

            GivenMostRecentForEpisode(FIRST_EPISODE_ID, "test", _upgradableQuality, DateTime.UtcNow.AddDays(-100), HistoryEventType.Grabbed);

            _upgradeHistory.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_only_episode_is_not_upgradable_and_cdh_is_disabled()
        {
            GivenCdhDisabled();
            GivenMostRecentForEpisode(FIRST_EPISODE_ID, "test", _notupgradableQuality, DateTime.UtcNow.AddDays(-100), HistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeFalse();
        }
    }
}
