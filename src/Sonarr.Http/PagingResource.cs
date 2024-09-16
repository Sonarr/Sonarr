using System.Collections.Generic;
using System.ComponentModel;
using NzbDrone.Core.Datastore;

namespace Sonarr.Http
{
    public class PagingRequestResource
    {
        [DefaultValue(1)]
        public int? Page { get; set; }
        [DefaultValue(10)]
        public int? PageSize { get; set; }
        public string SortKey { get; set; }
        public SortDirection? SortDirection { get; set; }
    }

    public class PagingResource<TResource>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string SortKey { get; set; }
        public SortDirection SortDirection { get; set; }
        public int TotalRecords { get; set; }
        public List<TResource> Records { get; set; } = new ();

        public PagingResource()
        {
        }

        public PagingResource(PagingRequestResource requestResource)
        {
            Page = requestResource.Page ?? 1;
            PageSize = requestResource.PageSize ?? 10;
            SortKey = requestResource.SortKey;
            SortDirection = requestResource.SortDirection ?? SortDirection.Descending;
        }
    }

    public static class PagingResourceMapper
    {
        public static PagingSpec<TModel> MapToPagingSpec<TResource, TModel>(
            this PagingResource<TResource> pagingResource,
            HashSet<string> allowedSortKeys,
            string defaultSortKey = "id",
            SortDirection defaultSortDirection = SortDirection.Ascending)
        {
            var pagingSpec = new PagingSpec<TModel>
            {
                Page = pagingResource.Page,
                PageSize = pagingResource.PageSize,
                SortKey = pagingResource.SortKey,
                SortDirection = pagingResource.SortDirection,
            };

            pagingSpec.SortKey = pagingResource.SortKey != null &&
                                 allowedSortKeys is { Count: > 0 } &&
                                 allowedSortKeys.Contains(pagingResource.SortKey)
                ? pagingResource.SortKey
                : defaultSortKey;

            pagingSpec.SortDirection = pagingResource.SortDirection == SortDirection.Default
                ? defaultSortDirection
                : pagingResource.SortDirection;

            return pagingSpec;
        }
    }
}
