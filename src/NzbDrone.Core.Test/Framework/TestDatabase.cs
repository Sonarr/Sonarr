using System.Collections.Generic;
using System.Linq;
using Moq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Test.Framework
{
    public interface ITestDatabase
    {
        void InsertMany<T>(IEnumerable<T> items)
            where T : ModelBase, new();
        T Insert<T>(T item)
            where T : ModelBase, new();
        List<T> All<T>()
            where T : ModelBase, new();
        T Single<T>()
            where T : ModelBase, new();
        void Update<T>(T childModel)
            where T : ModelBase, new();
        void Delete<T>(T childModel)
            where T : ModelBase, new();
        IDirectDataMapper GetDirectDataMapper();
    }

    public class TestDatabase : ITestDatabase
    {
        private readonly IDatabase _dbConnection;
        private readonly IEventAggregator _eventAggregator;

        public TestDatabase(IDatabase dbConnection)
        {
            _eventAggregator = new Mock<IEventAggregator>().Object;
            _dbConnection = dbConnection;
        }

        public void InsertMany<T>(IEnumerable<T> items)
            where T : ModelBase, new()
        {
            new BasicRepository<T>(_dbConnection, _eventAggregator).InsertMany(items.ToList());
        }

        public T Insert<T>(T item)
            where T : ModelBase, new()
        {
            return new BasicRepository<T>(_dbConnection, _eventAggregator).Insert(item);
        }

        public List<T> All<T>()
            where T : ModelBase, new()
        {
            return new BasicRepository<T>(_dbConnection, _eventAggregator).All().ToList();
        }

        public T Single<T>()
            where T : ModelBase, new()
        {
            return All<T>().SingleOrDefault();
        }

        public void Update<T>(T childModel)
            where T : ModelBase, new()
        {
            new BasicRepository<T>(_dbConnection, _eventAggregator).Update(childModel);
        }

        public void Delete<T>(T childModel)
            where T : ModelBase, new()
        {
            new BasicRepository<T>(_dbConnection, _eventAggregator).Delete(childModel);
        }

        public IDirectDataMapper GetDirectDataMapper()
        {
            return new DirectDataMapper(_dbConnection);
        }
    }
}
