using System;
using System.Configuration;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using NzbDrone.Services.Service.Migrations;
using Services.PetaPoco;


namespace NzbDrone.Services.Service.Datastore
{
    public static class Connection
    {
        public static string GetConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings["SqlExpress"].ConnectionString; }
        }

        public static IDatabase GetPetaPocoDb()
        {
            var db = new Database("SqlExpress")
            {
                KeepConnectionAlive = false,
                ForceDateTimesToUtc = false,
            };

            return db;
        }


        public static MongoDatabase GetMongoDb()
        {
            var serverSettings = new MongoServerSettings()
            {
                ConnectionMode = ConnectionMode.Direct,
                ConnectTimeout = TimeSpan.FromSeconds(10),
                DefaultCredentials = new MongoCredentials("nzbdrone", "nzbdronepassword"),
                GuidRepresentation = GuidRepresentation.Standard,
                Server = new MongoServerAddress("ds031747.mongolab.com", 31747),
                SafeMode = new SafeMode(true) { J = true },
            };


            var server = MongoServer.Create(serverSettings);

            return server.GetDatabase("nzbdrone_ex");
        }
    }
}