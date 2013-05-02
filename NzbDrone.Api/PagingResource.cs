using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Api
{
    public class PagingResource<TModel>
    {
        public int Page { get; set; }
        public string SortKey { get; set; }
        public int TotalRecords { get; set; }
        public List<TModel> Records { get; set; }
    }
}
