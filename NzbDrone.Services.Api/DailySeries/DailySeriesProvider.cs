using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace NzbDrone.Services.Api.DailySeries
{
    public class DailySeriesProvider
    {
        private readonly MongoDatabase _mongoDb;

        public DailySeriesProvider(MongoDatabase mongoDb)
        {
            _mongoDb = mongoDb;
        }

        public DailySeriesProvider()
        {
        }

        public List<DailySeriesModel> All()
        {
            return _mongoDb.GetCollection<DailySeriesModel>(DailySeriesModel.CollectionName).FindAll().ToList();
        }

        public List<DailySeriesModel> Public()
        {
            var query = Query<DailySeriesModel>.EQ(d => d.Public, true);
            return _mongoDb.GetCollection<DailySeriesModel>(DailySeriesModel.CollectionName).Find(query).ToList();
        }

        public Boolean IsDaily(int seriesId)
        {
            var query = Query<DailySeriesModel>.EQ(d => d.Id, seriesId);
            return _mongoDb.GetCollection<DailySeriesModel>(DailySeriesModel.CollectionName).Count(query) > 0;
        }
    }
}