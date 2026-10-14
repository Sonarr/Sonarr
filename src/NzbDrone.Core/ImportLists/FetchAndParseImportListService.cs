using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Core.ImportLists.ImportListItems;

namespace NzbDrone.Core.ImportLists;

public interface IFetchAndParseImportList
{
    ImportListFetchResult Fetch();
    ImportListFetchResult FetchSingleList(ImportListDefinition definition);
}

public class FetchAndParseImportListService(IImportListFactory importListFactory,
                                            IImportListStatusService importListStatusService,
                                            IImportListItemService importListItemService,
                                            Logger logger)
    : IFetchAndParseImportList
{
    public ImportListFetchResult Fetch()
    {
        var result = new ImportListFetchResult();

        var importLists = importListFactory.AutomaticAddEnabled();

        if (!importLists.Any())
        {
            logger.Debug("No enabled import lists, skipping");
            return result;
        }

        logger.Debug(CultureInfo.InvariantCulture, "Available import lists {ImportListCount}", importLists.Count);

        // Fetch the lists in parallel; each task only returns its own result, so nothing is shared between threads.
        // ToList starts all the tasks before any of them is awaited.
        var fetches = importLists
            .Where(IsDueForRefresh)
            .Select(importList => (ImportList: importList, Task: Task.Factory.StartNew(importList.Fetch, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default)))
            .ToList();

        foreach (var (importList, task) in fetches)
        {
            TrySyncList(importList, task.GetAwaiter().GetResult, result);
        }

        result.Series = [.. result.Series.DistinctBy(r => new { r.TvdbId, r.TmdbId, r.ImdbId, r.Title })];

        logger.Debug("Found {ReportCount} total reports from {ImportListCount} lists", result.Series.Count, importLists.Count);

        return result;
    }

    public ImportListFetchResult FetchSingleList(ImportListDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        var result = new ImportListFetchResult();

        var importList = importListFactory.GetInstance(definition);

        if (importList == null || !definition.EnableAutomaticAdd)
        {
            logger.Debug(CultureInfo.InvariantCulture, "Import List {DefinitionName} is not enabled, skipping", definition.Name);
            return result;
        }

        TrySyncList(importList, importList.Fetch, result);

        return result;
    }

    private bool IsDueForRefresh(IImportList importList)
    {
        var lastSync = importListStatusService.GetListStatus(importList.Definition.Id).LastInfoSync;

        if (!lastSync.HasValue)
        {
            return true;
        }

        var nextSync = lastSync.Value + importList.MinRefreshInterval;

        if (DateTime.UtcNow >= nextSync)
        {
            return true;
        }

        logger.Trace("Skipping refresh of Import List {ImportListName} ({DefinitionName}) due to minimum refresh interval. Next sync after {NextSync}", importList.Name, importList.Definition.Name, nextSync);
        return false;
    }

    private void TrySyncList(IImportList importList, Func<ImportListFetchResult> getFetchResult, ImportListFetchResult result)
    {
        try
        {
            var fetchResult = getFetchResult();
            var importListReports = fetchResult.Series;

            logger.Debug("Found {ReportCount} reports from {ImportListName} ({DefinitionName})", importListReports.Count, importList.Name, importList.Definition.Name);

            if (!fetchResult.AnyFailure)
            {
                importListReports.ForEach(s => s.ImportListId = importList.Definition.Id);
                result.Series.AddRange(importListReports);
                var removed = importListItemService.SyncSeriesForList(importListReports, importList.Definition.Id);
                importListStatusService.UpdateListSyncStatus(importList.Definition.Id, removed > 0);
            }

            result.AnyFailure |= fetchResult.AnyFailure;
        }
        catch (Exception e)
        {
            logger.Error(e, "Error during Import List Sync of {ImportListName} ({DefinitionName})", importList.Name, importList.Definition.Name);
        }
    }
}
