using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.ImportLists.Exclusions;
using NzbDrone.Core.ImportLists.ImportListItems;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.ThingiProvider.Events;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.ImportLists
{
    public class ImportListSyncService : IExecute<ImportListSyncCommand>, IHandleAsync<ProviderDeletedEvent<IImportList>>
    {
        private readonly IImportListFactory _importListFactory;
        private readonly IImportListStatusService _importListStatusService;
        private readonly IImportListExclusionService _importListExclusionService;
        private readonly IImportListItemService _importListItemService;
        private readonly IFetchAndParseImportList _listFetcherAndParser;
        private readonly ISearchForNewSeries _seriesSearchService;
        private readonly ISeriesService _seriesService;
        private readonly IAddSeriesService _addSeriesService;
        private readonly IConfigService _configService;
        private readonly ITaskManager _taskManager;
        private readonly Logger _logger;

        public ImportListSyncService(IImportListFactory importListFactory,
                              IImportListStatusService importListStatusService,
                              IImportListExclusionService importListExclusionService,
                              IImportListItemService importListItemService,
                              IFetchAndParseImportList listFetcherAndParser,
                              ISearchForNewSeries seriesSearchService,
                              ISeriesService seriesService,
                              IAddSeriesService addSeriesService,
                              IConfigService configService,
                              ITaskManager taskManager,
                              Logger logger)
        {
            _importListFactory = importListFactory;
            _importListStatusService = importListStatusService;
            _importListExclusionService = importListExclusionService;
            _importListItemService = importListItemService;
            _listFetcherAndParser = listFetcherAndParser;
            _seriesSearchService = seriesSearchService;
            _seriesService = seriesService;
            _addSeriesService = addSeriesService;
            _configService = configService;
            _taskManager = taskManager;
            _logger = logger;
        }

        private bool AllListsSuccessfulWithAPendingClean()
        {
            var lists = _importListFactory.AutomaticAddEnabled(false);
            var anyRemoved = false;

            foreach (var list in lists)
            {
                var status = _importListStatusService.GetListStatus(list.Definition.Id);

                if (status.DisabledTill.HasValue)
                {
                    // list failed the last time it was synced.
                    return false;
                }

                if (!status.LastInfoSync.HasValue)
                {
                    // list has never been synced.
                    return false;
                }

                anyRemoved |= status.HasRemovedItemSinceLastClean;
            }

            return anyRemoved;
        }

        private void SyncAll()
        {
            if (_importListFactory.AutomaticAddEnabled().Empty())
            {
                _logger.Debug("No import lists with automatic add enabled");

                return;
            }

            _logger.ProgressInfo("Starting Import List Sync");

            var result = _listFetcherAndParser.Fetch();

            var listItems = result.Series.ToList();

            ProcessListItems(listItems);

            TryCleanLibrary();
        }

        private void SyncList(ImportListDefinition definition)
        {
            _logger.ProgressInfo(string.Format("Starting Import List Refresh for List {0}", definition.Name));

            var result = _listFetcherAndParser.FetchSingleList(definition);

            var listItems = result.Series.ToList();

            ProcessListItems(listItems);

            TryCleanLibrary();
        }

        private void ProcessListItems(List<ImportListItemInfo> items)
        {
            var seriesToAdd = new List<Series>();

            if (items.Count == 0)
            {
                _logger.ProgressInfo("No list items to process");

                return;
            }

            _logger.ProgressInfo("Processing {0} list items", items.Count);

            var reportNumber = 1;

            var listExclusions = _importListExclusionService.All();
            var importLists = _importListFactory.All();
            var existingTvdbIds = _seriesService.AllSeriesTvdbIds();

            foreach (var item in items)
            {
                _logger.ProgressTrace("Processing list item {0}/{1}", reportNumber, items.Count);

                reportNumber++;

                var importList = importLists.Single(x => x.Id == item.ImportListId);

                if (!importList.EnableAutomaticAdd)
                {
                    continue;
                }

                // Map by IMDb ID if we have it
                if (item.TvdbId <= 0 && item.ImdbId.IsNotNullOrWhiteSpace())
                {
                    var mappedSeries = _seriesSearchService.SearchForNewSeriesByImdbId(item.ImdbId)
                        .FirstOrDefault();

                    if (mappedSeries != null)
                    {
                        item.TvdbId = mappedSeries.TvdbId;
                        item.Title = mappedSeries?.Title;
                    }
                }

                // Map by TMDb ID if we have it
                if (item.TvdbId <= 0 && item.TmdbId > 0)
                {
                    var mappedSeries = _seriesSearchService.SearchForNewSeriesByTmdbId(item.TmdbId)
                        .FirstOrDefault();

                    if (mappedSeries != null)
                    {
                        item.TvdbId = mappedSeries.TvdbId;
                        item.Title = mappedSeries?.Title;
                    }
                }

                // Map by AniList ID if we have it
                if (item.TvdbId <= 0 && item.AniListId > 0)
                {
                    var mappedSeries = _seriesSearchService.SearchForNewSeriesByAniListId(item.AniListId)
                        .FirstOrDefault();

                    if (mappedSeries == null)
                    {
                        _logger.Debug("Rejected, unable to find matching TVDB ID for Anilist ID: {0} [{1}]", item.AniListId, item.Title);

                        continue;
                    }

                    item.TvdbId = mappedSeries.TvdbId;
                    item.Title = mappedSeries.Title;
                }

                // Map by MyAniList ID if we have it
                if (item.TvdbId <= 0 && item.MalId > 0)
                {
                    var mappedSeries = _seriesSearchService.SearchForNewSeriesByMyAnimeListId(item.MalId)
                        .FirstOrDefault();

                    if (mappedSeries == null)
                    {
                        _logger.Debug("Rejected, unable to find matching TVDB ID for MAL ID: {0} [{1}]", item.MalId, item.Title);

                        continue;
                    }

                    item.TvdbId = mappedSeries.TvdbId;
                    item.Title = mappedSeries.Title;
                }

                if (item.TvdbId == 0)
                {
                    _logger.Debug("[{0}] Rejected, unable to find TVDB ID", item.Title);
                    continue;
                }

                // Check to see if series excluded
                var excludedSeries = listExclusions.Where(s => s.TvdbId == item.TvdbId).SingleOrDefault();

                if (excludedSeries != null)
                {
                    _logger.Debug("{0} [{1}] Rejected due to list exclusion", item.TvdbId, item.Title);
                    continue;
                }

                // Break if Series Exists in DB
                if (existingTvdbIds.Any(x => x == item.TvdbId))
                {
                    _logger.Debug("{0} [{1}] Rejected, series exists in database", item.TvdbId, item.Title);
                    continue;
                }

                // Append Series if not already in DB or already on add list
                if (seriesToAdd.All(s => s.TvdbId != item.TvdbId))
                {
                    var monitored = importList.ShouldMonitor != MonitorTypes.None;

                    seriesToAdd.Add(new Series
                    {
                        TvdbId = item.TvdbId,
                        Title = item.Title,
                        Year = item.Year,
                        Monitored = monitored,
                        MonitorNewItems = importList.MonitorNewItems,
                        RootFolderPath = importList.RootFolderPath,
                        QualityProfileId = importList.QualityProfileId,
                        SeriesType = importList.SeriesType,
                        SeasonFolder = importList.SeasonFolder,
                        Seasons = item.Seasons,
                        Tags = importList.Tags,
                        AddOptions = new AddSeriesOptions
                        {
                            SearchForMissingEpisodes = importList.SearchForMissingEpisodes,

                            // If seasons are provided use them for syncing monitored status, otherwise use the list setting.
                            Monitor = item.Seasons.Any() ? MonitorTypes.Skip : importList.ShouldMonitor
                        }
                    });
                }
            }

            _addSeriesService.AddSeries(seriesToAdd, true);

            var message = string.Format("Import List Sync Completed. Items found: {0}, Series added: {1}", items.Count, seriesToAdd.Count);

            _logger.ProgressInfo(message);
        }

        public void Execute(ImportListSyncCommand message)
        {
            if (message.DefinitionId.HasValue)
            {
                SyncList(_importListFactory.Get(message.DefinitionId.Value));
            }
            else
            {
                SyncAll();
            }
        }

        private void TryCleanLibrary()
        {
            if (_configService.ListSyncLevel == ListSyncLevelType.Disabled)
            {
                return;
            }

            if (AllListsSuccessfulWithAPendingClean())
            {
                CleanLibrary();
            }
        }

        private void CleanLibrary()
        {
            if (_configService.ListSyncLevel == ListSyncLevelType.Disabled)
            {
                return;
            }

            var seriesToUpdate = new List<Series>();
            var seriesInLibrary = _seriesService.GetAllSeries();

            foreach (var series in seriesInLibrary)
            {
                var seriesExists = _importListItemService.Exists(series.TvdbId, series.ImdbId);

                if (!seriesExists)
                {
                    switch (_configService.ListSyncLevel)
                    {
                        case ListSyncLevelType.LogOnly:
                            _logger.Info("{0} was in your library, but not found in your lists --> You might want to unmonitor or remove it", series);
                            break;
                        case ListSyncLevelType.KeepAndUnmonitor when series.Monitored:
                            _logger.Info("{0} was in your library, but not found in your lists --> Keeping in library but unmonitoring it", series);
                            series.Monitored = false;
                            seriesToUpdate.Add(series);
                            break;
                        case ListSyncLevelType.KeepAndTag when !series.Tags.Contains(_configService.ListSyncTag):
                            _logger.Info("{0} was in your library, but not found in your lists --> Keeping in library but tagging it", series);
                            series.Tags.Add(_configService.ListSyncTag);
                            seriesToUpdate.Add(series);
                            break;
                        default:
                            break;
                    }
                }
            }

            _seriesService.UpdateSeries(seriesToUpdate, true);
            _importListStatusService.MarkListsAsCleaned();
        }

        public void HandleAsync(ProviderDeletedEvent<IImportList> message)
        {
            TryCleanLibrary();
        }
    }
}
