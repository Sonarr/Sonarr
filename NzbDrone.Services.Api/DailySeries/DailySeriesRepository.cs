using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace NzbDrone.Services.Api.DailySeries
{
    public class DailySeriesRepository
    {
        private readonly MongoDatabase _mongoDb;

        public DailySeriesRepository(MongoDatabase mongoDb)
        {
            _mongoDb = mongoDb;
        }

        public DailySeriesRepository()
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

        public DailySeriesModel Find(int seriesId)
        {
            var query = Query<DailySeriesModel>.EQ(d => d.Id, seriesId);
            return _mongoDb.GetCollection<DailySeriesModel>(DailySeriesModel.CollectionName).FindOne(query);
        }

        public void Insert(DailySeriesModel dailySeries)
        {
            _mongoDb.GetCollection<DailySeriesModel>(DailySeriesModel.CollectionName).Insert(dailySeries);
        }

        public void Delete(int seriesId)
        {
            var query = Query<DailySeriesModel>.EQ(d => d.Id, seriesId);
            _mongoDb.GetCollection<DailySeriesModel>(DailySeriesModel.CollectionName).Remove(query);
        }

        public void TogglePublic(int seriesId, bool status)
        {
            var query = Query<DailySeriesModel>.EQ(d => d.Id, seriesId);
            var update = Update<DailySeriesModel>.Set(d => d.Public, status);
            _mongoDb.GetCollection<DailySeriesModel>(DailySeriesModel.CollectionName).Update(query, update);
        }
    }
}