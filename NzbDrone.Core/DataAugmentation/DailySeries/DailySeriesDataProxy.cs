using System;
using System.Collections.Generic;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.DataAugmentation.DailySeries
{

    public interface IDailySeriesDataProxy
    {
        IEnumerable<int> GetDailySeriesIds();
        bool IsDailySeries(int tvdbid);
    }

    public class DailySeriesDataProxy : IDailySeriesDataProxy
    {
        private readonly IHttpProvider _httpProvider;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public DailySeriesDataProxy(IHttpProvider httpProvider, IConfigService configService, Logger logger)
        {
            _httpProvider = httpProvider;
            _configService = configService;
            _logger = logger;
        }

        public IEnumerable<int> GetDailySeriesIds()
        {
            try
            {
                var dailySeriesIds = _httpProvider.DownloadString(_configService.ServiceRootUrl + "/DailySeries/AllIds");

                var seriesIds = Json.Deserialize<List<int>>(dailySeriesIds);

                return seriesIds;
            }
            catch (Exception ex)
            {
                _logger.WarnException("Failed to get Daily Series", ex);
                return new List<int>();
            }

        }

        public bool IsDailySeries(int tvdbid)
        {
            try
            {
                var result = _httpProvider.DownloadString(_configService.ServiceRootUrl + "/DailySeries/Check?seriesId=" + tvdbid);
                return Convert.ToBoolean(result);
            }
            catch (Exception ex)
            {
                _logger.WarnException("Failed to check Daily Series status for: " + tvdbid, ex);
                return false;
            }
        }
    }
}