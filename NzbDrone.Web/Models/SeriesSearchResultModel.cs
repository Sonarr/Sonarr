using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NzbDrone.Web.Models
{
    public class SeriesSearchResultModel
    {
        public int TvDbId { get; set; }
        public string TvDbName { get; set; }
        public DateTime FirstAired { get; set; }
    }
}