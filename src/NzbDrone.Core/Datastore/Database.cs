using System;
using Marr.Data;
using NLog;
using NzbDrone.Common.Instrumentation;

namespace NzbDrone.Core.Datastore
{
    public interface IDatabase
    {
        IDataMapper GetDataMapper();
        Version Version { get; }
        long Size { get; }
        void Vacuum();
    }

    public class Database : IDatabase
    {
        private readonly string _databaseName;
        private readonly Func<IDataMapper> _datamapperFactory;

        private readonly Logger _logger = NzbDroneLogger.GetLogger(typeof(Database));

        public Database(string databaseName, Func<IDataMapper> datamapperFactory)
        {
            _databaseName = databaseName;
            _datamapperFactory = datamapperFactory;
        }

        public IDataMapper GetDataMapper()
        {
            return _datamapperFactory();
        }

        public Version Version
        {
            get
            {
                var version = _datamapperFactory().ExecuteScalar("SELECT sqlite_version()").ToString();
                return new Version(version);
            }
        }

        public long Size
        {
            get
            {
                var page_count = _datamapperFactory().ExecuteScalar("PRAGMA page_count;");
                var page_size = _datamapperFactory().ExecuteScalar("PRAGMA page_size;");
                return Convert.ToInt64(page_count) * Convert.ToInt64(page_size);
            }
        }

        public void Vacuum()
        {
            try
            {
                _logger.Info("Vacuuming {0} database", _databaseName);
                _datamapperFactory().ExecuteNonQuery("Vacuum;");
                _logger.Info("{0} database compressed", _databaseName);
            }
            catch (Exception e)
            {
                _logger.Error(e, "An Error occurred while vacuuming database.");
            }
        }
    }
}
