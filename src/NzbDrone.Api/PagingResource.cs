using System.Collections.Generic;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Api
{
    public class PagingResource<TModel>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string SortKey { get; set; }
        public SortDirection SortDirection { get; set; }
        public string FilterKey { get; set; }
        public string FilterValue { get; set; }
        public int TotalRecords { get; set; }
        public List<TModel> Records { get; set; }
    }
}
