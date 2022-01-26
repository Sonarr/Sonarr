namespace NzbDrone.Core.Datastore
{
    public class DatabaseConnectionInfo
    {
        public DatabaseConnectionInfo(DatabaseType databaseType, string connectionString)
        {
            DatabaseType = databaseType;
            ConnectionString = connectionString;
        }

        public DatabaseType DatabaseType { get; internal set; }
        public string ConnectionString { get; internal set; }
    }
}
