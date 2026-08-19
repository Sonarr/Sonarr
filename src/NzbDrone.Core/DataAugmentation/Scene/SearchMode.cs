using System;

namespace NzbDrone.Core.DataAugmentation.Scene
{
    [Flags]
    public enum SearchMode
    {
        Default = 0,
        SearchID = 1,
        SearchTitle = 2,
        SeasonTitle = 4,

        // Includes SearchTitle so a season title is still a title search everywhere that only
        // looks for one, the season searches check for this to drop the season number.
        SearchSeasonTitle = SearchTitle | SeasonTitle
    }
}
