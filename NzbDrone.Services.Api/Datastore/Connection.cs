using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Driver;

namespace NzbDrone.Services.Api.Datastore
{
    public class Connection
    {
        public MongoDatabase GetMainDb()
        {
            var db = GetMongoDb("mongodb://nzbdrone:h53huDrAzufRe8a3@ds035147.mongolab.com:35147/?safe=true;wtimeoutMS=2000", "services-dev");
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