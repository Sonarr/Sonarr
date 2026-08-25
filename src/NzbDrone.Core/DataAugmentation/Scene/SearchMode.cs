using System;

namespace NzbDrone.Core.DataAugmentation.Scene
{
    [Flags]
    public enum SearchMode
    {
        Default = 0,
        SearchID = 1,
        SearchTitle = 2
    }
}
