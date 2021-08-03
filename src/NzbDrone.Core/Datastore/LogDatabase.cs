using System;
using Marr.Data;

namespace NzbDrone.Core.Datastore
{
    public interface ILogDatabase : IDatabase
    {
    }

    public class LogDatabase : ILogDatabase
    {
        private readonly IDatabase _database;

        public LogDatabase(IDatabase database)
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
