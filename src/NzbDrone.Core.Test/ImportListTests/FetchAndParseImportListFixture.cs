using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.ImportLists.ImportListItems;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

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
            _importLists = [];

            Mocker.GetMock<IImportListFactory>()
                  .Setup(v => v.AutomaticAddEnabled(It.IsAny<bool>()))
                  .Returns(_importLists);

            _listSeries = [.. Builder<ImportListItemInfo>.CreateListOfSize(5).Build()];

            Mocker.GetMock<ISearchForNewSeries>()
                .Setup(v => v.SearchForNewSeriesByImdbId(It.IsAny<string>(), It.IsAny<Language>()))
                .Returns((string value, Language _) => [new Tv.Series { ImdbId = value }]);
        }

        [Test]
        public void ShouldSkipRecentlyFetchedList()
        {
            var fetchResult = new ImportListFetchResult();
            var list = WithList(1, true, true, fetchResult, lastSyncOffset: 0);

            var result = Subject.Fetch();

            list.Verify(f => f.Fetch(), Times.Never());
            result.Series.Count.Should().Be(0);
            result.AnyFailure.Should().BeFalse();
        }

        [Test]
        public void ShouldSkipRecentAndFetchGood()
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
        public void ShouldReturnFailureIfSingleListFails()
        {
            var fetchResult = new ImportListFetchResult { Series = _listSeries, AnyFailure = true };
            WithList(1, true, true, fetchResult);

            var listResult = Subject.Fetch();
            listResult.AnyFailure.Should().BeTrue();

            Mocker.GetMock<IImportListStatusService>()
                .Verify(v => v.UpdateListSyncStatus(It.IsAny<int>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void ShouldReturnFailureIfAnyListFails()
        {
            var fetchResult1 = new ImportListFetchResult { Series = _listSeries, AnyFailure = true };
            WithList(1, true, true, fetchResult1);
            var fetchResult2 = new ImportListFetchResult { Series = _listSeries, AnyFailure = false };
            WithList(2, true, true, fetchResult2);

            var listResult = Subject.Fetch();
            listResult.AnyFailure.Should().BeTrue();
        }

        [Test]
        public void ShouldNotDeduplicateItemsWithSameTitleButDifferentTmdbId()
        {
            var series = new List<ImportListItemInfo>
            {
                new() { Title = "Doctor Who", TvdbId = 0, TmdbId = 1 },
                new() { Title = "Doctor Who", TvdbId = 0, TmdbId = 2 },
                new() { Title = "Doctor Who", TvdbId = 0, TmdbId = 3 }
            };

            WithList(1, true, true, new ImportListFetchResult { Series = series });

            Subject.Fetch().Series.Should().HaveCount(3);
        }

        [Test]
        public void ShouldDeduplicateItemsWithSameTmdbIdAndTitle()
        {
            var series = new List<ImportListItemInfo>
            {
                new() { Title = "Doctor Who", TvdbId = 0, TmdbId = 1 },
                new() { Title = "Doctor Who", TvdbId = 0, TmdbId = 1 }
            };

            WithList(1, true, true, new ImportListFetchResult { Series = series });

            Subject.Fetch().Series.Should().HaveCount(1);
        }

        [Test]
        public void ShouldReturnEarlyIfNoAvailableLists()
        {
            var listResult = Subject.Fetch();

            Mocker.GetMock<IImportListStatusService>()
                  .Verify(v => v.GetListStatus(It.IsAny<int>()), Times.Never());

            listResult.Series.Count.Should().Be(0);
            listResult.AnyFailure.Should().BeFalse();
        }

        [Test]
        public void ShouldStoreSeriesIfListDoesntFail()
        {
            const int listId = 1;
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
        public void ShouldNotStoreSeriesIfListFails()
        {
            const int listId = 1;
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
        public void ShouldOnlyStoreSeriesForListsThatDontFail()
        {
            const int passedListId = 1;
            var fetchResult1 = new ImportListFetchResult { Series = _listSeries, AnyFailure = false };
            WithList(passedListId, true, true, fetchResult1);
            const int failedListId = 2;
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
        public void ShouldReturnAllResultsForAllLists()
        {
            const int passedListId = 1;
            var fetchResult1 = new ImportListFetchResult { Series = _listSeries, AnyFailure = false };
            WithList(passedListId, true, true, fetchResult1);
            const int secondListId = 2;
            var fetchResult2 = new ImportListFetchResult { Series = _listSeries, AnyFailure = false };
            WithList(secondListId, true, true, fetchResult2);

            var listResult = Subject.Fetch();
            listResult.AnyFailure.Should().BeFalse();
            listResult.Series.Count.Should().Be(5);
        }

        [Test]
        public void ShouldSetRemovedFlagIfListHasRemovedItems()
        {
            const int listId = 1;
            var fetchResult = new ImportListFetchResult { Series = _listSeries, AnyFailure = false };
            WithList(listId, true, true, fetchResult, syncDeletedCount: 500);

            var result = Subject.Fetch();
            result.AnyFailure.Should().BeFalse();

            Mocker.GetMock<IImportListStatusService>()
                .Verify(v => v.UpdateListSyncStatus(listId, true), Times.Once());
        }

        [Test]
        public void ShouldNotCollapseTvdbOnlyAndTmdbBearingItemsWithSameTitle()
        {
            var series = new List<ImportListItemInfo>
            {
                new() { Title = "Show", TvdbId = 5, TmdbId = 0 },
                new() { Title = "Show", TvdbId = 5, TmdbId = 7 }
            };

            WithList(1, true, true, new ImportListFetchResult { Series = series });

            // Documented behavior: later sync dedupes on the resolved TvdbId.
            Subject.Fetch().Series.Should().HaveCount(2);
        }

        [Test]
        public void ShouldMergeAndSyncDistinctSeriesFromEachList()
        {
            var lists = Enumerable.Range(1, 3)
                .Select(id => (Id: id, Series: new List<ImportListItemInfo> { new() { Title = $"Show {id}", TmdbId = id } }))
                .ToList();

            foreach (var (id, series) in lists)
            {
                WithList(id, true, true, new ImportListFetchResult { Series = series });
            }

            var result = Subject.Fetch();

            result.AnyFailure.Should().BeFalse();
            result.Series.Select(s => s.TmdbId).Should().BeEquivalentTo([1, 2, 3]);
            result.Series.Should().OnlyContain(s => s.ImportListId == s.TmdbId);

            foreach (var (id, series) in lists)
            {
                Mocker.GetMock<IImportListItemService>().Verify(v => v.SyncSeriesForList(series, id), Times.Once());
                Mocker.GetMock<IImportListStatusService>().Verify(v => v.UpdateListSyncStatus(id, false), Times.Once());
            }
        }

        [Test]
        public void ShouldStillSyncOtherListsWhenOneListThrows()
        {
            const int throwingListId = 1;
            const int goodListId = 2;

            var throwingList = WithList(throwingListId, true, true, new ImportListFetchResult());
            throwingList.Setup(s => s.Fetch()).Throws(new InvalidOperationException("boom"));

            var goodSeries = new List<ImportListItemInfo> { new() { Title = "Good", TmdbId = 10 } };
            WithList(goodListId, true, true, new ImportListFetchResult { Series = goodSeries });

            var result = Subject.Fetch();

            ExceptionVerification.ExpectedErrors(1);
            result.Series.Should().ContainSingle(s => s.TmdbId == 10);

            Mocker.GetMock<IImportListItemService>()
                .Verify(v => v.SyncSeriesForList(goodSeries, goodListId), Times.Once());
            Mocker.GetMock<IImportListItemService>()
                .Verify(v => v.SyncSeriesForList(It.IsAny<List<ImportListItemInfo>>(), throwingListId), Times.Never());
            Mocker.GetMock<IImportListStatusService>()
                .Verify(v => v.UpdateListSyncStatus(throwingListId, It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void ShouldFetchListsInParallel()
        {
            // Both fetches only complete if they are running at the same time.
            var barrier = new Barrier(2);
            var rendezvous = new List<bool>();

            WithList(1, true, true, new ImportListFetchResult()).Setup(s => s.Fetch()).Returns(Fetch);
            WithList(2, true, true, new ImportListFetchResult()).Setup(s => s.Fetch()).Returns(Fetch);

            Subject.Fetch();

            rendezvous.Should().Equal(true, true);
            barrier.Dispose();

            return;

            ImportListFetchResult Fetch()
            {
                var met = barrier.SignalAndWait(TimeSpan.FromSeconds(10));

                lock (rendezvous)
                {
                    rendezvous.Add(met);
                }

                return new ImportListFetchResult();
            }
        }

        [Test]
        public void ShouldDeduplicateItemsAcrossLists()
        {
            WithList(1, true, true, new ImportListFetchResult { Series = [new ImportListItemInfo { Title = "Doctor Who", TmdbId = 1 }] });
            WithList(2, true, true, new ImportListFetchResult { Series = [new ImportListItemInfo { Title = "Doctor Who", TmdbId = 1 }, new ImportListItemInfo { Title = "Other", TmdbId = 2 }] });

            var result = Subject.Fetch();

            result.Series.Select(s => s.TmdbId).Should().BeEquivalentTo([1, 2]);
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
                .Returns(new ImportListStatus { LastInfoSync = lastSync });

            if (syncDeletedCount.HasValue)
            {
                Mocker.GetMock<IImportListItemService>()
                    .Setup(v => v.SyncSeriesForList(It.IsAny<List<ImportListItemInfo>>(), id))
                    .Returns(syncDeletedCount.Value);
            }

            _importLists.Add(mockImportList.Object);

            return mockImportList;
        }
    }
}
