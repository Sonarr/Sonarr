using System.Collections.Generic;
using NLog;
using PetaPoco;

namespace NzbDrone.Core.Instrumentation
{
    public class LogProvider
    {
        private readonly IDatabase _database;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();



        public LogProvider(IDatabase database)
        {
            _database = database;
        }

        public IList<Log> GetAllLogs()
        {
            return _database.Fetch<Log>();
        }

        public void DeleteAll()
        {
            _database.Delete<Log>("");
            Logger.Info("Cleared Log History");
        }
    }
}