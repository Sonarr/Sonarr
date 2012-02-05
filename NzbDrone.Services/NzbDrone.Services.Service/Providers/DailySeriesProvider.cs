using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NzbDrone.Services.Service.Repository;
using PetaPoco;

namespace NzbDrone.Services.Service.Providers
{
    public class DailySeriesProvider
    {
        private readonly IDatabase _database;

        public DailySeriesProvider(IDatabase database)
        {
            _database = database;
        }

        public IList<DailySeries> All()
        {
            return _database.Fetch<DailySeries>();
        }

        public IList<int> AllSeriesIds()
        {
            return _database.Fetch<int>("SELECT Id from DailySeries");
        }

        public bool IsDaily(int seriesId)
        {
            return _database.Exists<DailySeries>(seriesId);
        }
    }
}