using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Api.Series
{
    public class SeasonResource
    {
        public int SeasonNumber { get; set; }
        public Boolean Monitored { get; set; }
    }
}
