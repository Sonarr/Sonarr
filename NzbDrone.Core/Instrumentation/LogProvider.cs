using System;
using System.Collections.Generic;
using System.Linq;
using DataTables.Mvc.Core.Helpers;
using DataTables.Mvc.Core.Models;
using NLog;
using NzbDrone.Common;
using PetaPoco;

namespace NzbDrone.Core.Instrumentation
{
    public class LogProvider
    {
        private readonly IDatabase _database;
        private readonly DiskProvider _diskProvider;
        private readonly EnvironmentProvider _environmentProvider;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public LogProvider(IDatabase database, DiskProvider diskProvider, EnvironmentProvider environmentProvider)
        {
            _database = database;
            _diskProvider = diskProvider;
            _environmentProvider = environmentProvider;
        }

        public List<Log> GetAllLogs()
        {
            return _database.Fetch<Log>();
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

        public virtual Page<Log> GetPagedItems(DataTablesPageRequest pageRequest)
        {
            var query = Sql.Builder
                    .Select(@"*")
                    .From("Logs");

            var startPage = (pageRequest.DisplayLength == 0) ? 1 : pageRequest.DisplayStart / pageRequest.DisplayLength + 1;

            if (!string.IsNullOrEmpty(pageRequest.Search))
            {
                var whereClause = string.Join(" OR ", SqlBuilderHelper.GetSearchClause(pageRequest));

                if (!string.IsNullOrEmpty(whereClause))
                    query.Append("WHERE " + whereClause, "%" + pageRequest.Search + "%");
            }

            var orderBy = string.Join(",", SqlBuilderHelper.GetOrderByClause(pageRequest));

            if (!string.IsNullOrEmpty(orderBy))
            {
                query.Append("ORDER BY " + orderBy);
            }

            return _database.Page<Log>(startPage, pageRequest.DisplayLength, query);
        }

        public virtual long Count()
        {
            return _database.Single<long>(@"SELECT COUNT(*) from Logs");
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