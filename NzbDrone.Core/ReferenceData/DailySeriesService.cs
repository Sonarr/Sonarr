using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Newtonsoft.Json;
using NzbDrone.Common;
using NzbDrone.Core.Providers.Core;
using PetaPoco;

namespace NzbDrone.Core.ReferenceData
{
    public class DailySeriesService
    {
        private readonly IDatabase _database;
        private readonly HttpProvider _httpProvider;
        private readonly ConfigProvider _configProvider;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public DailySeriesService(IDatabase database, HttpProvider httpProvider, ConfigProvider configProvider)
        {
            _database = database;
            _httpProvider = httpProvider;
            _configProvider = configProvider;
        }

        public virtual void UpdateDailySeries()
        {
            //Update all series in DB
            //DailySeries.csv

            var seriesIds = GetDailySeriesIds();

            if (seriesIds.Any())
            {
                var dailySeriesString = String.Join(", ", seriesIds);
                var sql = String.Format("UPDATE Series SET IsDaily = 1 WHERE SeriesId in ({0})", dailySeriesString);

                _database.Execute(sql);
            }
        }

        public virtual bool IsSeriesDaily(int seriesId)
        {
            return GetDailySeriesIds().Contains(seriesId);
        }

        public List<int> GetDailySeriesIds()
        {
            try
            {
                var dailySeriesIds = _httpProvider.DownloadString(_configProvider.ServiceRootUrl + "/DailySeries/AllIds");

                var seriesIds = JsonConvert.DeserializeObject<List<int>>(dailySeriesIds);

                return seriesIds;
            }
            catch (Exception ex)
            {
                Logger.WarnException("Failed to get Daily Series", ex);
                return new List<int>();
            }

        }
    }
}
