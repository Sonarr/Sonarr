using System.Linq;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers
{
    public interface ISeriesProvider
    {
        IQueryable<Series> GetSeries();
        Series GetSeries(int tvdbId);
        void SyncSeriesWithDisk();
    }
}