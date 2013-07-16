using System;
using Marr.Data;

namespace NzbDrone.Core.Datastore
{
    public interface IDatabase
    {
        IDataMapper GetDataMapper();
    }

    public class Database : IDatabase
    {
        private readonly Func<IDataMapper> _dataMapperFactory;

        public Database(Func<IDataMapper> dataMapperFactory)
        {
            _dataMapperFactory = dataMapperFactory;
        }

        public IDataMapper GetDataMapper()
        {
            return _dataMapperFactory();
        }
    }
}