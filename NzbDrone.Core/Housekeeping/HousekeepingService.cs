using System;
using System.Collections.Generic;
using NLog;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Housekeeping
{
    public interface IHousekeepingService
    {
        
    }

    public class HousekeepingService : IHousekeepingService, IExecute<HousekeepingCommand>
    {
        private readonly IEnumerable<IHousekeepingTask> _housekeepers;
        private readonly Logger _logger;

        public HousekeepingService(IEnumerable<IHousekeepingTask> housekeepers, Logger logger)
        {
            _housekeepers = housekeepers;
            _logger = logger;
        }

        public void Execute(HousekeepingCommand message)
        {
            _logger.Info("Running housecleaning tasks");

            foreach (var housekeeper in _housekeepers)
            {
                try
                {
                    housekeeper.Clean();
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Error running housekeeping task: " + housekeeper.GetType().FullName, ex);
                }
            }
        }
    }
}
