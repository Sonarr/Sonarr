using System;
using System.Collections.Generic;
using NLog;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Housekeeping
{
    public class HousekeepingService : IExecute<HousekeepingCommand>
    {
        private readonly IEnumerable<IHousekeepingTask> _housekeepers;
        private readonly Logger _logger;
        private readonly IMainDatabase _mainDb;

        public HousekeepingService(IEnumerable<IHousekeepingTask> housekeepers, IMainDatabase mainDb, Logger logger)
        {
            _housekeepers = housekeepers;
            _logger = logger;
            _mainDb = mainDb;
        }

        private void Clean()
        {
            _logger.Info("Running housecleaning tasks");

            foreach (var housekeeper in _housekeepers)
            {
                try
                {
                    _logger.Debug("Starting {0}", housekeeper.GetType().Name);
                    housekeeper.Clean();
                    _logger.Debug("Completed {0}", housekeeper.GetType().Name);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error running housekeeping task: {0}", housekeeper.GetType().Name);
                }
            }

            // Vacuuming the log db isn't needed since that's done in a separate housekeeping task
            _logger.Debug("Compressing main database after housekeeping");
            _mainDb.Vacuum();
        }

        public void Execute(HousekeepingCommand message)
        {
            Clean();
        }
    }
}
