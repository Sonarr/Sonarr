using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MetadataSource
{
    public interface IProvideSeriesInfo
    {
        Series GetSeriesInfo(int tvDbSeriesId);
    }
}