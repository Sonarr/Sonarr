using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using NzbDrone.Services.Api.DailySeries;

namespace NzbDrone.Services.Api.SceneMapping
{
    public class SceneMappingRepository
    {
        private readonly MongoDatabase _mongoDb;

        public SceneMappingRepository(MongoDatabase mongoDb)
        {
            _mongoDb = mongoDb;
        }

        public SceneMappingRepository()
        {   
        }

        public List<SceneMappingModel> All()
        {
            return _mongoDb.GetCollection<SceneMappingModel>(SceneMappingModel.CollectionName).FindAll().ToList();
        }

        public List<SceneMappingModel> Public()
        {
            var query = Query<SceneMappingModel>.EQ(d => d.Public, true);
            return _mongoDb.GetCollection<SceneMappingModel>(SceneMappingModel.CollectionName).Find(query).ToList();
        }

        public SceneMappingModel Find(ObjectId mapId)
        {
            var query = Query<SceneMappingModel>.EQ(d => d.MapId, mapId);
            return _mongoDb.GetCollection<SceneMappingModel>(SceneMappingModel.CollectionName).FindOne(query);
        }

        public ObjectId Insert(SceneMappingModel mapping)
        {
            mapping.MapId = ObjectId.GenerateNewId();
            _mongoDb.GetCollection<SceneMappingModel>(SceneMappingModel.CollectionName).Insert(mapping);

            return mapping.MapId;
        }

        public void Delete(ObjectId mapId)
        {
            var query = Query<SceneMappingModel>.EQ(d => d.MapId, mapId);
            _mongoDb.GetCollection<SceneMappingModel>(SceneMappingModel.CollectionName).Remove(query);
        }

        public void TogglePublic(ObjectId mapId, bool status)
        {
            var query = Query<SceneMappingModel>.EQ(d => d.MapId, mapId);
            var update = Update<SceneMappingModel>.Set(d => d.Public, status);
            _mongoDb.GetCollection<SceneMappingModel>(SceneMappingModel.CollectionName).Update(query, update);
        }
    }
}