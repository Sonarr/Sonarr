using System;
using System.Collections.Generic;
using NLog;
using Newtonsoft.Json;
using NzbDrone.Common;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.DataAugmentation.DailySeries
{

    public interface IDailySeriesDataProxy
    {
        IEnumerable<int> GetDailySeriesIds();
    }

    public class DailySeriesDataProxy : IDailySeriesDataProxy
    {
        private readonly HttpProvider _httpProvider;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public DailySeriesDataProxy(HttpProvider httpProvider, IConfigService configService, Logger logger)
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

                var seriesIds = JsonConvert.DeserializeObject<List<int>>(dailySeriesIds);

                return seriesIds;
            }
            catch (Exception ex)
            {
                _logger.WarnException("Failed to get Daily Series", ex);
                return new List<int>();
            }

        }
    }
}