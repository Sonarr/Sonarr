using System.Collections.Generic;
using NzbDrone.Core.Notifications.Plex.PlexTv;

namespace NzbDrone.Core.ImportLists.Plex
{
    public class PlexListRequestGenerator : IImportListRequestGenerator
    {
        private readonly IPlexTvService _plexTvService;
        public PlexListSettings Settings { get; set; }

        public PlexListRequestGenerator(IPlexTvService plexTvService)
        {
            _plexTvService = plexTvService;
        }

        public virtual ImportListPageableRequestChain GetListItems()
        {
            var pageableRequests = new ImportListPageableRequestChain();

            pageableRequests.Add(GetMoviesRequest());

            return pageableRequests;
        }

        private IEnumerable<ImportListRequest> GetMoviesRequest()
        {
            var request = new ImportListRequest(_plexTvService.GetWatchlist(Settings.AccessToken));

            yield return request;
        }
    }
}
