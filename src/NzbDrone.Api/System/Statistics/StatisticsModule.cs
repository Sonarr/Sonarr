using System;
using System.Linq;
using Nancy;
using Nancy.Routing;
using NLog;
using NzbDrone.Api.Extensions;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.History;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Statistics;

namespace NzbDrone.Api.System
{
    public class StatisticsModule : NzbDroneApiModule
    {
        private readonly IStatisticsService _statisticsService;
        private readonly Logger _logger;

        public StatisticsModule(IStatisticsService statisticsService, Logger logger)
            : base("system/statistics")
        {
            _statisticsService = statisticsService;
            _logger = logger;

            Get["/"] = x => GetGlobalStatistics();
            Get["/indexer"] = x => GetIndexerStatistics();
        }

        private Response GetGlobalStatistics()
        {
            return new
                {
                    Generated = DateTime.UtcNow,
                    Uptime = GetUpTime(),
                    History = _statisticsService.GetGlobalStatistics()
                }.AsResponse();
        }

        private Response GetIndexerStatistics()
        {
            var stats = _statisticsService.GetIndexerStatistics();

            return stats.AsResponse();
        }

        private TimeSpan? GetUpTime()
        {
            try
            {
                return DateTime.Now - global::System.Diagnostics.Process.GetCurrentProcess().StartTime;
            }
            catch (Exception ex)
            {
                _logger.DebugException("Failed to get uptime", ex);
                return null;
            }
        }
    }
}
