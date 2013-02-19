using System.Linq;
using System;
using System.Collections.Generic;
using PetaPoco;

namespace NzbDrone.Core.Tv
{
    public class Season
    {
        public int SeasonId { get; set; }
        public int SeriesId { get; set; }
        public int SeasonNumber { get; set; }
        public Boolean Ignored { get; set; }

        public List<Episode> Episodes { get; set; }
    }
}