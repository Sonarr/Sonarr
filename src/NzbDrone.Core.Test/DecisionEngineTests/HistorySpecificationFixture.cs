using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.DecisionEngine.Specifications.RssSync;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients.Sabnzbd;
using NzbDrone.Core.History;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class HistorySpecificationFixture : CoreTest<HistorySpecification>
    {
        private HistorySpecification _upgradeHistory;

        private RemoteEpisode _parseResultMulti;
        private RemoteMovie _parseMovieResult;
        private RemoteEpisode _parseResultSingle;
        private QualityModel _upgradableQuality;
        private QualityModel _notupgradableQuality;
        private Series _fakeSeries;
        private Movie _fakeMovie;

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
                         .With(c => c.Profile = new Profile { Cutoff = Quality.Bluray1080p, Items = Qualities.QualityFixture.GetDefaultQualities() })
                         .Build();

            _fakeMovie = Builder<Movie>.CreateNew()
                        .With(c => c.Profile = new Profile { Cutoff = Quality.Bluray1080p, Items = Qualities.QualityFixture.GetDefaultQualities() })
                        .With(c => c.Id = 1)
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

            _parseMovieResult = new RemoteMovie
            {
                Movie = _fakeMovie,
                ParsedMovieInfo = new ParsedMovieInfo { Quality = new QualityModel(Quality.DVD, new Revision(version: 2)) }
            };

            _upgradableQuality = new QualityModel(Quality.SDTV, new Revision(version: 1));
            _notupgradableQuality = new QualityModel(Quality.HDTV1080p, new Revision(version: 2));

            Mocker.GetMock<IHistoryService>().Setup(c => c.GetBestEpisodeQualityInHistory(It.IsAny<Profile>(), 1)).Returns(_notupgradableQuality);
            Mocker.GetMock<IHistoryService>().Setup(c => c.GetBestEpisodeQualityInHistory(It.IsAny<Profile>(), 2)).Returns(_notupgradableQuality);
            Mocker.GetMock<IHistoryService>().Setup(c => c.GetBestEpisodeQualityInHistory(It.IsAny<Profile>(), 3)).Returns<QualityModel>(null);

            Mocker.GetMock<IHistoryService>().Setup(c => c.GetBestMovieQualityInHistory(It.IsAny<Profile>(), 1)).Returns(_notupgradableQuality);

            Mocker.GetMock<IProvideDownloadClient>()
                  .Setup(c => c.GetDownloadClients())
                  .Returns(new IDownloadClient[] { Mocker.GetMock<IDownloadClient>().Object });
        }

        private void WithFirstReportUpgradable()
        {
            Mocker.GetMock<IHistoryService>().Setup(c => c.GetBestEpisodeQualityInHistory(It.IsAny<Profile>(), 1)).Returns(_upgradableQuality);
            Mocker.GetMock<IHistoryService>().Setup(c => c.GetBestMovieQualityInHistory(It.IsAny<Profile>(), 1)).Returns(_upgradableQuality);
        }

        private void WithSecondReportUpgradable()
        {
            Mocker.GetMock<IHistoryService>().Setup(c => c.GetBestEpisodeQualityInHistory(It.IsAny<Profile>(), 2)).Returns(_upgradableQuality);
        }

        private void GivenSabnzbdDownloadClient()
        {
            Mocker.GetMock<IProvideDownloadClient>()
                  .Setup(c => c.GetDownloadClients())
                  .Returns(new IDownloadClient[] { Mocker.Resolve<Sabnzbd>() });
        }

        private void GivenMostRecentForEpisode(HistoryEventType eventType)
        {
            Mocker.GetMock<IHistoryService>().Setup(s => s.MostRecentForEpisode(It.IsAny<int>()))
                  .Returns(new History.History { EventType = eventType });
        }

        private void GivenMostRecentForMovie(HistoryEventType eventType)
        {
            Mocker.GetMock<IHistoryService>().Setup(s => s.MostRecentForMovie(It.IsAny<int>()))
                  .Returns(new History.History { EventType = eventType });
        }

        [Test]
        public void should_be_upgradable_if_only_episode_is_upgradable()
        {
            WithFirstReportUpgradable();
            _upgradeHistory.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_upgradable_if_movie_is_upgradable()
        {
            WithFirstReportUpgradable();
            _upgradeHistory.IsSatisfiedBy(_parseMovieResult, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_upgradable_if_both_episodes_are_upgradable()
        {
            WithFirstReportUpgradable();
            WithSecondReportUpgradable();
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_not_be_upgradable_if_movie_is_not_upgradable()
        {
            _upgradeHistory.IsSatisfiedBy(_parseMovieResult, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_not_be_upgradable_if_both_episodes_are_not_upgradable()
        {
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_not_upgradable_if_only_first_episodes_is_upgradable()
        {
            WithFirstReportUpgradable();
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_not_upgradable_if_only_second_episodes_is_upgradable()
        {
            WithSecondReportUpgradable();
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_not_be_upgradable_if_episode_is_of_same_quality_as_existing()
        {
            _fakeSeries.Profile = new Profile { Cutoff = Quality.WEBDL1080p, Items = Qualities.QualityFixture.GetDefaultQualities() };
            _parseResultSingle.ParsedEpisodeInfo.Quality = new QualityModel(Quality.WEBDL1080p, new Revision(version: 1));
            _upgradableQuality = new QualityModel(Quality.WEBDL1080p, new Revision(version: 1));

            Mocker.GetMock<IHistoryService>().Setup(c => c.GetBestEpisodeQualityInHistory(It.IsAny<Profile>(), 1)).Returns(_upgradableQuality);

            _upgradeHistory.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_not_be_upgradable_if_movie_is_of_same_quality_as_existing()
        {
            _fakeMovie.Profile = new Profile { Cutoff = Quality.WEBDL1080p, Items = Qualities.QualityFixture.GetDefaultQualities() };
            _parseMovieResult.ParsedMovieInfo.Quality = new QualityModel(Quality.WEBDL1080p, new Revision(version: 1));
            _upgradableQuality = new QualityModel(Quality.WEBDL1080p, new Revision(version: 1));

            Mocker.GetMock<IHistoryService>().Setup(c => c.GetBestMovieQualityInHistory(It.IsAny<Profile>(), 1)).Returns(_upgradableQuality);

            _upgradeHistory.IsSatisfiedBy(_parseMovieResult, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_it_is_a_search()
        {
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, new SeasonSearchCriteria()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_it_is_a_search_for_movie()
        {
            _upgradeHistory.IsSatisfiedBy(_parseMovieResult, new MovieSearchCriteria()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_using_sabnzbd_and_nothing_in_history()
        {
            GivenSabnzbdDownloadClient();

            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeTrue();
            _upgradeHistory.IsSatisfiedBy(_parseMovieResult, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_most_recent_in_history_is_grabbed()
        {
            GivenSabnzbdDownloadClient();
            GivenMostRecentForEpisode(HistoryEventType.Grabbed);
            GivenMostRecentForMovie(HistoryEventType.Grabbed);

            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeFalse();
            _upgradeHistory.IsSatisfiedBy(_parseMovieResult, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_most_recent_in_history_is_failed()
        {
            GivenSabnzbdDownloadClient();
            GivenMostRecentForEpisode(HistoryEventType.DownloadFailed);
            GivenMostRecentForMovie(HistoryEventType.DownloadFailed);

            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeTrue();
            _upgradeHistory.IsSatisfiedBy(_parseMovieResult, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_most_recent_in_history_is_imported()
        {
            GivenSabnzbdDownloadClient();
            GivenMostRecentForEpisode(HistoryEventType.DownloadFolderImported);
            GivenMostRecentForMovie(HistoryEventType.DownloadFolderImported);

            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeTrue();
            _upgradeHistory.IsSatisfiedBy(_parseMovieResult, null).Accepted.Should().BeTrue();
        }
    }
}