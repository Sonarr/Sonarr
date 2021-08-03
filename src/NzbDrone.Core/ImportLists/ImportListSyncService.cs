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
            _logger.ProgressInfo("Starting Import List Sync");

            var rssReleases = _listFetcherAndParser.Fetch();

            var reports = rssReleases.ToList();

            ProcessReports(reports);
        }

        private void SyncList(ImportListDefinition definition)
        {
            _logger.ProgressInfo(string.Format("Starting Import List Refresh for List {0}", definition.Name));

            var rssReleases = _listFetcherAndParser.FetchSingleList(definition);

            var reports = rssReleases.ToList();

            ProcessReports(reports);
        }

        private void ProcessReports(List<ImportListItemInfo> reports)
        {
            var seriesToAdd = new List<Series>();

            _logger.ProgressInfo("Processing {0} list items", reports.Count);

            var reportNumber = 1;

            var listExclusions = _importListExclusionService.All();

            foreach (var report in reports)
            {
                _logger.ProgressTrace("Processing list item {0}/{1}", reportNumber, reports.Count);

                reportNumber++;

                var importList = _importListFactory.Get(report.ImportListId);

                // Map TVDb if we only have a series name
                if (report.TvdbId <= 0 && report.Title.IsNotNullOrWhiteSpace())
                {
                    var mappedSeries = _seriesSearchService.SearchForNewSeries(report.Title)
                        .FirstOrDefault();

                    if (mappedSeries != null)
                    {
                        report.TvdbId = mappedSeries.TvdbId;
                        report.Title = mappedSeries?.Title;
                    }
                }

                // Check to see if series in DB
                var existingSeries = _seriesService.FindByTvdbId(report.TvdbId);

                // Break if Series Exists in DB
                if (existingSeries != null)
                {
                    _logger.Debug("{0} [{1}] Rejected, Series Exists in DB", report.TvdbId, report.Title);
                    continue;
                }

                // Check to see if series excluded
                var excludedSeries = listExclusions.Where(s => s.TvdbId == report.TvdbId).SingleOrDefault();

                if (excludedSeries != null)
                {
                    _logger.Debug("{0} [{1}] Rejected due to list exlcusion", report.TvdbId, report.Title);
                    continue;
                }

                // Append Series if not already in DB or already on add list
                if (seriesToAdd.All(s => s.TvdbId != report.TvdbId))
                {
                    var monitored = importList.ShouldMonitor != MonitorTypes.None;

                    seriesToAdd.Add(new Series
                    {
                        TvdbId = report.TvdbId,
                        Title = report.Title,
                        Year = report.Year,
                        Monitored = monitored,
                        RootFolderPath = importList.RootFolderPath,
                        QualityProfileId = importList.QualityProfileId,
                        LanguageProfileId = importList.LanguageProfileId,
                        SeriesType = importList.SeriesType,
                        SeasonFolder = importList.SeasonFolder,
                        Tags = importList.Tags,
                        AddOptions = new AddSeriesOptions
                                     {
                                         SearchForMissingEpisodes = monitored,
                                         Monitor = importList.ShouldMonitor
                                     }
                    });
                }
            }

            _addSeriesService.AddSeries(seriesToAdd, true);

            var message = string.Format("Import List Sync Completed. Items found: {0}, Series added: {1}", reports.Count, seriesToAdd.Count);

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
