using System.Collections.Generic;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MetadataSource
{
    public interface IProvideEpisodeInfo
    {
        IList<Episode> GetEpisodeInfo(int tvDbSeriesId);
    }
}