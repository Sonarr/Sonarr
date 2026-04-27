using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Test.Framework
{
    public interface ITestDatabase
    {
        void InsertMany<T>(IEnumerable<T> items)
            where T : ModelBase, new();
        Task InsertManyAsync<T>(IEnumerable<T> items, CancellationToken cancellationToken = default)
            where T : ModelBase, new();
        T Insert<T>(T item)
            where T : ModelBase, new();
        Task<T> InsertAsync<T>(T item, CancellationToken cancellationToken = default)
            where T : ModelBase, new();
        List<T> All<T>()
            where T : ModelBase, new();
        Task<List<T>> AllAsync<T>(CancellationToken cancellationToken = default)
            where T : ModelBase, new();
        T Single<T>()
            where T : ModelBase, new();
        Task<T> SingleAsync<T>(CancellationToken cancellationToken = default)
            where T : ModelBase, new();
        void Update<T>(T childModel)
            where T : ModelBase, new();
        Task UpdateAsync<T>(T childModel, CancellationToken cancellationToken = default)
            where T : ModelBase, new();
        void Delete<T>(T childModel)
            where T : ModelBase, new();
        Task DeleteAsync<T>(T childModel, CancellationToken cancellationToken = default)
            where T : ModelBase, new();
        IDirectDataMapper GetDirectDataMapper();
        IDbConnection OpenConnection();
        Task<IDbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default);
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

        public void InsertMany<T>(IEnumerable<T> items)
            where T : ModelBase, new()
        {
            new BasicRepository<T>(_dbConnection, _eventAggregator).InsertMany(items.ToList());
        }

        public async Task InsertManyAsync<T>(IEnumerable<T> items, CancellationToken cancellationToken = default)
            where T : ModelBase, new()
        {
            await new BasicRepository<T>(_dbConnection, _eventAggregator).InsertManyAsync(items.ToList(), cancellationToken);
        }

        public T Insert<T>(T item)
            where T : ModelBase, new()
        {
            return new BasicRepository<T>(_dbConnection, _eventAggregator).Insert(item);
        }

        public async Task<T> InsertAsync<T>(T item, CancellationToken cancellationToken = default)
            where T : ModelBase, new()
        {
            return await new BasicRepository<T>(_dbConnection, _eventAggregator).InsertAsync(item, cancellationToken);
        }

        public List<T> All<T>()
            where T : ModelBase, new()
        {
            return new BasicRepository<T>(_dbConnection, _eventAggregator).All().ToList();
        }

        public async Task<List<T>> AllAsync<T>(CancellationToken cancellationToken = default)
            where T : ModelBase, new()
        {
            return await new BasicRepository<T>(_dbConnection, _eventAggregator).AllAsync(cancellationToken).ToListAsync(cancellationToken);
        }

        public T Single<T>()
            where T : ModelBase, new()
        {
            return All<T>().SingleOrDefault();
        }

        public async Task<T> SingleAsync<T>(CancellationToken cancellationToken = default)
            where T : ModelBase, new()
        {
            return (await AllAsync<T>(cancellationToken)).SingleOrDefault();
        }

        public void Update<T>(T childModel)
            where T : ModelBase, new()
        {
            new BasicRepository<T>(_dbConnection, _eventAggregator).Update(childModel);
        }

        public async Task UpdateAsync<T>(T childModel, CancellationToken cancellationToken = default)
            where T : ModelBase, new()
        {
            await new BasicRepository<T>(_dbConnection, _eventAggregator).UpdateAsync(childModel, cancellationToken);
        }

        public void Delete<T>(T childModel)
            where T : ModelBase, new()
        {
            new BasicRepository<T>(_dbConnection, _eventAggregator).Delete(childModel);
        }

        public async Task DeleteAsync<T>(T childModel, CancellationToken cancellationToken = default)
            where T : ModelBase, new()
        {
            await new BasicRepository<T>(_dbConnection, _eventAggregator).DeleteAsync(childModel, cancellationToken);
        }

        public IDirectDataMapper GetDirectDataMapper()
        {
            return new DirectDataMapper(_dbConnection);
        }

        public IDbConnection OpenConnection()
        {
            return _dbConnection.OpenConnection();
        }

        public async Task<IDbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
        {
            return await _dbConnection.OpenConnectionAsync(cancellationToken);
        }
    }
}
