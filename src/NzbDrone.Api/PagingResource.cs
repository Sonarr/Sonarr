using System.Collections.Generic;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Api
{
    public class PagingResource<TResource>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string SortKey { get; set; }
        public SortDirection SortDirection { get; set; }
        public string FilterKey { get; set; }
        public string FilterValue { get; set; }
        public int TotalRecords { get; set; }
        public List<TResource> Records { get; set; }
    }

    public static class PagingResourceMapper
    {
        public static PagingSpec<TModel> MapToPagingSpec<TResource, TModel>(this PagingResource<TResource> pagingSpec)
        {
            return new PagingSpec<TModel>
            {
                Page = pagingSpec.Page,
                PageSize = pagingSpec.PageSize,
                SortKey = pagingSpec.SortKey,
                SortDirection = pagingSpec.SortDirection,
            };
        }
    }
}
