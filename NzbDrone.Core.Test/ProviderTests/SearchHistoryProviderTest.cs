using System;
using System.IO;
using System.Linq;
using System.Net;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Repository.Search;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common.AutoMoq;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class SearchHistoryProviderTest : CoreTest
    {
        private SearchHistory _searchHistory;
        private Series _series;
        private Episode _episode;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew()
                    .Build();

            _episode = Builder<Episode>.CreateNew()
                    .Build();

            var items = Builder<SearchHistoryItem>.CreateListOfSize(10)
                    .Build().ToList();

            _searchHistory = Builder<SearchHistory>.CreateNew()
                    .With(h => h.EpisodeId = _episode.EpisodeId)
                    .With(h => h.SeriesId - _series.SeriesId)
                    .With(h => h.SearchHistoryItems = items)
                    .Build();
        }

        private void WithUnsuccessfulSearch()
        {
            foreach(var item in _searchHistory.SearchHistoryItems)
            {
                item.Success = false;
            }
        }

        private void WithSuccessfulSearch()
        {
            foreach(var item in _searchHistory.SearchHistoryItems)
            {
                item.Success = false;
            }

            var i = _searchHistory.SearchHistoryItems.Last();
            i.Success = true;
            i.SearchError = ReportRejectionType.None;
        }

        private void WithExpiredHistory()
        {
            var history = Builder<SearchHistory>.CreateListOfSize(10)
                    .All()
                    .With(h => h.SearchTime = DateTime.Now.AddDays(-10))
                    .Build();

            foreach(var searchHistory in history)
            {
                var items = Builder<SearchHistoryItem>.CreateListOfSize(10)
                        .All()
                        .With(i => i.Id == searchHistory.Id)
                        .Build();

                Db.InsertMany(items);
            }

            Db.InsertMany(history);
        }

        private void WithValidHistory()
        {
            var history = Builder<SearchHistory>.CreateListOfSize(10)
                    .All()
                    .With(h => h.SearchTime = DateTime.Now)
                    .Build();

            foreach (var searchHistory in history)
            {
                var items = Builder<SearchHistoryItem>.CreateListOfSize(10)
                        .All()
                        .With(i => i.Id == searchHistory.Id)
                        .Build();

                Db.InsertMany(items);
            }

            Db.InsertMany(history);
        }

        [Test]
        public void Add_should_add_history_and_history_items()
        {
            WithRealDb();

            Mocker.Resolve<SearchHistoryProvider>().Add(_searchHistory);

            Db.Fetch<SearchHistory>().Should().HaveCount(1);
            Db.Fetch<SearchHistoryItem>().Should().HaveCount(10);
        }

        [Test]
        public void Add_should_add_return_id()
        {
            WithRealDb();

            var result = Mocker.Resolve<SearchHistoryProvider>().Add(_searchHistory);

            result.Should().NotBe(0);
        }

        [Test]
        public void Delete_should_delete_history_and_history_items()
        {
            WithRealDb();

            Mocker.Resolve<SearchHistoryProvider>().Add(_searchHistory);

            var history = Db.Fetch<SearchHistory>();

            Mocker.Resolve<SearchHistoryProvider>().Delete(history.First().Id);

            Db.Fetch<SearchHistory>().Should().HaveCount(0);
            Db.Fetch<SearchHistoryItem>().Should().HaveCount(0);
        }

        [Test]
        public void AllSearchHistory_should_return_all_items()
        {
            WithRealDb();
            Db.Insert(_series);
            Db.Insert(_episode);

            Mocker.Resolve<SearchHistoryProvider>().Add(_searchHistory);

            var result = Mocker.Resolve<SearchHistoryProvider>().AllSearchHistory();

            result.Count.Should().Be(1);
        }

        [Test]
        public void AllSearchHistory_should_have_series_title()
        {
            WithRealDb();
            Db.Insert(_series);
            Db.Insert(_episode);

            Mocker.Resolve<SearchHistoryProvider>().Add(_searchHistory);

            var result = Mocker.Resolve<SearchHistoryProvider>().AllSearchHistory();

            result.Count.Should().Be(1);
            result.First().SeriesTitle.Should().Be(_series.Title);
        }

        [Test]
        public void AllSearchHistory_should_have_episode_information()
        {
            WithRealDb();
            Db.Insert(_series);
            Db.Insert(_episode);

            Mocker.Resolve<SearchHistoryProvider>().Add(_searchHistory);

            var result = Mocker.Resolve<SearchHistoryProvider>().AllSearchHistory();

            result.Count.Should().Be(1);
            result.First().EpisodeTitle.Should().Be(_episode.Title);
            result.First().EpisodeNumber.Should().Be(_episode.EpisodeNumber);
            result.First().SeasonNumber.Should().Be(_episode.SeasonNumber);
        }

        [Test]
        public void AllSearchHistory_should_have_totalItems_count()
        {
            WithRealDb();
            Db.Insert(_series);
            Db.Insert(_episode);

            Mocker.Resolve<SearchHistoryProvider>().Add(_searchHistory);

            var result = Mocker.Resolve<SearchHistoryProvider>().AllSearchHistory();

            result.Count.Should().Be(1);
            result.First().TotalItems.Should().Be(_searchHistory.SearchHistoryItems.Count);
        }

        [Test]
        public void AllSearchHistory_should_have_successfulCount_equal_zero_when_all_failed()
        {
            WithRealDb();
            Db.Insert(_series);
            Db.Insert(_episode);

            WithUnsuccessfulSearch();

            Mocker.Resolve<SearchHistoryProvider>().Add(_searchHistory);

            var result = Mocker.Resolve<SearchHistoryProvider>().AllSearchHistory();

            result.Count.Should().Be(1);
            result.First().SuccessfulCount.Should().Be(0);
        }

        [Test]
        public void AllSearchHistory_should_have_successfulCount_equal_one_when_one_was_downloaded()
        {
            WithRealDb();
            Db.Insert(_series);
            Db.Insert(_episode);

            WithSuccessfulSearch();

            Mocker.Resolve<SearchHistoryProvider>().Add(_searchHistory);

            var result = Mocker.Resolve<SearchHistoryProvider>().AllSearchHistory();

            result.Count.Should().Be(1);
            result.First().SuccessfulCount.Should().Be(1);
        }

        [Test]
        public void GetSearchHistory_should_return_searchHistory_with_items()
        {
            WithRealDb();
            Db.Insert(_series);
            Db.Insert(_episode);

            WithSuccessfulSearch();
            var id = Mocker.Resolve<SearchHistoryProvider>().Add(_searchHistory);

            var result = Mocker.Resolve<SearchHistoryProvider>().GetSearchHistory(id);

            result.SearchHistoryItems.Should().HaveCount(_searchHistory.SearchHistoryItems.Count);
        }

        [Test]
        public void GetSearchHistory_should_have_episodeDetails()
        {
            WithRealDb();
            Db.Insert(_series);
            Db.Insert(_episode);

            WithSuccessfulSearch();
            var id = Mocker.Resolve<SearchHistoryProvider>().Add(_searchHistory);

            var result = Mocker.Resolve<SearchHistoryProvider>().GetSearchHistory(id);

            result.EpisodeNumber.Should().Be(_episode.EpisodeNumber);
            result.SeasonNumber.Should().Be(_episode.SeasonNumber);
            result.EpisodeTitle.Should().Be(_episode.Title);
            result.AirDate.Should().Be(_episode.AirDate.Value);
        }

        [Test]
        public void GetSearchHistory_should_not_have_episode_info_if_it_was_a_full_season_search()
        {
            WithRealDb();
            Db.Insert(_series);

            _searchHistory.EpisodeId = 0;
            var id = Mocker.Resolve<SearchHistoryProvider>().Add(_searchHistory);

            var result = Mocker.Resolve<SearchHistoryProvider>().GetSearchHistory(id);

            result.EpisodeNumber.Should().Be(null);
            result.SeasonNumber.Should().Be(_searchHistory.SeasonNumber);
            result.EpisodeTitle.Should().Be(null);
        }

        [Test]
        public void ForceDownload_should_download_report()
        {
            WithRealDb();
            Db.Insert(_series);
            Db.Insert(_episode);

            var reportTitle = String.Format("{0} - S{1:00}E{2:00}", _series.Title, _episode.SeasonNumber, _episode.EpisodeNumber);
            _searchHistory.SearchHistoryItems.First().ReportTitle = reportTitle;

            Mocker.Resolve<SearchHistoryProvider>().Add(_searchHistory);
            var items = Db.Fetch<SearchHistoryItem>();

            Mocker.Resolve<SearchHistoryProvider>().ForceDownload(items.First().Id);

            Mocker.GetMock<DownloadProvider>().Verify(v => v.DownloadReport(It.IsAny<EpisodeParseResult>()), Times.Once());
        }

        [Test]
        public void Cleanup_should_not_blowup_if_there_is_nothing_to_delete()
        {
            WithRealDb();

            Mocker.Resolve<SearchHistoryProvider>().Cleanup();
            Db.Fetch<SearchHistory>().Should().HaveCount(0);
        }

        [Test]
        public void Cleanup_should_delete_searchHistory_older_than_1_week()
        {
            WithRealDb();
            WithExpiredHistory();

            Mocker.Resolve<SearchHistoryProvider>().Cleanup();
            Db.Fetch<SearchHistory>().Should().HaveCount(0);
        }

        [Test]
        public void Cleanup_should_delete_searchHistoryItems_older_than_1_week()
        {
            WithRealDb();
            WithExpiredHistory();

            Mocker.Resolve<SearchHistoryProvider>().Cleanup();
            Db.Fetch<SearchHistoryItem>().Should().HaveCount(0);
        }

        [Test]
        public void Cleanup_should_not_delete_searchHistory_younger_than_1_week()
        {
            WithRealDb();
            WithValidHistory();

            Mocker.Resolve<SearchHistoryProvider>().Cleanup();
            Db.Fetch<SearchHistory>().Should().HaveCount(10);
        }

        [Test]
        public void Cleanup_should_not_delete_searchHistoryItems_younger_than_1_week()
        {
            WithRealDb();
            WithValidHistory();

            Mocker.Resolve<SearchHistoryProvider>().Cleanup();
            Db.Fetch<SearchHistoryItem>().Should().HaveCount(100);
        }
    }
}