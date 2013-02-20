using System.Linq;
using System;
using System.Collections.Generic;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Tv
{
    public class Season : ModelBase
    {
        public int SeriesId { get; set; }
        public int SeasonNumber { get; set; }
        public Boolean Ignored { get; set; }

        public List<Episode> Episodes { get; set; }
    }
}