using System;
using System.Data.SQLite;
using Marr.Data;
using NLog;
using NzbDrone.Common.Instrumentation;

namespace NzbDrone.Core.Datastore
{
    public interface IDatabase
    {
        IDataMapper GetDataMapper();
        Version Version { get; }
        void Vacuum();
    }

    public class Database : IDatabase
    {
        private readonly string _connectionString;

        private Logger logger = NzbDroneLogger.GetLogger();

        public Database(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDataMapper GetDataMapper()
        {
            return new DataMapper(SQLiteFactory.Instance, _connectionString)
                    {
                        SqlMode = SqlModes.Text,
                    };
        }

        public Version Version
        {
            get
            {
                var version = GetDataMapper().ExecuteScalar("SELECT sqlite_version()").ToString();
                return new Version(version);
            }
        }

        public void Vacuum()
        {
            try
            {
                logger.Info("Vacuuming database " + _connectionString);
                GetDataMapper().ExecuteNonQuery("Vacuum;");
                logger.Info("Database Compressed " + _connectionString);
            }
            catch (Exception e)
            {
                logger.Error("An Error occurred while vacuuming database. " + _connectionString, e);
            }
        }
    }
}