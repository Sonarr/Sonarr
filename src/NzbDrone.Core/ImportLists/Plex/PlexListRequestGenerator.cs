using System.Collections.Generic;
using NzbDrone.Core.Notifications.Plex.PlexTv;

namespace NzbDrone.Core.ImportLists.Plex
{
    public class PlexListRequestGenerator : IImportListRequestGenerator
    {
        private const int MaxPages = 10;

        private readonly IPlexTvService _plexTvService;
        private readonly PlexListSettings _settings;
        private readonly int _pageSize;

        public PlexListRequestGenerator(IPlexTvService plexTvService, PlexListSettings settings, int pageSize)
        {
            _plexTvService = plexTvService;
            _settings = settings;
            _pageSize = pageSize;
        }

        public virtual ImportListPageableRequestChain GetListItems()
        {
            var pageableRequests = new ImportListPageableRequestChain();

            pageableRequests.Add(GetSeriesRequest());

            return pageableRequests;
        }

        private IEnumerable<ImportListRequest> GetSeriesRequest()
        {
            for (var page = 0; page < MaxPages; page++)
            {
                yield return new ImportListRequest(_plexTvService.GetWatchlist(_settings.AccessToken, _pageSize, page * _pageSize));
            }
        }
    }
}
