using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using SubSonic.Repository;

namespace NzbDrone.Core.Instrumentation
{

    public class LogProvider : ILogProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IRepository _repository;

        public LogProvider(IRepository repository)
        {
            _repository = repository;
        }

        public IQueryable<Log> GetAllLogs()
        {
            return _repository.All<Log>();
        }

        public void DeleteAll()
        {
            _repository.DeleteMany(GetAllLogs());
            Logger.Info("Cleared Log History");
        }
    }
}
