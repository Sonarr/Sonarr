using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common;
using PetaPoco;

namespace NzbDrone.Core.Instrumentation
{
    public class LogProvider
    {
        private readonly IDatabase _database;
        private readonly LogDbContext _logDbContext;
        private readonly DiskProvider _diskProvider;
        private readonly EnvironmentProvider _environmentProvider;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();



        public LogProvider(IDatabase database, LogDbContext logDbContext, DiskProvider diskProvider, EnvironmentProvider environmentProvider)
        {
            _database = database;
            _logDbContext = logDbContext;
            _diskProvider = diskProvider;
            _environmentProvider = environmentProvider;
        }

        public IQueryable<Log> GetAllLogs()
        {
            return _logDbContext.Logs;
        }

        public IList<Log> TopLogs(int count)
        {
            var logs = _database.Fetch<Log>("SELECT TOP " + count + " * FROM Logs ORDER BY Time Desc");
            logs.Add(new Log
                         {
                             Time = DateTime.Now.AddYears(-100),
                             Level = "Info",
                             Logger = "Core.Instrumentation.LogProvider",
                             Message = String.Format("Number of logs currently shown: {0}. More may exist, check 'All' to see everything", Math.Min(count, logs.Count))
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
            _diskProvider.DeleteFile(_environmentProvider.GetLogFileName());
            _diskProvider.DeleteFile(_environmentProvider.GetArchivedLogFileName());
            Logger.Info("Cleared Log History");
        }

        public void Trim()
        {
            _database.Delete<Log>("WHERE Time < @0", DateTime.Now.AddDays(-30).Date);
            Logger.Debug("Logs have been trimmed, events older than 30 days have been removed");
        }
    }
}