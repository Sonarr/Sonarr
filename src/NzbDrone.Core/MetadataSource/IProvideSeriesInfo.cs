using System;
using System.Collections.Generic;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MetadataSource
{
    public interface IProvideSeriesInfo
    {
        Tuple<Series, List<Episode>> GetSeriesInfo(int tvdbSeriesId, Language language, string seasonType);
    }
}
