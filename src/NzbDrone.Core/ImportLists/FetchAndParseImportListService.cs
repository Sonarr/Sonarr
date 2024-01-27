using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Common.TPL;
using NzbDrone.Core.ImportLists.ImportListItems;

namespace NzbDrone.Core.ImportLists
{
    public interface IFetchAndParseImportList
    {
        ImportListFetchResult Fetch();
        ImportListFetchResult FetchSingleList(ImportListDefinition definition);
    }

    public class FetchAndParseImportListService : IFetchAndParseImportList
    {
        private readonly IImportListFactory _importListFactory;
        private readonly IImportListStatusService _importListStatusService;
        private readonly IImportListItemService _importListItemService;
        private readonly Logger _logger;

        public FetchAndParseImportListService(IImportListFactory importListFactory, IImportListStatusService importListStatusService, IImportListItemService importListItemService, Logger logger)
        {
            _importListFactory = importListFactory;
            _importListStatusService = importListStatusService;
            _importListItemService = importListItemService;
            _logger = logger;
        }

        public ImportListFetchResult Fetch()
        {
            var result = new ImportListFetchResult();

            var importLists = _importListFactory.AutomaticAddEnabled();

            if (!importLists.Any())
            {
                _logger.Debug("No enabled import lists, skipping.");
                return result;
            }

            _logger.Debug("Available import lists {0}", importLists.Count);

            var taskList = new List<Task>();
            var taskFactory = new TaskFactory(TaskCreationOptions.LongRunning, TaskContinuationOptions.None);

            foreach (var importList in importLists)
            {
                var importListLocal = importList;
                var importListStatus = _importListStatusService.GetListStatus(importListLocal.Definition.Id).LastInfoSync;

                if (importListStatus.HasValue)
                {
                    var importListNextSync = importListStatus.Value + importListLocal.MinRefreshInterval;

                    if (DateTime.UtcNow < importListNextSync)
                    {
                        _logger.Trace("Skipping refresh of Import List {0} ({1}) due to minimum refresh interval. Next sync after {2}", importList.Name, importListLocal.Definition.Name, importListNextSync);
                        continue;
                    }
                }

                var task = taskFactory.StartNew(() =>
                     {
                         try
                         {
                             var fetchResult = importListLocal.Fetch();
                             var importListReports = fetchResult.Series;

                             lock (result)
                             {
                                 _logger.Debug("Found {0} reports from {1} ({2})", importListReports.Count, importList.Name, importListLocal.Definition.Name);

                                 if (!fetchResult.AnyFailure)
                                 {
                                     importListReports.ForEach(s => s.ImportListId = importList.Definition.Id);
                                     result.Series.AddRange(importListReports);
                                     var removed = _importListItemService.SyncSeriesForList(importListReports, importList.Definition.Id);
                                     _importListStatusService.UpdateListSyncStatus(importList.Definition.Id, removed > 0);
                                 }

                                 result.AnyFailure |= fetchResult.AnyFailure;
                             }
                         }
                         catch (Exception e)
                         {
                             _logger.Error(e, "Error during Import List Sync of {0} ({1})", importList.Name, importListLocal.Definition.Name);
                         }
                     }).LogExceptions();

                taskList.Add(task);
            }

            Task.WaitAll(taskList.ToArray());

            result.Series = result.Series.DistinctBy(r => new { r.TvdbId, r.ImdbId, r.Title }).ToList();

            _logger.Debug("Found {0} total reports from {1} lists", result.Series.Count, importLists.Count);

            return result;
        }

        public ImportListFetchResult FetchSingleList(ImportListDefinition definition)
        {
            var result = new ImportListFetchResult();

            var importList = _importListFactory.GetInstance(definition);

            if (importList == null || !definition.EnableAutomaticAdd)
            {
                _logger.Debug("Import List {0} ({1}) is not enabled, skipping.", importList.Name, importList.Definition.Name);
                return result;
            }

            var taskList = new List<Task>();
            var taskFactory = new TaskFactory(TaskCreationOptions.LongRunning, TaskContinuationOptions.None);

            var importListLocal = importList;

            var task = taskFactory.StartNew(() =>
            {
                try
                {
                    var fetchResult = importListLocal.Fetch();
                    var importListReports = fetchResult.Series;

                    lock (result)
                    {
                        _logger.Debug("Found {0} reports from {1} ({2})", importListReports.Count, importList.Name, importListLocal.Definition.Name);

                        if (!fetchResult.AnyFailure)
                        {
                            importListReports.ForEach(s => s.ImportListId = importList.Definition.Id);
                            result.Series.AddRange(importListReports);
                            var removed = _importListItemService.SyncSeriesForList(importListReports, importList.Definition.Id);
                            _importListStatusService.UpdateListSyncStatus(importList.Definition.Id, removed > 0);
                        }

                        result.AnyFailure |= fetchResult.AnyFailure;
                    }

                    result.AnyFailure |= fetchResult.AnyFailure;
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Error during Import List Sync of {0} ({1})", importList.Name, importListLocal.Definition.Name);
                }
            }).LogExceptions();

            taskList.Add(task);

            Task.WaitAll(taskList.ToArray());

            return result;
        }
    }
}
