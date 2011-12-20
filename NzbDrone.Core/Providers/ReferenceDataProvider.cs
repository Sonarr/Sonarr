using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Core.Providers.Core;
using PetaPoco;

namespace NzbDrone.Core.Providers
{
    public class ReferenceDataProvider
    {
        private readonly IDatabase _database;
        private readonly HttpProvider _httpProvider;
        
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ReferenceDataProvider(IDatabase database, HttpProvider httpProvider)
        {
            _database = database;
            _httpProvider = httpProvider;
        }

        public virtual void UpdateDailySeries()
        {
            //Update all series in DB
            //DailySeries.csv

            var seriesIds = GetDailySeriesIds();

            var dailySeriesString = String.Join(", ", seriesIds);
            var sql = String.Format("UPDATE Series SET IsDaily = 1 WHERE SeriesId in ({0})", dailySeriesString);

            _database.Execute(sql);
        }

        public virtual bool IsSeriesDaily(int seriesId)
        {
            return GetDailySeriesIds().Contains(seriesId);
        }

        public List<int> GetDailySeriesIds()
        {
            try
            {
                var dailySeries = _httpProvider.DownloadString("http://www.nzbdrone.com/DailySeries.csv");

                var seriesIds = new List<int>();

                using (var reader = new StringReader(dailySeries))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        int seriesId;

                        //Split CSV, first item should be the seriesId
                        var split = line.Split(',');

                        if (Int32.TryParse(split[0], out seriesId))
                            seriesIds.Add(seriesId);
                    }
                }

                return seriesIds;
            }
            catch(Exception ex)
            {
                Logger.WarnException("Failed to get Daily Series", ex);
                return new List<int>();
            }
            
        }
    }
}
