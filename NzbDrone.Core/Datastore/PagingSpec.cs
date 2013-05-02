using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        public ListSortDirection SortDirection { get; set; }
        public List<TModel> Records { get; set; } 
    }
}
