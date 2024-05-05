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
        public static PagingSpec<TModel> MapToPagingSpec<TResource, TModel>(this PagingResource<TResource> pagingResource, string defaultSortKey = "Id", SortDirection defaultSortDirection = SortDirection.Ascending)
        {
            var pagingSpec = new PagingSpec<TModel>
            {
                Page = pagingResource.Page,
                PageSize = pagingResource.PageSize,
                SortKey = pagingResource.SortKey,
                SortDirection = pagingResource.SortDirection,
            };

            if (pagingResource.SortKey == null)
            {
                pagingSpec.SortKey = defaultSortKey;

                if (pagingResource.SortDirection == SortDirection.Default)
                {
                    pagingSpec.SortDirection = defaultSortDirection;
                }
            }

            return pagingSpec;
        }
    }
}
