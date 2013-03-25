using System;
using Marr.Data;

namespace NzbDrone.Core.Datastore
{
    public interface IDatabase
    {
        IDataMapper DataMapper { get; }
    }

    public class Database : IDatabase
    {

        public Database(IDataMapper dataMapper)
        {
            DataMapper = dataMapper;
        }

        public IDataMapper DataMapper { get; private set; }
    }
}