using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NzbDrone.Web.Models
{
    public class AddExistingSeriesModel
    {
        public bool IsWanted { get; set; }
        public string Path { get; set; }
        public string PathEncoded { get; set; }
        public int TvDbId { get; set; }
        public string TvDbName { get; set; }
    }
}