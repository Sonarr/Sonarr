using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NzbDrone.Web.Models
{
    public class SeasonEditModel
    {
        public int SeriesId { get; set; }
        public int SeasonNumber { get; set; }
        public string SeasonString { get; set; }
        public bool Monitored { get; set; }
    }
}