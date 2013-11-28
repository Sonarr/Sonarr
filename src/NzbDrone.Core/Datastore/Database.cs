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
        void Vacuum();
    }

    public class Database : IDatabase
    {
        private readonly Func<IDataMapper> _datamapperFactory;

        private Logger logger = NzbDroneLogger.GetLogger();

        public Database(Func<IDataMapper> datamapperFactory)
        {
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

        public void Vacuum()
        {
            try
            {
                logger.Info("Vacuuming database");
                _datamapperFactory().ExecuteNonQuery("Vacuum;");
                logger.Info("Database Compressed");
            }
            catch (Exception e)
            {
                logger.Error("An Error occurred while vacuuming database.", e);
            }
        }
    }
}