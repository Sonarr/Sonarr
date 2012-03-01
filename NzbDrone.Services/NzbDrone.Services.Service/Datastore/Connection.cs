using System.Configuration;
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
                KeepConnectionAlive = false,
                ForceDateTimesToUtc = false,
            };

            return db;
        }
    }
}