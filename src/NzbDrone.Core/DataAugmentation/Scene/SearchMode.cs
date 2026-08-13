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

        // Carries the SearchTitle bit so the generators that only care about a title search
        // are unaffected, the season searches check for this one to drop the season number.
        SeasonSearchTitle = SearchTitle | SeasonTitle
    }
}
