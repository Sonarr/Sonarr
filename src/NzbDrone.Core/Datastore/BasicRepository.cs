using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using NLog;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Datastore.Extensions;
using NzbDrone.Core.Messaging.Events;
using Polly;
using Polly.Retry;

namespace NzbDrone.Core.Datastore
{
    public interface IBasicRepository<TModel>
        where TModel : ModelBase, new()
    {
        IEnumerable<TModel> All();
        IAsyncEnumerable<TModel> AllAsync(CancellationToken cancellationToken = default);
        int Count();
        Task<int> CountAsync(CancellationToken cancellationToken = default);
        TModel Find(int id);
        Task<TModel> FindAsync(int id, CancellationToken cancellationToken = default);
        TModel Get(int id);
        Task<TModel> GetAsync(int id, CancellationToken cancellationToken = default);
        TModel Insert(TModel model);
        Task<TModel> InsertAsync(TModel model, CancellationToken cancellationToken = default);
        TModel Update(TModel model);
        Task<TModel> UpdateAsync(TModel model, CancellationToken cancellationToken = default);
        TModel Upsert(TModel model);
        Task<TModel> UpsertAsync(TModel model, CancellationToken cancellationToken = default);
        void SetFields(TModel model, params Expression<Func<TModel, object>>[] properties);
        Task SetFieldsAsync(TModel model, params Expression<Func<TModel, object>>[] properties);
        void Delete(TModel model);
        Task DeleteAsync(TModel model, CancellationToken cancellationToken = default);
        void Delete(int id);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
        IEnumerable<TModel> Get(IEnumerable<int> ids);
        IAsyncEnumerable<TModel> GetAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
        void InsertMany(IList<TModel> model);
        Task InsertManyAsync(IList<TModel> models, CancellationToken cancellationToken = default);
        void UpdateMany(IList<TModel> model);
        Task UpdateManyAsync(IList<TModel> models, CancellationToken cancellationToken = default);
        void SetFields(IList<TModel> models, params Expression<Func<TModel, object>>[] properties);
        Task SetFieldsAsync(IList<TModel> models, params Expression<Func<TModel, object>>[] properties);
        void DeleteMany(List<TModel> model);
        Task DeleteManyAsync(List<TModel> models, CancellationToken cancellationToken = default);
        void DeleteMany(IEnumerable<int> ids);
        Task DeleteManyAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
        void Purge(bool vacuum = false);
        Task PurgeAsync(bool vacuum = false, CancellationToken cancellationToken = default);
        bool HasItems();
        Task<bool> HasItemsAsync(CancellationToken cancellationToken = default);
        TModel Single();
        Task<TModel> SingleAsync(CancellationToken cancellationToken = default);
        TModel SingleOrDefault();
        Task<TModel> SingleOrDefaultAsync(CancellationToken cancellationToken = default);
        PagingSpec<TModel> GetPaged(PagingSpec<TModel> pagingSpec);
    }

    public class BasicRepository<TModel> : IBasicRepository<TModel>
        where TModel : ModelBase, new()
    {
        private static readonly ILogger Logger = NzbDroneLogger.GetLogger(typeof(BasicRepository<TModel>));

        private readonly IEventAggregator _eventAggregator;
        private readonly PropertyInfo _keyProperty;
        private readonly List<PropertyInfo> _properties;
        private readonly string _updateSql;
        private readonly string _insertSql;

        private static ResiliencePipeline RetryStrategy => new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<SQLiteException>(ex => ex.ResultCode == SQLiteErrorCode.Busy),
                Delay = TimeSpan.FromMilliseconds(100),
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                OnRetry = args =>
                {
                    Logger.Warn(args.Outcome.Exception, "Failed writing to database. Retry #{0}", args.AttemptNumber);

                    return default;
                }
            })
            .Build();

        protected readonly IDatabase _database;
        protected readonly string _table;

        public BasicRepository(IDatabase database, IEventAggregator eventAggregator)
        {
            _database = database;
            _eventAggregator = eventAggregator;

            var type = typeof(TModel);

            _table = TableMapping.Mapper.TableNameMapping(type);
            _keyProperty = type.GetProperty(nameof(ModelBase.Id));

            var excluded = TableMapping.Mapper.ExcludeProperties(type).Select(x => x.Name).ToList();
            excluded.Add(_keyProperty.Name);
            _properties = type.GetProperties().Where(x => x.IsMappableProperty() && !excluded.Contains(x.Name)).ToList();

            _insertSql = GetInsertSql();
            _updateSql = GetUpdateSql(_properties);
        }

        protected virtual SqlBuilder Builder() => new SqlBuilder(_database.DatabaseType);

        protected virtual List<TModel> Query(SqlBuilder builder) => _database.Query<TModel>(builder).ToList();

        protected virtual IAsyncEnumerable<TModel> QueryAsync(SqlBuilder builder, CancellationToken cancellationToken = default) => _database.QueryAsync<TModel>(builder, cancellationToken);

        protected virtual List<TModel> QueryDistinct(SqlBuilder builder) => _database.QueryDistinct<TModel>(builder).ToList();

        protected virtual IAsyncEnumerable<TModel> QueryDistinctAsync(SqlBuilder builder, CancellationToken cancellationToken = default) => _database.QueryDistinctAsync<TModel>(builder, cancellationToken);

        protected List<TModel> Query(Expression<Func<TModel, bool>> where) => Query(Builder().Where(where));

        protected IAsyncEnumerable<TModel> QueryAsync(Expression<Func<TModel, bool>> where, CancellationToken cancellationToken = default) => QueryAsync(Builder().Where(where), cancellationToken);

        public int Count()
        {
            using (var conn = _database.OpenConnection())
            {
                return conn.ExecuteScalar<int>($"SELECT COUNT(*) FROM \"{_table}\"");
            }
        }

        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await using var conn = await _database.OpenConnectionAsync(cancellationToken);

            return await conn.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM \"{_table}\"");
        }

        public virtual IEnumerable<TModel> All()
        {
            return Query(Builder());
        }

        public virtual IAsyncEnumerable<TModel> AllAsync(CancellationToken cancellationToken = default)
        {
            return QueryAsync(Builder(), cancellationToken);
        }

        public TModel Find(int id)
        {
            var model = Query(x => x.Id == id).FirstOrDefault();

            return model;
        }

        public async Task<TModel> FindAsync(int id, CancellationToken cancellationToken = default)
        {
            return await QueryAsync(x => x.Id == id, cancellationToken).FirstOrDefaultAsync(cancellationToken);
        }

        public TModel Get(int id)
        {
            var model = Find(id);

            if (model == null)
            {
                throw new ModelNotFoundException(typeof(TModel), id);
            }

            return model;
        }

        public async Task<TModel> GetAsync(int id, CancellationToken cancellationToken = default)
        {
            var model = await FindAsync(id, cancellationToken);

            if (model == null)
            {
                throw new ModelNotFoundException(typeof(TModel), id);
            }

            return model;
        }

        public IEnumerable<TModel> Get(IEnumerable<int> ids)
        {
            if (!ids.Any())
            {
                return Array.Empty<TModel>();
            }

            var result = Query(x => ids.Contains(x.Id));

            if (result.Count != ids.Count())
            {
                throw new ApplicationException($"Expected query to return {ids.Count()} rows but returned {result.Count}");
            }

            return result;
        }

        public async IAsyncEnumerable<TModel> GetAsync(IEnumerable<int> ids, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (!ids.Any())
            {
                yield break;
            }

            var result = QueryAsync(x => ids.Contains(x.Id), cancellationToken);
            var countResult = await result.CountAsync(cancellationToken);

            if (countResult != ids.Count())
            {
                throw new ApplicationException($"Expected query to return {ids.Count()} rows but returned {countResult}");
            }

            await foreach (var model in result)
            {
                yield return model;
            }
        }

        public TModel SingleOrDefault()
        {
            return All().SingleOrDefault();
        }

        public async Task<TModel> SingleOrDefaultAsync(CancellationToken cancellationToken = default)
        {
            return await AllAsync(cancellationToken).SingleOrDefaultAsync(cancellationToken);
        }

        public TModel Single()
        {
            return All().Single();
        }

        public async Task<TModel> SingleAsync(CancellationToken cancellationToken = default)
        {
            return await AllAsync(cancellationToken).SingleAsync(cancellationToken);
        }

        public TModel Insert(TModel model)
        {
            if (model.Id != 0)
            {
                throw new InvalidOperationException("Can't insert model with existing ID " + model.Id);
            }

            using (var conn = _database.OpenConnection())
            {
                model = Insert(conn, null, model);
            }

            ModelCreated(model);

            return model;
        }

        public async Task<TModel> InsertAsync(TModel model, CancellationToken cancellationToken = default)
        {
            if (model.Id != 0)
            {
                throw new InvalidOperationException("Can't insert model with existing ID " + model.Id);
            }

            await using (var conn = await _database.OpenConnectionAsync(cancellationToken))
            {
                model = await InsertAsync(conn, null, model, cancellationToken);
            }

            ModelCreated(model);

            return model;
        }

        private string GetInsertSql()
        {
            var sbColumnList = new StringBuilder(null);
            for (var i = 0; i < _properties.Count; i++)
            {
                var property = _properties[i];
                sbColumnList.AppendFormat("\"{0}\"", property.Name);
                if (i < _properties.Count - 1)
                {
                    sbColumnList.Append(", ");
                }
            }

            var sbParameterList = new StringBuilder(null);
            for (var i = 0; i < _properties.Count; i++)
            {
                var property = _properties[i];
                sbParameterList.AppendFormat("@{0}", property.Name);
                if (i < _properties.Count - 1)
                {
                    sbParameterList.Append(", ");
                }
            }

            if (_database.DatabaseType == DatabaseType.PostgreSQL)
            {
                return $"INSERT INTO \"{_table}\" ({sbColumnList.ToString()}) VALUES ({sbParameterList.ToString()}) RETURNING \"Id\"";
            }

            return $"INSERT INTO {_table} ({sbColumnList.ToString()}) VALUES ({sbParameterList.ToString()}); SELECT last_insert_rowid() id";
        }

        private TModel Insert(IDbConnection connection, IDbTransaction transaction, TModel model)
        {
            SqlBuilderExtensions.LogQuery(_insertSql, model);

            var multi = RetryStrategy.Execute(static (state, _) => state.connection.QueryMultiple(state._insertSql, state.model, state.transaction), (connection, _insertSql, model, transaction));

            var multiRead = multi.Read();
            var id = (int)(multiRead.First().id ?? multiRead.First().Id);
            _keyProperty.SetValue(model, id);

            return model;
        }

        private async Task<TModel> InsertAsync(IDbConnection connection, IDbTransaction transaction, TModel model, CancellationToken cancellationToken = default)
        {
            SqlBuilderExtensions.LogQuery(_insertSql, model);

            var multi = await RetryStrategy.ExecuteAsync(async static (state, _) => await state.connection.QueryMultipleAsync(state._insertSql, state.model, state.transaction), (connection, _insertSql, model, transaction), cancellationToken);

            var multiRead = await multi.ReadAsync();
            var id = (int)(multiRead.First().id ?? multiRead.First().Id);
            _keyProperty.SetValue(model, id);

            return model;
        }

        public void InsertMany(IList<TModel> models)
        {
            if (models.Any(x => x.Id != 0))
            {
                throw new InvalidOperationException("Can't insert model with existing ID != 0");
            }

            using (var conn = _database.OpenConnection())
            {
                using (var tran = conn.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    foreach (var model in models)
                    {
                        Insert(conn, tran, model);
                    }

                    tran.Commit();
                }
            }
        }

        public async Task InsertManyAsync(IList<TModel> models, CancellationToken cancellationToken = default)
        {
            if (models.Any(x => x.Id != 0))
            {
                throw new InvalidOperationException("Can't insert model with existing ID != 0");
            }

            await using var conn = await _database.OpenConnectionAsync(cancellationToken);
            await using var tran = await conn.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

            foreach (var model in models)
            {
                await InsertAsync(conn, tran, model, cancellationToken);
            }

            await tran.CommitAsync(cancellationToken);
        }

        public TModel Update(TModel model)
        {
            if (model.Id == 0)
            {
                throw new InvalidOperationException("Can't update model with ID 0");
            }

            using (var conn = _database.OpenConnection())
            {
                UpdateFields(conn, null, model, _properties);
            }

            ModelUpdated(model);

            return model;
        }

        public async Task<TModel> UpdateAsync(TModel model, CancellationToken cancellationToken = default)
        {
            if (model.Id == 0)
            {
                throw new InvalidOperationException("Can't update model with ID 0");
            }

            await using (var conn = await _database.OpenConnectionAsync(cancellationToken))
            {
                await UpdateFieldsAsync(conn, null, model, _properties, cancellationToken);
            }

            ModelUpdated(model);

            return model;
        }

        public void UpdateMany(IList<TModel> models)
        {
            if (models.Any(x => x.Id == 0))
            {
                throw new InvalidOperationException("Can't update model with ID 0");
            }

            using (var conn = _database.OpenConnection())
            using (var tran = conn.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                UpdateFields(conn, tran, models, _properties);
                tran.Commit();
            }
        }

        public async Task UpdateManyAsync(IList<TModel> models, CancellationToken cancellationToken = default)
        {
            if (models.Any(x => x.Id == 0))
            {
                throw new InvalidOperationException("Can't update model with ID 0");
            }

            await using var conn = await _database.OpenConnectionAsync(cancellationToken);
            await using var tran = await conn.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

            await UpdateFieldsAsync(conn, tran, models, _properties, cancellationToken);

            await tran.CommitAsync(cancellationToken);
        }

        protected void Delete(Expression<Func<TModel, bool>> where)
        {
            Delete(Builder().Where<TModel>(where));
        }

        protected async Task DeleteAsync(Expression<Func<TModel, bool>> where, CancellationToken cancellationToken = default)
        {
            await DeleteAsync(Builder().Where<TModel>(where), cancellationToken);
        }

        protected void Delete(SqlBuilder builder)
        {
            var sql = builder.AddDeleteTemplate(typeof(TModel));

            using (var conn = _database.OpenConnection())
            {
                conn.Execute(sql.RawSql, sql.Parameters);
            }
        }

        protected async Task DeleteAsync(SqlBuilder builder, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var sql = builder.AddDeleteTemplate(typeof(TModel));

            await using var conn = await _database.OpenConnectionAsync(cancellationToken);

            await conn.ExecuteAsync(sql.RawSql, sql.Parameters);
        }

        public void Delete(TModel model)
        {
            Delete(model.Id);
        }

        public async Task DeleteAsync(TModel model, CancellationToken cancellationToken = default)
        {
            await DeleteAsync(model.Id, cancellationToken);
        }

        public void Delete(int id)
        {
            Delete(x => x.Id == id);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            await DeleteAsync(x => x.Id == id, cancellationToken);
        }

        public void DeleteMany(IEnumerable<int> ids)
        {
            if (ids.Any())
            {
                Delete(x => ids.Contains(x.Id));
            }
        }

        public async Task DeleteManyAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
        {
            if (ids.Any())
            {
                await DeleteAsync(x => ids.Contains(x.Id), cancellationToken);
            }
        }

        public void DeleteMany(List<TModel> models)
        {
            DeleteMany(models.Select(m => m.Id));
        }

        public async Task DeleteManyAsync(List<TModel> models, CancellationToken cancellationToken = default)
        {
            await DeleteManyAsync(models.Select(m => m.Id), cancellationToken);
        }

        public TModel Upsert(TModel model)
        {
            if (model.Id == 0)
            {
                Insert(model);
                return model;
            }

            Update(model);
            return model;
        }

        public async Task<TModel> UpsertAsync(TModel model, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (model.Id == 0)
            {
                await InsertAsync(model, cancellationToken);
                return model;
            }

            await UpdateAsync(model, cancellationToken);
            return model;
        }

        public void Purge(bool vacuum = false)
        {
            using (var conn = _database.OpenConnection())
            {
                conn.Execute($"DELETE FROM \"{_table}\"");
            }

            if (vacuum)
            {
                Vacuum();
            }
        }

        public async Task PurgeAsync(bool vacuum = false, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await using (var conn = await _database.OpenConnectionAsync(cancellationToken))
            {
                await conn.ExecuteAsync($"DELETE FROM \"{_table}\"");
            }

            if (vacuum)
            {
                Vacuum();
            }
        }

        protected void Vacuum()
        {
            _database.Vacuum();
        }

        public bool HasItems()
        {
            return Count() > 0;
        }

        public async Task<bool> HasItemsAsync(CancellationToken cancellationToken = default)
        {
            return await CountAsync(cancellationToken) > 0;
        }

        public void SetFields(TModel model, params Expression<Func<TModel, object>>[] properties)
        {
            if (model.Id == 0)
            {
                throw new InvalidOperationException("Attempted to update model without ID");
            }

            var propertiesToUpdate = properties.Select(x => x.GetMemberName()).ToList();

            using (var conn = _database.OpenConnection())
            {
                UpdateFields(conn, null, model, propertiesToUpdate);
            }

            ModelUpdated(model);
        }

        public async Task SetFieldsAsync(TModel model, params Expression<Func<TModel, object>>[] properties)
        {
            if (model.Id == 0)
            {
                throw new InvalidOperationException("Attempted to update model without ID");
            }

            var propertiesToUpdate = properties.Select(x => x.GetMemberName()).ToList();

            await using (var conn = await _database.OpenConnectionAsync())
            {
                await UpdateFieldsAsync(conn, null, model, propertiesToUpdate);
            }

            ModelUpdated(model);
        }

        public void SetFields(IList<TModel> models, params Expression<Func<TModel, object>>[] properties)
        {
            if (models.Any(x => x.Id == 0))
            {
                throw new InvalidOperationException("Attempted to update model without ID");
            }

            var propertiesToUpdate = properties.Select(x => x.GetMemberName()).ToList();

            using (var conn = _database.OpenConnection())
            using (var tran = conn.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                UpdateFields(conn, tran, models, propertiesToUpdate);
                tran.Commit();
            }

            foreach (var model in models)
            {
                ModelUpdated(model);
            }
        }

        public async Task SetFieldsAsync(IList<TModel> models, params Expression<Func<TModel, object>>[] properties)
        {
            if (models.Any(x => x.Id == 0))
            {
                throw new InvalidOperationException("Attempted to update model without ID");
            }

            var propertiesToUpdate = properties.Select(x => x.GetMemberName()).ToList();

            await using (var conn = await _database.OpenConnectionAsync())
            await using (var tran = await conn.BeginTransactionAsync(IsolationLevel.ReadCommitted))
            {
                await UpdateFieldsAsync(conn, tran, models, propertiesToUpdate);
                await tran.CommitAsync();
            }

            foreach (var model in models)
            {
                ModelUpdated(model);
            }
        }

        private string GetUpdateSql(List<PropertyInfo> propertiesToUpdate)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("UPDATE \"{0}\" SET ", _table);

            for (var i = 0; i < propertiesToUpdate.Count; i++)
            {
                var property = propertiesToUpdate[i];
                sb.AppendFormat("\"{0}\" = @{1}", property.Name, property.Name);
                if (i < propertiesToUpdate.Count - 1)
                {
                    sb.Append(", ");
                }
            }

            sb.Append($" WHERE \"{_keyProperty.Name}\" = @{_keyProperty.Name}");

            return sb.ToString();
        }

        private void UpdateFields(IDbConnection connection, IDbTransaction transaction, TModel model, List<PropertyInfo> propertiesToUpdate)
        {
            var sql = propertiesToUpdate == _properties ? _updateSql : GetUpdateSql(propertiesToUpdate);

            SqlBuilderExtensions.LogQuery(sql, model);

            RetryStrategy.Execute(static (state, _) => state.connection.Execute(state.sql, state.model, transaction: state.transaction), (connection, sql, model, transaction));
        }

        private async Task UpdateFieldsAsync(IDbConnection connection, IDbTransaction transaction, TModel model, List<PropertyInfo> propertiesToUpdate, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var sql = propertiesToUpdate == _properties ? _updateSql : GetUpdateSql(propertiesToUpdate);

            SqlBuilderExtensions.LogQuery(sql, model);

            await RetryStrategy.ExecuteAsync(async static (state, _) => await state.connection.ExecuteAsync(state.sql, state.model, transaction: state.transaction), (connection, sql, model, transaction), cancellationToken);
        }

        private void UpdateFields(IDbConnection connection, IDbTransaction transaction, IList<TModel> models, List<PropertyInfo> propertiesToUpdate)
        {
            var sql = propertiesToUpdate == _properties ? _updateSql : GetUpdateSql(propertiesToUpdate);

            foreach (var model in models)
            {
                SqlBuilderExtensions.LogQuery(sql, model);
            }

            RetryStrategy.Execute(static (state, _) => state.connection.Execute(state.sql, state.models, transaction: state.transaction), (connection, sql, models, transaction));
        }

        private async Task UpdateFieldsAsync(IDbConnection connection, IDbTransaction transaction, IList<TModel> models, List<PropertyInfo> propertiesToUpdate, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var sql = propertiesToUpdate == _properties ? _updateSql : GetUpdateSql(propertiesToUpdate);

            foreach (var model in models)
            {
                SqlBuilderExtensions.LogQuery(sql, model);
            }

            await RetryStrategy.ExecuteAsync(async static (state, _) => await state.connection.ExecuteAsync(state.sql, state.models, transaction: state.transaction), (connection, sql, models, transaction), cancellationToken);
        }

        protected virtual SqlBuilder PagedBuilder() => Builder();
        protected virtual IEnumerable<TModel> PagedQuery(SqlBuilder sql) => Query(sql);

        public virtual PagingSpec<TModel> GetPaged(PagingSpec<TModel> pagingSpec)
        {
            pagingSpec.Records = GetPagedRecords(PagedBuilder(), pagingSpec, PagedQuery);
            pagingSpec.TotalRecords = GetPagedRecordCount(PagedBuilder().SelectCount(), pagingSpec);

            return pagingSpec;
        }

        protected void AddFilters(SqlBuilder builder, PagingSpec<TModel> pagingSpec)
        {
            var filters = pagingSpec.FilterExpressions;

            foreach (var filter in filters)
            {
                builder.Where<TModel>(filter);
            }
        }

        protected List<TModel> GetPagedRecords(SqlBuilder builder, PagingSpec<TModel> pagingSpec, Func<SqlBuilder, IEnumerable<TModel>> queryFunc)
        {
            AddFilters(builder, pagingSpec);

            if (pagingSpec.SortKey == null)
            {
                pagingSpec.SortKey = $"{_table}.{_keyProperty.Name}";
            }

            var sortKey = TableMapping.Mapper.GetSortKey(pagingSpec.SortKey);
            var sortDirection = pagingSpec.SortDirection == SortDirection.Descending ? "DESC" : "ASC";
            var pagingOffset = Math.Max(pagingSpec.Page - 1, 0) * pagingSpec.PageSize;
            builder.OrderBy($"\"{sortKey.Table ?? _table}\".\"{sortKey.Column}\" {sortDirection} LIMIT {pagingSpec.PageSize} OFFSET {pagingOffset}");

            return queryFunc(builder).ToList();
        }

        protected int GetPagedRecordCount(SqlBuilder builder, PagingSpec<TModel> pagingSpec, string template = null)
        {
            AddFilters(builder, pagingSpec);

            SqlBuilder.Template sql;
            if (template != null)
            {
                sql = builder.AddTemplate(template).LogQuery();
            }
            else
            {
                sql = builder.AddPageCountTemplate(typeof(TModel));
            }

            using (var conn = _database.OpenConnection())
            {
                return conn.ExecuteScalar<int>(sql.RawSql, sql.Parameters);
            }
        }

        protected void ModelCreated(TModel model, bool forcePublish = false)
        {
            PublishModelEvent(model, ModelAction.Created, forcePublish);
        }

        protected void ModelUpdated(TModel model, bool forcePublish = false)
        {
            PublishModelEvent(model, ModelAction.Updated, forcePublish);
        }

        protected void ModelDeleted(TModel model, bool forcePublish = false)
        {
            PublishModelEvent(model, ModelAction.Deleted, forcePublish);
        }

        private void PublishModelEvent(TModel model, ModelAction action, bool forcePublish)
        {
            if (PublishModelEvents || forcePublish)
            {
                _eventAggregator.PublishEvent(new ModelEvent<TModel>(model, action));
            }
        }

        protected virtual bool PublishModelEvents => false;
    }
}
