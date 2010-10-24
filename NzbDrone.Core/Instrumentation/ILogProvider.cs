using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.Instrumentation
{
    public interface ILogProvider
    {
        IQueryable<Log> GetAllLogs();
        void DeleteAll();
    }
}