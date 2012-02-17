using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
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

            MigrationsHelper.Run(GetConnectionString);

            var db = new Database("SqlExpress")
            {
                KeepConnectionAlive = true,
                ForceDateTimesToUtc = false,
            };

            return db;
        }
    }
}