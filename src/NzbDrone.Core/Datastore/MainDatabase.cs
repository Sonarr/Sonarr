using System;
using Marr.Data;

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

        public IDataMapper GetDataMapper()
        {
            return _database.GetDataMapper();
        }

        public Version Version => _database.Version;

        public void Vacuum()
        {
            _database.Vacuum();
        }
    }
}
