using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.ImportLists.ImportListItems;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ImportListTests
{
    [TestFixture]
    public class FetchAndParseImportListServiceFixture : CoreTest<FetchAndParseImportListService>
    {
        private List<IImportList> _importLists;
        private List<ImportListItemInfo> _listSeries;

        [SetUp]
        public void Setup()
        {
            _importLists = new List<IImportList>();

            Mocker.GetMock<IImportListFactory>()
                  .Setup(v => v.AutomaticAddEnabled(It.IsAny<bool>()))
                  .Returns(_importLists);

            _listSeries = Builder<ImportListItemInfo>.CreateListOfSize(5)
                .Build().ToList();

            Mocker.GetMock<ISearchForNewSeries>()
                .Setup(v => v.SearchForNewSeriesByImdbId(It.IsAny<string>()))
                .Returns((string value) => new List<Tv.Series>() { new Tv.Series() { ImdbId = value } });
        }

        private Mock<IImportList> WithList(int id, bool enabled, bool enabledAuto, ImportListFetchResult fetchResult, TimeSpan? minRefresh = null, int? lastSyncOffset = null, int? syncDeletedCount = null)
        {
            return CreateListResult(id, enabled, enabledAuto, fetchResult, minRefresh, lastSyncOffset, syncDeletedCount);
        }

        private Mock<IImportList> CreateListResult(int id, bool enabled, bool enabledAuto, ImportListFetchResult fetchResult, TimeSpan? minRefresh = null, int? lastSyncOffset = null, int? syncDeletedCount = null)
        {
            var refreshInterval = minRefresh ?? TimeSpan.FromHours(12);
            var importListDefinition = new ImportListDefinition { Id = id, Enable = enabled, EnableAutomaticAdd = enabledAuto, MinRefreshInterval = refreshInterval };

            var mockImportList = new Mock<IImportList>();
            mockImportList.SetupGet(s => s.Definition).Returns(importListDefinition);
            mockImportList.Setup(s => s.Fetch()).Returns(fetchResult);
            mockImportList.SetupGet(s => s.MinRefreshInterval).Returns(refreshInterval);

            DateTime? lastSync = lastSyncOffset.HasValue ? DateTime.UtcNow.AddHours(lastSyncOffset.Value) : null;
            Mocker.GetMock<IImportListStatusService>()
                .Setup(v => v.GetListStatus(id))
                .Returns(new ImportListStatus() { LastInfoSync = lastSync });

            if (syncDeletedCount.HasValue)
            {
                Mocker.GetMock<IImportListItemService>()
                    .Setup(v => v.SyncSeriesForList(It.IsAny<List<ImportListItemInfo>>(), id))
                    .Returns(syncDeletedCount.Value);
            }

            _importLists.Add(mockImportList.Object);

            return mockImportList;
        }

        [Test]
        public void should_skip_recently_fetched_list()
        {
            var fetchResult = new ImportListFetchResult();
            var list = WithList(1, true, true, fetchResult, lastSyncOffset: 0);

            var result = Subject.Fetch();

            list.Verify(f => f.Fetch(), Times.Never());
            result.Series.Count.Should().Be(0);
            result.AnyFailure.Should().BeFalse();
        }

        [Test]
        public void should_skip_recent_and_fetch_good()
        {
            var fetchResult = new ImportListFetchResult();
            var recent = WithList(1, true, true, fetchResult, lastSyncOffset: 0);
            var old = WithList(2, true, true, fetchResult);

            var result = Subject.Fetch();

            recent.Verify(f => f.Fetch(), Times.Never());
            old.Verify(f => f.Fetch(), Times.Once());
            result.AnyFailure.Should().BeFalse();
        }

        [Test]
        public void should_return_failure_if_single_list_fails()
        {
            var fetchResult = new ImportListFetchResult { Series = _listSeries, AnyFailure = true };
            WithList(1, true, true, fetchResult);

            var listResult = Subject.Fetch();
            listResult.AnyFailure.Should().BeTrue();

            Mocker.GetMock<IImportListStatusService>()
                .Verify(v => v.UpdateListSyncStatus(It.IsAny<int>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void should_return_failure_if_any_list_fails()
        {
            var fetchResult1 = new ImportListFetchResult { Series = _listSeries, AnyFailure = true };
            WithList(1, true, true, fetchResult1);
            var fetchResult2 = new ImportListFetchResult { Series = _listSeries, AnyFailure = false };
            WithList(2, true, true, fetchResult2);

            var listResult = Subject.Fetch();
            listResult.AnyFailure.Should().BeTrue();
        }

        [Test]
        public void should_return_early_if_no_available_lists()
        {
            var listResult = Subject.Fetch();

            Mocker.GetMock<IImportListStatusService>()
                  .Verify(v => v.GetListStatus(It.IsAny<int>()), Times.Never());

            listResult.Series.Count.Should().Be(0);
            listResult.AnyFailure.Should().BeFalse();
        }

        [Test]
        public void should_store_series_if_list_doesnt_fail()
        {
            var listId = 1;
            var fetchResult = new ImportListFetchResult { Series = _listSeries, AnyFailure = false };
            WithList(listId, true, true, fetchResult);

            var listResult = Subject.Fetch();
            listResult.AnyFailure.Should().BeFalse();

            Mocker.GetMock<IImportListStatusService>()
                .Verify(v => v.UpdateListSyncStatus(listId, false), Times.Once());
            Mocker.GetMock<IImportListItemService>()
                .Verify(v => v.SyncSeriesForList(_listSeries, listId), Times.Once());
        }

        [Test]
        public void should_not_store_series_if_list_fails()
        {
            var listId = 1;
            var fetchResult = new ImportListFetchResult { Series = _listSeries, AnyFailure = true };
            WithList(listId, true, true, fetchResult);

            var listResult = Subject.Fetch();
            listResult.AnyFailure.Should().BeTrue();

            Mocker.GetMock<IImportListStatusService>()
                .Verify(v => v.UpdateListSyncStatus(listId, false), Times.Never());
            Mocker.GetMock<IImportListItemService>()
                .Verify(v => v.SyncSeriesForList(It.IsAny<List<ImportListItemInfo>>(), listId), Times.Never());
        }

        [Test]
        public void should_only_store_series_for_lists_that_dont_fail()
        {
            var passedListId = 1;
            var fetchResult1 = new ImportListFetchResult { Series = _listSeries, AnyFailure = false };
            WithList(passedListId, true, true, fetchResult1);
            var failedListId = 2;
            var fetchResult2 = new ImportListFetchResult { Series = _listSeries, AnyFailure = true };
            WithList(failedListId, true, true, fetchResult2);

            var listResult = Subject.Fetch();
            listResult.AnyFailure.Should().BeTrue();

            Mocker.GetMock<IImportListStatusService>()
                .Verify(v => v.UpdateListSyncStatus(passedListId, false), Times.Once());
            Mocker.GetMock<IImportListItemService>()
                .Verify(v => v.SyncSeriesForList(_listSeries, passedListId), Times.Once());
            Mocker.GetMock<IImportListStatusService>()
                .Verify(v => v.UpdateListSyncStatus(failedListId, false), Times.Never());
            Mocker.GetMock<IImportListItemService>()
                .Verify(v => v.SyncSeriesForList(It.IsAny<List<ImportListItemInfo>>(), failedListId), Times.Never());
        }

        [Test]
        public void should_return_all_results_for_all_lists()
        {
            var passedListId = 1;
            var fetchResult1 = new ImportListFetchResult { Series = _listSeries, AnyFailure = false };
            WithList(passedListId, true, true, fetchResult1);
            var secondListId = 2;
            var fetchResult2 = new ImportListFetchResult { Series = _listSeries, AnyFailure = false };
            WithList(secondListId, true, true, fetchResult2);

            var listResult = Subject.Fetch();
            listResult.AnyFailure.Should().BeFalse();
            listResult.Series.Count.Should().Be(5);
        }

        [Test]
        public void should_set_removed_flag_if_list_has_removed_items()
        {
            var listId = 1;
            var fetchResult = new ImportListFetchResult { Series = _listSeries, AnyFailure = false };
            WithList(listId, true, true, fetchResult, syncDeletedCount: 500);

            var result = Subject.Fetch();
            result.AnyFailure.Should().BeFalse();

            Mocker.GetMock<IImportListStatusService>()
                .Verify(v => v.UpdateListSyncStatus(listId, true), Times.Once());
        }
    }
}
