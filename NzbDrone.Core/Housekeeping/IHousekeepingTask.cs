using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Housekeeping
{
    public interface IHousekeepingTask
    {
        void Clean();
    }
}