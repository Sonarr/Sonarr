using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.ImportLists.Exclusions;
using NzbDrone.Core.ImportLists.ImportListItems;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.ImportListTests
{
    public class ImportListSyncServiceFixture : CoreTest<ImportListSyncService>
    {
        private ImportListFetchResult _importListFetch;
        private List<ImportListItemInfo> _list1Series;
        private List<ImportListItemInfo> _list2Series;

        private List<Series> _existingSeries;
        private List<IImportList> _importLists;
        private ImportListSyncCommand _commandAll;
        private ImportListSyncCommand _commandSingle;

        [SetUp]
        public void SetUp()
        {
            _importLists = new List<IImportList>();

            var item1 = new ImportListItemInfo()
            {
                Title = "Breaking Bad"
            };

            _list1Series = new List<ImportListItemInfo>() { item1 };

            _existingSeries = Builder<Series>.CreateListOfSize(3)
                .TheFirst(1)
                .With(s => s.TvdbId = 6)
                .With(s => s.ImdbId = "6")
                .With(s => s.TmdbId = 6)
                .With(s => s.MalIds = new HashSet<int> { 6 })
                .With(s => s.AniListIds = new HashSet<int> { 6 })
                .With(s => s.Monitored = true)
                .TheNext(1)
                .With(s => s.TvdbId = 7)
                .With(s => s.ImdbId = "7")
                .With(s => s.TmdbId = 7)
                .With(s => s.MalIds = new HashSet<int> { 7 })
                .With(s => s.AniListIds = new HashSet<int> { 7 })
                .With(s => s.Monitored = true)
                .TheNext(1)
                .With(s => s.TvdbId = 8)
                .With(s => s.ImdbId = "8")
                .With(s => s.TmdbId = 8)
                .With(s => s.MalIds = new HashSet<int> { 8 })
                .With(s => s.AniListIds = new HashSet<int> { 8 })
                .With(s => s.Monitored = true)
                .Build().ToList();

            _list2Series = Builder<ImportListItemInfo>.CreateListOfSize(3)
                .TheFirst(1)
                .With(s => s.TvdbId = 6)
                .With(s => s.ImdbId = "6")
                .With(s => s.TmdbId = 6)
                .With(s => s.MalId = 6)
                .With(s => s.AniListId = 6)
                .TheNext(1)
                .With(s => s.TvdbId = 7)
                .With(s => s.ImdbId = "7")
                .With(s => s.TmdbId = 7)
                .With(s => s.MalId = 7)
                .With(s => s.AniListId = 7)
                .TheNext(1)
                .With(s => s.TvdbId = 8)
                .With(s => s.ImdbId = "8")
                .With(s => s.TmdbId = 8)
                .With(s => s.MalId = 8)
                .With(s => s.AniListId = 8)
                .Build().ToList();

            _importListFetch = new ImportListFetchResult(_list1Series, false);

            _commandAll = new ImportListSyncCommand
            {
            };

            _commandSingle = new ImportListSyncCommand
            {
                DefinitionId = 1
            };

            var mockImportList = new Mock<IImportList>();

            Mocker.GetMock<ISeriesService>()
                  .Setup(v => v.AllSeriesTvdbIds())
                  .Returns(new List<int>());

            Mocker.GetMock<ISeriesService>()
                .Setup(v => v.GetAllSeries())
                .Returns(_existingSeries);

            Mocker.GetMock<ISearchForNewSeries>()
                  .Setup(v => v.SearchForNewSeries(It.IsAny<string>()))
                  .Returns(new List<Series>());

            Mocker.GetMock<ISearchForNewSeries>()
                  .Setup(v => v.SearchForNewSeriesByImdbId(It.IsAny<string>()))
                  .Returns(new List<Series>());

            Mocker.GetMock<IImportListFactory>()
                  .Setup(v => v.All())
                  .Returns(() => _importLists.Select(x => x.Definition as ImportListDefinition).ToList());

            Mocker.GetMock<IImportListFactory>()
                .Setup(v => v.GetAvailableProviders())
                .Returns(_importLists);

            Mocker.GetMock<IImportListFactory>()
                .Setup(v => v.AutomaticAddEnabled(It.IsAny<bool>()))
                .Returns(() => _importLists.Where(x => (x.Definition as ImportListDefinition).EnableAutomaticAdd).ToList());

            Mocker.GetMock<IFetchAndParseImportList>()
                  .Setup(v => v.Fetch())
                  .Returns(_importListFetch);

            Mocker.GetMock<IImportListExclusionService>()
                  .Setup(v => v.All())
                  .Returns(new List<ImportListExclusion>());

            Mocker.GetMock<IImportListItemService>()
                .Setup(s => s.All())
                .Returns(new List<ImportListItemInfo>());
        }

        private void WithTvdbId()
        {
            _list1Series.First().TvdbId = 81189;
        }

        private void WithImdbId()
        {
            _list1Series.First().ImdbId = "tt0496424";

            Mocker.GetMock<ISearchForNewSeries>()
                .Setup(s => s.SearchForNewSeriesByImdbId(_list1Series.First().ImdbId))
                .Returns(
                    Builder<Series>
                        .CreateListOfSize(1)
                        .All()
                        .With(s => s.Title = "Breaking Bad")
                        .With(s => s.TvdbId = 81189)
                        .Build()
                        .ToList());
        }

        private void WithExistingSeries()
        {
            Mocker.GetMock<ISeriesService>()
                  .Setup(v => v.AllSeriesTvdbIds())
                  .Returns(new List<int> { _list1Series.First().TvdbId });
        }

        private void WithExcludedSeries()
        {
            Mocker.GetMock<IImportListExclusionService>()
                  .Setup(v => v.All())
                  .Returns(new List<ImportListExclusion>
                    {
                      new ImportListExclusion
                        {
                          TvdbId = _list1Series.First().TvdbId
                        }
                    });
        }

        private List<ImportListItemInfo> WithImportListItems(int count)
        {
            var importListItems = Builder<ImportListItemInfo>.CreateListOfSize(count)
                .Build()
                .ToList();

            Mocker.GetMock<IImportListItemService>()
                .Setup(s => s.All())
                .Returns(importListItems);

            return importListItems;
        }

        private void WithMonitorType(MonitorTypes monitor)
        {
            _importLists.ForEach(li => (li.Definition as ImportListDefinition).ShouldMonitor = monitor);
        }

        private void WithCleanLevel(ListSyncLevelType cleanLevel, int? tagId = null)
        {
            Mocker.GetMock<IConfigService>()
                  .SetupGet(v => v.ListSyncLevel)
                  .Returns(cleanLevel);
            if (tagId.HasValue)
            {
                Mocker.GetMock<IConfigService>()
                    .SetupGet(v => v.ListSyncTag)
                    .Returns(tagId.Value);
            }
        }

        private void WithList(int id, bool enabledAuto, int lastSyncHoursOffset = 0, bool pendingRemovals = true, DateTime? disabledTill = null)
        {
            var importListDefinition = new ImportListDefinition { Id = id, EnableAutomaticAdd = enabledAuto };

            Mocker.GetMock<IImportListFactory>()
                  .Setup(v => v.Get(id))
                  .Returns(importListDefinition);

            var mockImportList = new Mock<IImportList>();
            mockImportList.SetupGet(s => s.Definition).Returns(importListDefinition);
            mockImportList.SetupGet(s => s.MinRefreshInterval).Returns(TimeSpan.FromHours(12));

            var status = new ImportListStatus()
            {
                LastInfoSync = DateTime.UtcNow.AddHours(lastSyncHoursOffset),
                HasRemovedItemSinceLastClean = pendingRemovals,
                DisabledTill = disabledTill
            };

            if (disabledTill.HasValue)
            {
                _importListFetch.AnyFailure = true;
            }

            Mocker.GetMock<IImportListStatusService>()
                .Setup(v => v.GetListStatus(id))
                .Returns(status);

            _importLists.Add(mockImportList.Object);
        }

        private void VerifyDidAddTag(int expectedSeriesCount, int expectedTagId)
        {
            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.UpdateSeries(It.Is<List<Series>>(x => x.Count == expectedSeriesCount && x.All(series => series.Tags.Contains(expectedTagId))), true), Times.Once());
        }

        [Test]
        public void should_not_clean_library_if_lists_have_not_removed_any_items()
        {
            _importListFetch.Series = _existingSeries.Select(x => new ImportListItemInfo() { TvdbId = x.TvdbId }).ToList();
            _importListFetch.Series.ForEach(m => m.ImportListId = 1);
            WithList(1, true, pendingRemovals: false);
            WithCleanLevel(ListSyncLevelType.KeepAndUnmonitor);

            Subject.Execute(_commandAll);

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.GetAllSeries(), Times.Never());

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.UpdateSeries(It.IsAny<List<Series>>(), true), Times.Never());
        }

        [Test]
        public void should_not_clean_library_if_config_value_disable()
        {
            _importListFetch.Series.ForEach(m => m.ImportListId = 1);
            WithList(1, true);
            WithCleanLevel(ListSyncLevelType.Disabled);

            Subject.Execute(_commandAll);

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.GetAllSeries(), Times.Never());

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.UpdateSeries(new List<Series>(), true), Times.Never());
        }

        [Test]
        public void should_log_only_on_clean_library_if_config_value_logonly()
        {
            _importListFetch.Series.ForEach(m => m.ImportListId = 1);
            WithList(1, true);
            WithCleanLevel(ListSyncLevelType.LogOnly);

            Subject.Execute(_commandAll);

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.GetAllSeries(), Times.Once());

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.DeleteSeries(It.IsAny<List<int>>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never());

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.UpdateSeries(new List<Series>(), true), Times.Once());
        }

        [Test]
        public void should_unmonitor_on_clean_library_if_config_value_keepAndUnmonitor()
        {
            _importListFetch.Series.ForEach(m => m.ImportListId = 1);
            WithList(1, true);
            WithCleanLevel(ListSyncLevelType.KeepAndUnmonitor);
            var monitored = _existingSeries.Count(x => x.Monitored);

            Subject.Execute(_commandAll);

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.GetAllSeries(), Times.Once());

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.DeleteSeries(It.IsAny<List<int>>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never());

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.UpdateSeries(It.Is<List<Series>>(s => s.Count == monitored && s.All(m => !m.Monitored)), true), Times.Once());
        }

        [Test]
        public void should_not_clean_on_clean_library_if_tvdb_match()
        {
            WithList(1, true);
            WithCleanLevel(ListSyncLevelType.KeepAndUnmonitor);
            var importListItems = WithImportListItems(_existingSeries.Count - 1);
            _importListFetch.Series.ForEach(m => m.ImportListId = 1);

            for (var i = 0; i < importListItems.Count; i++)
            {
                importListItems[i].TvdbId = _existingSeries[i].TvdbId;
            }

            Subject.Execute(_commandAll);

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.UpdateSeries(It.Is<List<Series>>(s => s.Count == 1 && s.All(m => !m.Monitored)), true), Times.Once());
        }

        [Test]
        public void should_not_clean_on_clean_library_if_imdb_match()
        {
            WithList(1, true);
            WithCleanLevel(ListSyncLevelType.KeepAndUnmonitor);
            var importListItems = WithImportListItems(_existingSeries.Count - 1);
            _importListFetch.Series.ForEach(m => m.ImportListId = 1);

            for (var i = 0; i < importListItems.Count; i++)
            {
                importListItems[i].ImdbId = _existingSeries[i].ImdbId;
            }

            Subject.Execute(_commandAll);

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.UpdateSeries(It.Is<List<Series>>(s => s.Count == 1 && s.All(m => !m.Monitored)), true), Times.Once());
        }

        [Test]
        public void should_not_clean_on_clean_library_if_tmdb_match()
        {
            WithList(1, true);
            WithCleanLevel(ListSyncLevelType.KeepAndUnmonitor);
            var importListItems = WithImportListItems(_existingSeries.Count - 1);
            _importListFetch.Series.ForEach(m => m.ImportListId = 1);

            for (var i = 0; i < importListItems.Count; i++)
            {
                importListItems[i].TmdbId = _existingSeries[i].TmdbId;
            }

            Subject.Execute(_commandAll);

            Mocker.GetMock<ISeriesService>()
                .Verify(v => v.UpdateSeries(It.Is<List<Series>>(s => s.Count == 1 && s.All(m => !m.Monitored)), true), Times.Once());
        }

        [Test]
        public void should_not_clean_on_clean_library_if_malid_match()
        {
            WithList(1, true);
            WithCleanLevel(ListSyncLevelType.KeepAndUnmonitor);
            var importListItems = WithImportListItems(_existingSeries.Count - 1);
            _importListFetch.Series.ForEach(m => m.ImportListId = 1);

            for (var i = 0; i < importListItems.Count; i++)
            {
                importListItems[i].MalId = _existingSeries[i].MalIds.First();
            }

            Subject.Execute(_commandAll);

            Mocker.GetMock<ISeriesService>()
                .Verify(v => v.UpdateSeries(It.Is<List<Series>>(s => s.Count == 1 && s.All(m => !m.Monitored)), true), Times.Once());
        }

        [Test]
        public void should_not_clean_on_clean_library_if_anilistid_match()
        {
            WithList(1, true);
            WithCleanLevel(ListSyncLevelType.KeepAndUnmonitor);
            var importListItems = WithImportListItems(_existingSeries.Count - 1);
            _importListFetch.Series.ForEach(m => m.ImportListId = 1);

            for (var i = 0; i < importListItems.Count; i++)
            {
                importListItems[i].AniListId = _existingSeries[i].AniListIds.First();
            }

            Subject.Execute(_commandAll);

            Mocker.GetMock<ISeriesService>()
                .Verify(v => v.UpdateSeries(It.Is<List<Series>>(s => s.Count == 1 && s.All(m => !m.Monitored)), true), Times.Once());
        }

        [Test]
        public void should_tag_series_on_clean_library_if_config_value_keepAndTag()
        {
            _importListFetch.Series.ForEach(m => m.ImportListId = 1);
            WithList(1, true);
            WithCleanLevel(ListSyncLevelType.KeepAndTag, 1);

            Subject.Execute(_commandAll);

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.GetAllSeries(), Times.Once());

            VerifyDidAddTag(_existingSeries.Count, 1);
        }

        [Test]
        public void should_not_clean_if_list_failures()
        {
            _importListFetch.Series.ForEach(m => m.ImportListId = 1);
            WithList(1, true, disabledTill: DateTime.UtcNow.AddHours(1));
            WithCleanLevel(ListSyncLevelType.LogOnly);

            Subject.Execute(_commandAll);

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.GetAllSeries(), Times.Never());

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.UpdateSeries(It.IsAny<List<Series>>(), It.IsAny<bool>()), Times.Never());

            Mocker.GetMock<ISeriesService>()
                .Verify(v => v.DeleteSeries(It.IsAny<List<int>>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void should_add_new_series_from_single_list_to_library()
        {
            _importListFetch.Series.ForEach(m => m.ImportListId = 1);
            WithTvdbId();
            WithList(1, true);
            WithCleanLevel(ListSyncLevelType.Disabled);

            Subject.Execute(_commandAll);

            Mocker.GetMock<IAddSeriesService>()
                  .Verify(v => v.AddSeries(It.Is<List<Series>>(s => s.Count == 1), true), Times.Once());
        }

        [Test]
        public void should_add_new_series_from_multiple_list_to_library()
        {
            _list2Series.ForEach(m => m.ImportListId = 2);
            _importListFetch.Series.ForEach(m => m.ImportListId = 1);
            _importListFetch.Series.AddRange(_list2Series);

            WithTvdbId();
            WithList(1, true);
            WithList(2, true);

            WithCleanLevel(ListSyncLevelType.Disabled);

            Subject.Execute(_commandAll);

            Mocker.GetMock<IAddSeriesService>()
                  .Verify(v => v.AddSeries(It.Is<List<Series>>(s => s.Count == 4), true), Times.Once());
        }

        [Test]
        public void should_add_new_series_to_library_only_from_enabled_lists()
        {
            _list2Series.ForEach(m => m.ImportListId = 2);
            _importListFetch.Series.ForEach(m => m.ImportListId = 1);
            _importListFetch.Series.AddRange(_list2Series);

            WithTvdbId();
            WithList(1, true);
            WithList(2, false);

            WithCleanLevel(ListSyncLevelType.Disabled);

            Subject.Execute(_commandAll);

            Mocker.GetMock<IAddSeriesService>()
                  .Verify(v => v.AddSeries(It.Is<List<Series>>(s => s.Count == 1), true), Times.Once());
        }

        [Test]
        public void should_not_add_duplicate_series_from_seperate_lists()
        {
            _list2Series.ForEach(m => m.ImportListId = 2);
            _importListFetch.Series.ForEach(m => m.ImportListId = 1);
            _importListFetch.Series.AddRange(_list2Series);
            _importListFetch.Series[0].TvdbId = 6;

            WithList(1, true);
            WithList(2, true);

            WithCleanLevel(ListSyncLevelType.Disabled);

            Subject.Execute(_commandAll);

            Mocker.GetMock<IAddSeriesService>()
                  .Verify(v => v.AddSeries(It.Is<List<Series>>(s => s.Count == 3), true), Times.Once());
        }

        [Test]
        public void should_not_search_if_series_title_and_series_id()
        {
            _importListFetch.Series.ForEach(m => m.ImportListId = 1);
            WithList(1, true);
            WithTvdbId();
            Subject.Execute(_commandAll);

            Mocker.GetMock<ISearchForNewSeries>()
                  .Verify(v => v.SearchForNewSeries(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_search_by_imdb_if_series_title_and_series_imdb()
        {
            _importListFetch.Series.ForEach(m => m.ImportListId = 1);

            WithList(1, true);
            WithImdbId();

            Subject.Execute(_commandAll);

            Mocker.GetMock<ISearchForNewSeries>()
                  .Verify(v => v.SearchForNewSeriesByImdbId(It.IsAny<string>()), Times.Once());

            Mocker.GetMock<IAddSeriesService>()
                .Verify(v => v.AddSeries(It.Is<List<Series>>(t => t.Count == 1), It.IsAny<bool>()));
        }

        [Test]
        public void should_not_add_if_existing_series()
        {
            _importListFetch.Series.ForEach(m => m.ImportListId = 1);
            WithList(1, true);
            WithTvdbId();
            WithExistingSeries();

            Subject.Execute(_commandAll);

            Mocker.GetMock<IAddSeriesService>()
                  .Verify(v => v.AddSeries(It.Is<List<Series>>(t => t.Count == 0), It.IsAny<bool>()));
        }

        [TestCase(MonitorTypes.None, false)]
        [TestCase(MonitorTypes.All, true)]
        public void should_add_if_not_existing_series(MonitorTypes monitor, bool expectedSeriesMonitored)
        {
            _importListFetch.Series.ForEach(m => m.ImportListId = 1);
            WithList(1, true);
            WithTvdbId();
            WithMonitorType(monitor);

            Subject.Execute(_commandAll);

            Mocker.GetMock<IAddSeriesService>()
                  .Verify(v => v.AddSeries(It.Is<List<Series>>(t => t.Count == 1 && t.First().Monitored == expectedSeriesMonitored), It.IsAny<bool>()));
        }

        [Test]
        public void should_not_add_if_excluded_series()
        {
            _importListFetch.Series.ForEach(m => m.ImportListId = 1);
            WithList(1, true);
            WithTvdbId();
            WithExcludedSeries();

            Subject.Execute(_commandAll);

            Mocker.GetMock<IAddSeriesService>()
                  .Verify(v => v.AddSeries(It.Is<List<Series>>(t => t.Count == 0), It.IsAny<bool>()));
        }

        [Test]
        public void should_not_fetch_if_no_lists_are_enabled()
        {
            Mocker.GetMock<IImportListFactory>()
                .Setup(v => v.AutomaticAddEnabled(It.IsAny<bool>()))
                .Returns(new List<IImportList>());

            Subject.Execute(new ImportListSyncCommand());

            Mocker.GetMock<IFetchAndParseImportList>()
                .Verify(v => v.Fetch(), Times.Never);
        }

        [Test]
        public void should_not_process_if_no_items_are_returned()
        {
            Mocker.GetMock<IFetchAndParseImportList>()
                .Setup(v => v.Fetch())
                .Returns(new ImportListFetchResult());

            Subject.Execute(new ImportListSyncCommand());

            Mocker.GetMock<IImportListExclusionService>()
                .Verify(v => v.All(), Times.Never);
        }

        [Test]
        public void should_not_add_if_tvdbid_is_0()
        {
            _importListFetch.Series.ForEach(m => m.ImportListId = 1);
            WithList(1, true);
            WithExcludedSeries();

            Subject.Execute(_commandAll);

            Mocker.GetMock<IAddSeriesService>()
                .Verify(v => v.AddSeries(It.Is<List<Series>>(t => t.Count == 0), It.IsAny<bool>()));
        }
    }
}
