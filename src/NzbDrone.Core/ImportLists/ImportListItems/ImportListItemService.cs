using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.ThingiProvider.Events;

namespace NzbDrone.Core.ImportLists.ImportListItems
{
    public interface IImportListItemService
    {
        List<ImportListItemInfo> All();
        List<ImportListItemInfo> GetAllForLists(List<int> listIds);
        int SyncSeriesForList(List<ImportListItemInfo> listSeries, int listId);
    }

    public class ImportListItemService : IImportListItemService, IHandleAsync<ProviderDeletedEvent<IImportList>>
    {
        private readonly IImportListItemRepository _importListItemRepository;

        public ImportListItemService(IImportListItemRepository importListItemRepository)
        {
            _importListItemRepository = importListItemRepository;
        }

        public int SyncSeriesForList(List<ImportListItemInfo> listSeries, int listId)
        {
            var existingListSeries = GetAllForLists(new List<int> { listId });

            var toAdd = new List<ImportListItemInfo>();
            var toUpdate = new List<ImportListItemInfo>();

            listSeries.ForEach(item =>
            {
                var existingItem = FindItem(existingListSeries, item);

                if (existingItem == null)
                {
                    toAdd.Add(item);
                    return;
                }

                // Remove so we'll only be left with items to remove at the end
                existingListSeries.Remove(existingItem);
                toUpdate.Add(existingItem);

                existingItem.Title = item.Title;
                existingItem.Year = item.Year;
                existingItem.TvdbId = item.TvdbId;
                existingItem.ImdbId = item.ImdbId;
                existingItem.TmdbId = item.TmdbId;
                existingItem.MalId = item.MalId;
                existingItem.AniListId = item.AniListId;
                existingItem.ReleaseDate = item.ReleaseDate;
            });

            _importListItemRepository.InsertMany(toAdd);
            _importListItemRepository.UpdateMany(toUpdate);
            _importListItemRepository.DeleteMany(existingListSeries);

            return existingListSeries.Count;
        }

        public List<ImportListItemInfo> All()
        {
            return _importListItemRepository.All().ToList();
        }

        public List<ImportListItemInfo> GetAllForLists(List<int> listIds)
        {
            return _importListItemRepository.GetAllForLists(listIds).ToList();
        }

        public void HandleAsync(ProviderDeletedEvent<IImportList> message)
        {
            var seriesOnList = _importListItemRepository.GetAllForLists(new List<int> { message.ProviderId });
            _importListItemRepository.DeleteMany(seriesOnList);
        }

        private ImportListItemInfo FindItem(List<ImportListItemInfo> existingItems, ImportListItemInfo item)
        {
            return existingItems.FirstOrDefault(e =>
            {
                if (e.TvdbId > 0 && item.TvdbId > 0 && e.TvdbId == item.TvdbId)
                {
                    return true;
                }

                if (e.ImdbId.IsNotNullOrWhiteSpace() && item.ImdbId.IsNotNullOrWhiteSpace() && e.ImdbId == item.ImdbId)
                {
                    return true;
                }

                if (e.TmdbId > 0 && item.TmdbId > 0 && e.TmdbId == item.TmdbId)
                {
                    return true;
                }

                if (e.MalId > 0 && item.MalId > 0 && e.MalId == item.MalId)
                {
                    return true;
                }

                if (e.AniListId > 0 && item.AniListId > 0 && e.AniListId == item.AniListId)
                {
                    return true;
                }

                return false;
            });
        }
    }
}
