using System;
using NzbDrone.Core.Blacklisting;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Api.Blacklist
{
    public class BlacklistModule : NzbDroneRestModule<BlacklistResource>
    {
        private readonly IBlacklistService _blacklistService;

        public BlacklistModule(IBlacklistService blacklistService)
        {
            _blacklistService = blacklistService;
            GetResourcePaged = GetBlacklist;
            DeleteResource = Delete;
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

            //This is a hack to deal with backgrid setting the sortKey to the column name instead of sortValue
            if (pagingSpec.SortKey.Equals("series", StringComparison.InvariantCultureIgnoreCase))
            {
                pagingSpec.SortKey = "series.title";
            }

            return ApplyToPage(_blacklistService.Paged, pagingSpec);
        }

        private void Delete(int id)
        {
            _blacklistService.Delete(id);
        }
    }
}