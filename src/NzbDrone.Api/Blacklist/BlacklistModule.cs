using System;
using NzbDrone.Core.Blacklisting;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.History;

namespace NzbDrone.Api.Blacklist
{
    public class BlacklistModule : NzbDroneRestModule<BlacklistResource>
    {
        private readonly IBlacklistService _blacklistService;

        public BlacklistModule(IBlacklistService blacklistService)
        {
            _blacklistService = blacklistService;
            GetResourcePaged = GetBlacklist;
            DeleteResource = DeleteBlacklist;
        }

        private PagingResource<BlacklistResource> GetBlacklist(PagingResource<BlacklistResource> pagingResource)
        {
            var pagingSpec = new PagingSpec<Core.Blacklisting.Blacklist>
                                 {
                                     Page = pagingResource.Page,
                                     PageSize = pagingResource.PageSize,
                                     SortKey = pagingResource.SortKey,
                                     SortDirection = pagingResource.SortDirection
                                 };

            var mediaType = Request.Query.mediaType;

            if (mediaType.HasValue)
            {
                var type = (MediaType)Convert.ToInt32(mediaType.Value);
                switch (type)
                {
                    case MediaType.Series:
                        pagingSpec.FilterExpression = h => h.SeriesId > 0;
                        break;
                    case MediaType.Movies:
                        pagingSpec.FilterExpression = h => h.MovieId > 0;
                        break;
                }
            }

            return ApplyToPage(_blacklistService.Paged, pagingSpec);
        }

        private void DeleteBlacklist(int id)
        {
            _blacklistService.Delete(id);
        }
    }
}