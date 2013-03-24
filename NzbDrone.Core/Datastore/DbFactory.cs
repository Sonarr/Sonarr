using System;
using System.Data;
using NzbDrone.Common;
using ServiceStack.OrmLite;

namespace NzbDrone.Core.Datastore
{
    public interface IDbFactory
    {
        IDbConnection Create(string dbPath = null);
    }

    public class DbFactory : IDbFactory
    {
        private readonly EnvironmentProvider _environmentProvider;

        public DbFactory(EnvironmentProvider environmentProvider)
        {
            _environmentProvider = environmentProvider;
        }

        public IDbConnection Create(string dbPath = null)
        {
            if (string.IsNullOrWhiteSpace(dbPath))
            {
                dbPath = _environmentProvider.GetObjectDbFolder();
            }

            var dbFactory = new OrmLiteConnectionFactory(GetConnectionString(dbPath));
            return dbFactory.Open();
        }

        private string GetConnectionString(string dbPath)
        {
            return String.Format("Data Source={0};Version=3;", dbPath);
        }
    }
}
