using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Test.Framework
{
    public interface ITestDatabase
    {
        Task InsertManyAsync<T>(IEnumerable<T> items)
            where T : ModelBase, new();
        Task<T> InsertAsync<T>(T item)
            where T : ModelBase, new();
        Task<List<T>> AllAsync<T>()
            where T : ModelBase, new();
        Task<T> SingleAsync<T>()
            where T : ModelBase, new();
        Task UpdateAsync<T>(T childModel)
            where T : ModelBase, new();
        Task DeleteAsync<T>(T childModel)
            where T : ModelBase, new();
        IDirectDataMapper GetDirectDataMapper();
        IDbConnection OpenConnection();
        DatabaseType DatabaseType { get; }
    }

    public class TestDatabase : ITestDatabase
    {
        private readonly IDatabase _dbConnection;
        private readonly IEventAggregator _eventAggregator;

        public DatabaseType DatabaseType => _dbConnection.DatabaseType;

        public TestDatabase(IDatabase dbConnection)
        {
            _eventAggregator = new Mock<IEventAggregator>().Object;
            _dbConnection = dbConnection;
        }

        public async Task InsertManyAsync<T>(IEnumerable<T> items)
            where T : ModelBase, new()
        {
            await new BasicRepository<T>(_dbConnection, _eventAggregator).InsertManyAsync(items.ToList());
        }

        public async Task<T> InsertAsync<T>(T item)
            where T : ModelBase, new()
        {
            return await new BasicRepository<T>(_dbConnection, _eventAggregator).InsertAsync(item);
        }

        public async Task<List<T>> AllAsync<T>()
            where T : ModelBase, new()
        {
            var enumerable = await new BasicRepository<T>(_dbConnection, _eventAggregator).AllAsync();
            return enumerable.ToList();
        }

        public async Task<T> SingleAsync<T>()
            where T : ModelBase, new()
        {
            var items = await AllAsync<T>();
            return items.SingleOrDefault();
        }

        public async Task UpdateAsync<T>(T childModel)
            where T : ModelBase, new()
        {
            await new BasicRepository<T>(_dbConnection, _eventAggregator).UpdateAsync(childModel);
        }

        public async Task DeleteAsync<T>(T childModel)
            where T : ModelBase, new()
        {
            await new BasicRepository<T>(_dbConnection, _eventAggregator).DeleteAsync(childModel);
        }

        public IDirectDataMapper GetDirectDataMapper()
        {
            return new DirectDataMapper(_dbConnection);
        }

        public IDbConnection OpenConnection()
        {
            return _dbConnection.OpenConnection();
        }
    }
}
