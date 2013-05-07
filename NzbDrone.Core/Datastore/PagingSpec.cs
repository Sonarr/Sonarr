using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Datastore
{
    public class PagingSpec<TModel>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public string SortKey { get; set; }
        public SortDirection SortDirection { get; set; }
        public List<TModel> Records { get; set; }
    }

    public enum SortDirection
    {
        Ascending,
        Descending
    }
}
