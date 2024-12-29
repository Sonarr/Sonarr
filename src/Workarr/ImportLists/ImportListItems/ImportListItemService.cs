using NLog;
using Workarr.Messaging.Events;
using Workarr.Parser.Model;
using Workarr.ThingiProvider.Events;

namespace Workarr.ImportLists.ImportListItems
{
    public interface IImportListItemService
    {
        List<ImportListItemInfo> GetAllForLists(List<int> listIds);
        int SyncSeriesForList(List<ImportListItemInfo> listSeries, int listId);
        bool Exists(int tvdbId, string imdbId);
    }

    public class ImportListItemService : IImportListItemService, IHandleAsync<ProviderDeletedEvent<IImportList>>
    {
        private readonly IImportListItemInfoRepository _importListSeriesRepository;
        private readonly Logger _logger;

        public ImportListItemService(IImportListItemInfoRepository importListSeriesRepository,
                             Logger logger)
        {
            _importListSeriesRepository = importListSeriesRepository;
            _logger = logger;
        }

        public int SyncSeriesForList(List<ImportListItemInfo> listSeries, int listId)
        {
            var existingListSeries = GetAllForLists(new List<int> { listId });

            listSeries.ForEach(l => l.Id = existingListSeries.FirstOrDefault(e => e.TvdbId == l.TvdbId)?.Id ?? 0);

            _importListSeriesRepository.InsertMany(listSeries.Where(l => l.Id == 0).ToList());
            _importListSeriesRepository.UpdateMany(listSeries.Where(l => l.Id > 0).ToList());
            var toDelete = existingListSeries.Where(l => !listSeries.Any(x => x.TvdbId == l.TvdbId)).ToList();
            _importListSeriesRepository.DeleteMany(toDelete);

            return toDelete.Count;
        }

        public List<ImportListItemInfo> GetAllForLists(List<int> listIds)
        {
            return _importListSeriesRepository.GetAllForLists(listIds).ToList();
        }

        public void HandleAsync(ProviderDeletedEvent<IImportList> message)
        {
            var seriesOnList = _importListSeriesRepository.GetAllForLists(new List<int> { message.ProviderId });
            _importListSeriesRepository.DeleteMany(seriesOnList);
        }

        public bool Exists(int tvdbId, string imdbId)
        {
            return _importListSeriesRepository.Exists(tvdbId, imdbId);
        }
    }
}
