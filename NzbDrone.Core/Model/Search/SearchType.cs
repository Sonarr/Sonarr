using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Model.Search
{
    public enum SearchType
    {
        EpisodeSearch = 0,
        DailySearch = 1,
        PartialSeasonSearch = 2,
        SeasonSearch = 3
    }
}
