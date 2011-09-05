using System;
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

        public IList<Log> TopLogs()
        {
            var logs = _database.Fetch<Log>("SELECT TOP 7500 * FROM Logs ORDER BY Time Desc");
            logs.Add(new Log
                         {
                             Time = DateTime.Now.AddYears(-100),
                             Level = "Info",
                             Logger = "NzbDrone.Core.Instrumentation.LogProvider",
                             Message = String.Format("Number of logs currently shown: 7500. More may exist, check 'All' to see everything")
                         });

            return logs;
        }

        public Page<Log> GetPagedLogs(int pageNumber, int pageSize)
        {
            return _database.Page<Log>(pageNumber, pageSize, "SELECT * FROM Logs ORDER BY Time DESC");
        }

        public void DeleteAll()
        {
            _database.Delete<Log>("");
            Logger.Info("Cleared Log History");
        }
    }
}