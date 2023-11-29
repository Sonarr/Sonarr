using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.ImportLists.Exclusions;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.ImportLists
{
    public class ImportListSyncService : IExecute<ImportListSyncCommand>
    {
        private readonly IImportListFactory _importListFactory;
        private readonly IImportListExclusionService _importListExclusionService;
        private readonly IFetchAndParseImportList _listFetcherAndParser;
        private readonly ISearchForNewSeries _seriesSearchService;
        private readonly ISeriesService _seriesService;
        private readonly IAddSeriesService _addSeriesService;
        private readonly Logger _logger;

        public ImportListSyncService(IImportListFactory importListFactory,
                              IImportListExclusionService importListExclusionService,
                              IFetchAndParseImportList listFetcherAndParser,
                              ISearchForNewSeries seriesSearchService,
                              ISeriesService seriesService,
                              IAddSeriesService addSeriesService,
                              Logger logger)
        {
            _importListFactory = importListFactory;
            _importListExclusionService = importListExclusionService;
            _listFetcherAndParser = listFetcherAndParser;
            _seriesSearchService = seriesSearchService;
            _seriesService = seriesService;
            _addSeriesService = addSeriesService;
            _logger = logger;
        }

        private void SyncAll()
        {
            if (_importListFactory.AutomaticAddEnabled().Empty())
            {
                _logger.Debug("No import lists with automatic add enabled");

                return;
            }

            _logger.ProgressInfo("Starting Import List Sync");

            var listItems = _listFetcherAndParser.Fetch().ToList();

            ProcessListItems(listItems);
        }

        private void SyncList(ImportListDefinition definition)
        {
            _logger.ProgressInfo(string.Format("Starting Import List Refresh for List {0}", definition.Name));

            var listItems = _listFetcherAndParser.FetchSingleList(definition).ToList();

            ProcessListItems(listItems);
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

                // Map TVDb if we only have a series name
                if (item.TvdbId <= 0 && item.Title.IsNotNullOrWhiteSpace())
                {
                    var mappedSeries = _seriesSearchService.SearchForNewSeries(item.Title)
                        .FirstOrDefault();

                    if (mappedSeries != null)
                    {
                        item.TvdbId = mappedSeries.TvdbId;
                        item.Title = mappedSeries?.Title;
                    }
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
                    _logger.Debug("{0} [{1}] Rejected, Series Exists in DB", item.TvdbId, item.Title);
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
                        Tags = importList.Tags,
                        AddOptions = new AddSeriesOptions
                                     {
                                         SearchForMissingEpisodes = importList.SearchForMissingEpisodes,
                                         Monitor = importList.ShouldMonitor
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
    }
}
