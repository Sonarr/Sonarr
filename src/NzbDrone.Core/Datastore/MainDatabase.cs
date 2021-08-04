using System;
using System.Data;

namespace NzbDrone.Core.Datastore
{
    public interface IMainDatabase : IDatabase
    {
    }

    public class MainDatabase : IMainDatabase
    {
        private readonly IDatabase _database;

        public MainDatabase(IDatabase database)
        {
            _database = database;
        }

        public IDbConnection OpenConnection()
        {
            return _database.OpenConnection();
        }

        public Version Version => _database.Version;

        public int Migration => _database.Migration;

        public void Vacuum()
        {
            _database.Vacuum();
        }
    }
}
