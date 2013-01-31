using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using MongoDB.Driver;

namespace NzbDrone.Services.Api.Datastore
{
    public class Connection
    {
        public MongoDatabase GetMainDb()
        {
            var db = GetMongoDb(ConfigurationManager.ConnectionStrings["MongoLab"].ConnectionString, "services-dev");
            return db;
        }

        public MongoDatabase GetMongoDb(string connectionString, string dbName)
        {
            var mongoServer = MongoServer.Create(connectionString);
            var database = mongoServer.GetDatabase(dbName.ToLowerInvariant());
            return database;
        }
    }
}