using System;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Jobs;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class FixFutureRunScheduledTasks : IHousekeepingTask
    {
        private readonly IDatabase _database;
        private readonly Logger _logger;

        public FixFutureRunScheduledTasks(IDatabase database, Logger logger)
        {
            _database = database;
            _logger = logger;
        }

        public void Clean()
        {
            if (BuildInfo.IsDebug)
            {
                _logger.Trace("Not running scheduled task last execution cleanup during debug");
            }

            _logger.Trace("Running scheduled task last execution cleanup");

            var mapper = _database.GetDataMapper();
            mapper.AddParameter("time", DateTime.UtcNow);

            mapper.ExecuteNonQuery(@"UPDATE ScheduledTasks
                                     SET LastExecution = @time
                                     WHERE LastExecution > @time");
        }
    }
}
