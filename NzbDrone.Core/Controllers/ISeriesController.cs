using System.Linq;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Controllers
{
    public interface ISeriesController
    {
        IQueryable<Series> GetSeries();
        void SyncSeriesWithDisk();
    }
}