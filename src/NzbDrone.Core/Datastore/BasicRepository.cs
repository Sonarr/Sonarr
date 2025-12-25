using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using NLog;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Messaging.Events;
using Polly;
using Polly.Retry;

namespace NzbDrone.Core.Datastore
{
    public interface IBasicRepository<TModel>
        where TModel : ModelBase, new()
    {
        IEnumerable<TModel> All();
        int Count();
        TModel Find(int id);
        TModel Get(int id);
        TModel Insert(TModel model);
        TModel Update(TModel model);
        TModel Upsert(TModel model);
        void SetFields(TModel model, params Expression<Func<TModel, object>>[] properties);
        void Delete(TModel model);
        void Delete(int id);
        IEnumerable<TModel> Get(IEnumerable<int> ids);
        void InsertMany(IList<TModel> model);
        void UpdateMany(IList<TModel> model);
        void SetFields(IList<TModel> models, params Expression<Func<TModel, object>>[] properties);
        void DeleteMany(List<TModel> model);
        void DeleteMany(IEnumerable<int> ids);
        void Purge(bool vacuum = false);
        bool HasItems();
        TModel Single();
        TModel SingleOrDefault();
        PagingSpec<TModel> GetPaged(PagingSpec<TModel> pagingSpec);

        // Async methods
        Task<IEnumerable<TModel>> AllAsync(CancellationToken cancellationToken = default);
        Task<int> CountAsync(CancellationToken cancellationToken = default);
        Task<TModel> FindAsync(int id, CancellationToken cancellationToken = default);
        Task<TModel> GetAsync(int id, CancellationToken cancellationToken = default);
        Task<TModel> InsertAsync(TModel model, CancellationToken cancellationToken = default);
        Task<TModel> UpdateAsync(TModel model, CancellationToken cancellationToken = default);
        Task<TModel> UpsertAsync(TModel model, CancellationToken cancellationToken = default);
        Task SetFieldsAsync(TModel model, CancellationToken cancellationToken = default, params Expression<Func<TModel, object>>[] properties);
        Task DeleteAsync(TModel model, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<TModel>> GetAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
        Task InsertManyAsync(IList<TModel> models, CancellationToken cancellationToken = default);
        Task UpdateManyAsync(IList<TModel> models, CancellationToken cancellationToken = default);
        Task SetFieldsAsync(IList<TModel> models, CancellationToken cancellationToken = default, params Expression<Func<TModel, object>>[] properties);
        Task DeleteManyAsync(List<TModel> models, CancellationToken cancellationToken = default);
        Task DeleteManyAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
        Task PurgeAsync(bool vacuum = false, CancellationToken cancellationToken = default);
        Task<bool> HasItemsAsync(CancellationToken cancellationToken = default);
        Task<TModel> SingleAsync(CancellationToken cancellationToken = default);
        Task<TModel> SingleOrDefaultAsync(CancellationToken cancellationToken = default);
        Task<PagingSpec<TModel>> GetPagedAsync(PagingSpec<TModel> pagingSpec, CancellationToken cancellationToken = default);
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

        protected virtual List<TModel> QueryDistinct(SqlBuilder builder) => _database.QueryDistinct<TModel>(builder).ToList();

        protected List<TModel> Query(Expression<Func<TModel, bool>> where) => Query(Builder().Where(where));

        protected virtual async Task<List<TModel>> QueryAsync(SqlBuilder builder, CancellationToken cancellationToken = default)
        {
            var type = typeof(TModel);
            var sql = builder.Select(type).AddSelectTemplate(type).LogQuery();

            using var conn = await _database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            var cmd = new CommandDefinition(sql.RawSql, sql.Parameters, cancellationToken: cancellationToken);
            var results = await conn.QueryAsync<TModel>(cmd).ConfigureAwait(false);
            return results.ToList();
        }

        protected virtual async Task<List<TModel>> QueryDistinctAsync(SqlBuilder builder, CancellationToken cancellationToken = default)
        {
            var type = typeof(TModel);
            var sql = builder.SelectDistinct(type).AddSelectTemplate(type).LogQuery();

            using var conn = await _database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            var cmd = new CommandDefinition(sql.RawSql, sql.Parameters, cancellationToken: cancellationToken);
            var results = await conn.QueryAsync<TModel>(cmd).ConfigureAwait(false);
            return results.ToList();
        }

        protected Task<List<TModel>> QueryAsync(Expression<Func<TModel, bool>> where, CancellationToken cancellationToken = default)
        {
            return QueryAsync(Builder().Where(where), cancellationToken);
        }

        public int Count()
        {
            using (var conn = _database.OpenConnection())
            {
                return conn.ExecuteScalar<int>($"SELECT COUNT(*) FROM \"{_table}\"");
            }
        }

        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            using var conn = await _database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            var cmd = new CommandDefinition($"SELECT COUNT(*) FROM \"{_table}\"", cancellationToken: cancellationToken);
            return await conn.ExecuteScalarAsync<int>(cmd).ConfigureAwait(false);
        }

        public virtual IEnumerable<TModel> All()
        {
            return Query(Builder());
        }

        public virtual async Task<IEnumerable<TModel>> AllAsync(CancellationToken cancellationToken = default)
        {
            return await QueryAsync(Builder(), cancellationToken).ConfigureAwait(false);
        }

        public TModel Find(int id)
        {
            var model = Query(x => x.Id == id).FirstOrDefault();

            return model;
        }

        public async Task<TModel> FindAsync(int id, CancellationToken cancellationToken = default)
        {
            var results = await QueryAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);
            return results.FirstOrDefault();
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
            var model = await FindAsync(id, cancellationToken).ConfigureAwait(false);

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

        public async Task<IEnumerable<TModel>> GetAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
        {
            if (!ids.Any())
            {
                return Array.Empty<TModel>();
            }

            var result = await QueryAsync(x => ids.Contains(x.Id), cancellationToken).ConfigureAwait(false);

            if (result.Count != ids.Count())
            {
                throw new ApplicationException($"Expected query to return {ids.Count()} rows but returned {result.Count}");
            }

            return result;
        }

        public TModel SingleOrDefault()
        {
            return All().SingleOrDefault();
        }

        public async Task<TModel> SingleOrDefaultAsync(CancellationToken cancellationToken = default)
        {
            var results = await AllAsync(cancellationToken).ConfigureAwait(false);
            return results.SingleOrDefault();
        }

        public TModel Single()
        {
            return All().Single();
        }

        public async Task<TModel> SingleAsync(CancellationToken cancellationToken = default)
        {
            var results = await AllAsync(cancellationToken).ConfigureAwait(false);
            return results.Single();
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

        public void UpdateMany(IList<TModel> models)
        {
            if (models.Any(x => x.Id == 0))
            {
                throw new InvalidOperationException("Can't update model with ID 0");
            }

            using (var conn = _database.OpenConnection())
            {
                UpdateFields(conn, null, models, _properties);
            }
        }

        protected void Delete(Expression<Func<TModel, bool>> where)
        {
            Delete(Builder().Where<TModel>(where));
        }

        protected void Delete(SqlBuilder builder)
        {
            var sql = builder.AddDeleteTemplate(typeof(TModel));

            using (var conn = _database.OpenConnection())
            {
                conn.Execute(sql.RawSql, sql.Parameters);
            }
        }

        public void Delete(TModel model)
        {
            Delete(model.Id);
        }

        public void Delete(int id)
        {
            Delete(x => x.Id == id);
        }

        public void DeleteMany(IEnumerable<int> ids)
        {
            if (ids.Any())
            {
                Delete(x => ids.Contains(x.Id));
            }
        }

        public void DeleteMany(List<TModel> models)
        {
            DeleteMany(models.Select(m => m.Id));
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

        protected void Vacuum()
        {
            _database.Vacuum();
        }

        public bool HasItems()
        {
            return Count() > 0;
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

        public void SetFields(IList<TModel> models, params Expression<Func<TModel, object>>[] properties)
        {
            if (models.Any(x => x.Id == 0))
            {
                throw new InvalidOperationException("Attempted to update model without ID");
            }

            var propertiesToUpdate = properties.Select(x => x.GetMemberName()).ToList();

            using (var conn = _database.OpenConnection())
            {
                UpdateFields(conn, null, models, propertiesToUpdate);
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

        private void UpdateFields(IDbConnection connection, IDbTransaction transaction, TModel model, List<string> propertyNamesToUpdate)
        {
            var propertiesToUpdate = _properties.Where(p => propertyNamesToUpdate.Contains(p.Name)).ToList();
            UpdateFields(connection, transaction, model, propertiesToUpdate);
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

        private void UpdateFields(IDbConnection connection, IDbTransaction transaction, IList<TModel> models, List<string> propertyNamesToUpdate)
        {
            var propertiesToUpdate = _properties.Where(p => propertyNamesToUpdate.Contains(p.Name)).ToList();
            UpdateFields(connection, transaction, models, propertiesToUpdate);
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

        protected async Task ModelCreatedAsync(TModel model, bool forcePublish = false, CancellationToken cancellationToken = default)
        {
            await PublishModelEventAsync(model, ModelAction.Created, forcePublish, cancellationToken).ConfigureAwait(false);
        }

        protected async Task ModelUpdatedAsync(TModel model, bool forcePublish = false, CancellationToken cancellationToken = default)
        {
            await PublishModelEventAsync(model, ModelAction.Updated, forcePublish, cancellationToken).ConfigureAwait(false);
        }

        protected async Task ModelDeletedAsync(TModel model, bool forcePublish = false, CancellationToken cancellationToken = default)
        {
            await PublishModelEventAsync(model, ModelAction.Deleted, forcePublish, cancellationToken).ConfigureAwait(false);
        }

        // TODO: Sync wrappers for existing sync methods - will be removed in later phase
        protected void ModelCreated(TModel model, bool forcePublish = false)
        {
            PublishModelEventAsync(model, ModelAction.Created, forcePublish).GetAwaiter().GetResult();
        }

        protected void ModelUpdated(TModel model, bool forcePublish = false)
        {
            PublishModelEventAsync(model, ModelAction.Updated, forcePublish).GetAwaiter().GetResult();
        }

        protected void ModelDeleted(TModel model, bool forcePublish = false)
        {
            PublishModelEventAsync(model, ModelAction.Deleted, forcePublish).GetAwaiter().GetResult();
        }

        private async Task PublishModelEventAsync(TModel model, ModelAction action, bool forcePublish, CancellationToken cancellationToken = default)
        {
            if (PublishModelEvents || forcePublish)
            {
                await _eventAggregator.PublishEventAsync(new ModelEvent<TModel>(model, action), cancellationToken).ConfigureAwait(false);
            }
        }

        protected virtual bool PublishModelEvents => false;

        public async Task<TModel> InsertAsync(TModel model, CancellationToken cancellationToken = default)
        {
            if (model.Id != 0)
            {
                throw new InvalidOperationException("Can't insert model with existing ID " + model.Id);
            }

            using var conn = await _database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            model = await InsertAsync(conn, null, model, cancellationToken).ConfigureAwait(false);

            await ModelCreatedAsync(model, cancellationToken: cancellationToken).ConfigureAwait(false);

            return model;
        }

        private async Task<TModel> InsertAsync(IDbConnection connection, IDbTransaction transaction, TModel model, CancellationToken cancellationToken)
        {
            SqlBuilderExtensions.LogQuery(_insertSql, model);

            var cmd = new CommandDefinition(_insertSql, model, transaction, cancellationToken: cancellationToken);
            var multi = await RetryStrategy.ExecuteAsync(static async (state, ct) =>
                await state.connection.QueryMultipleAsync(state.cmd).ConfigureAwait(false),
                (connection, cmd),
                cancellationToken).ConfigureAwait(false);

            var multiRead = await multi.ReadAsync().ConfigureAwait(false);
            var id = (int)(multiRead.First().id ?? multiRead.First().Id);
            _keyProperty.SetValue(model, id);

            return model;
        }

        public async Task InsertManyAsync(IList<TModel> models, CancellationToken cancellationToken = default)
        {
            if (models.Any(x => x.Id != 0))
            {
                throw new InvalidOperationException("Can't insert model with existing ID != 0");
            }

            using var conn = await _database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            await using var tran = await conn.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken).ConfigureAwait(false);
            foreach (var model in models)
            {
                await InsertAsync(conn, tran, model, cancellationToken).ConfigureAwait(false);
            }

            await tran.CommitAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<TModel> UpdateAsync(TModel model, CancellationToken cancellationToken = default)
        {
            if (model.Id == 0)
            {
                throw new InvalidOperationException("Can't update model with ID 0");
            }

            using var conn = await _database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            await UpdateFieldsAsync(conn, null, model, _properties, cancellationToken).ConfigureAwait(false);

            await ModelUpdatedAsync(model, cancellationToken: cancellationToken).ConfigureAwait(false);

            return model;
        }

        public async Task UpdateManyAsync(IList<TModel> models, CancellationToken cancellationToken = default)
        {
            if (models.Any(x => x.Id == 0))
            {
                throw new InvalidOperationException("Can't update model with ID 0");
            }

            using var conn = await _database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            await UpdateFieldsAsync(conn, null, models, _properties, cancellationToken).ConfigureAwait(false);
        }

        protected async Task DeleteAsync(Expression<Func<TModel, bool>> where, CancellationToken cancellationToken = default)
        {
            await DeleteAsync(Builder().Where<TModel>(where), cancellationToken).ConfigureAwait(false);
        }

        protected async Task DeleteAsync(SqlBuilder builder, CancellationToken cancellationToken = default)
        {
            var sql = builder.AddDeleteTemplate(typeof(TModel));

            using var conn = await _database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            var cmd = new CommandDefinition(sql.RawSql, sql.Parameters, cancellationToken: cancellationToken);
            await conn.ExecuteAsync(cmd).ConfigureAwait(false);
        }

        public async Task DeleteAsync(TModel model, CancellationToken cancellationToken = default)
        {
            await DeleteAsync(model.Id, cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            await DeleteAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteManyAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
        {
            if (ids.Any())
            {
                await DeleteAsync(x => ids.Contains(x.Id), cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task DeleteManyAsync(List<TModel> models, CancellationToken cancellationToken = default)
        {
            await DeleteManyAsync(models.Select(m => m.Id), cancellationToken).ConfigureAwait(false);
        }

        public async Task SetFieldsAsync(TModel model, CancellationToken cancellationToken = default, params Expression<Func<TModel, object>>[] properties)
        {
            if (model.Id == 0)
            {
                throw new InvalidOperationException("Attempted to update model without ID");
            }

            var propertiesToUpdate = properties.Select(x => x.GetMemberName()).ToList();

            using var conn = await _database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            await UpdateFieldsAsync(conn, null, model, propertiesToUpdate, cancellationToken).ConfigureAwait(false);

            await ModelUpdatedAsync(model, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        public async Task SetFieldsAsync(IList<TModel> models, CancellationToken cancellationToken = default, params Expression<Func<TModel, object>>[] properties)
        {
            if (models.Any(x => x.Id == 0))
            {
                throw new InvalidOperationException("Attempted to update model without ID");
            }

            var propertiesToUpdate = properties.Select(x => x.GetMemberName()).ToList();

            using var conn = await _database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            await UpdateFieldsAsync(conn, null, models, propertiesToUpdate, cancellationToken).ConfigureAwait(false);

            foreach (var model in models)
            {
                await ModelUpdatedAsync(model, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task UpdateFieldsAsync(IDbConnection connection, IDbTransaction transaction, TModel model, List<PropertyInfo> propertiesToUpdate, CancellationToken cancellationToken)
        {
            var sql = propertiesToUpdate == _properties ? _updateSql : GetUpdateSql(propertiesToUpdate);

            SqlBuilderExtensions.LogQuery(sql, model);

            var cmd = new CommandDefinition(sql, model, transaction, cancellationToken: cancellationToken);
            await RetryStrategy.ExecuteAsync(static async (state, ct) =>
                await state.connection.ExecuteAsync(state.cmd).ConfigureAwait(false),
                (connection, cmd),
                cancellationToken).ConfigureAwait(false);
        }

        private async Task UpdateFieldsAsync(IDbConnection connection, IDbTransaction transaction, TModel model, List<string> propertyNamesToUpdate, CancellationToken cancellationToken)
        {
            var propertiesToUpdate = _properties.Where(p => propertyNamesToUpdate.Contains(p.Name)).ToList();
            await UpdateFieldsAsync(connection, transaction, model, propertiesToUpdate, cancellationToken).ConfigureAwait(false);
        }

        private async Task UpdateFieldsAsync(IDbConnection connection, IDbTransaction transaction, IList<TModel> models, List<PropertyInfo> propertiesToUpdate, CancellationToken cancellationToken)
        {
            var sql = propertiesToUpdate == _properties ? _updateSql : GetUpdateSql(propertiesToUpdate);

            foreach (var model in models)
            {
                SqlBuilderExtensions.LogQuery(sql, model);
            }

            var cmd = new CommandDefinition(sql, models, transaction, cancellationToken: cancellationToken);
            await RetryStrategy.ExecuteAsync(static async (state, ct) =>
                await state.connection.ExecuteAsync(state.cmd).ConfigureAwait(false),
                (connection, cmd),
                cancellationToken).ConfigureAwait(false);
        }

        private async Task UpdateFieldsAsync(IDbConnection connection, IDbTransaction transaction, IList<TModel> models, List<string> propertyNamesToUpdate, CancellationToken cancellationToken)
        {
            var propertiesToUpdate = _properties.Where(p => propertyNamesToUpdate.Contains(p.Name)).ToList();
            await UpdateFieldsAsync(connection, transaction, models, propertiesToUpdate, cancellationToken).ConfigureAwait(false);
        }

        public async Task<TModel> UpsertAsync(TModel model, CancellationToken cancellationToken = default)
        {
            if (model.Id == 0)
            {
                await InsertAsync(model, cancellationToken).ConfigureAwait(false);
                return model;
            }

            await UpdateAsync(model, cancellationToken).ConfigureAwait(false);
            return model;
        }

        public async Task PurgeAsync(bool vacuum = false, CancellationToken cancellationToken = default)
        {
            using var conn = await _database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            var cmd = new CommandDefinition($"DELETE FROM \"{_table}\"", cancellationToken: cancellationToken);
            await conn.ExecuteAsync(cmd).ConfigureAwait(false);

            if (vacuum)
            {
                await VacuumAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task<bool> HasItemsAsync(CancellationToken cancellationToken = default)
        {
            return await CountAsync(cancellationToken).ConfigureAwait(false) > 0;
        }

        protected virtual async Task<IEnumerable<TModel>> PagedQueryAsync(SqlBuilder sql, CancellationToken cancellationToken = default)
        {
            return await QueryAsync(sql, cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task<PagingSpec<TModel>> GetPagedAsync(PagingSpec<TModel> pagingSpec, CancellationToken cancellationToken = default)
        {
            pagingSpec.Records = await GetPagedRecordsAsync(PagedBuilder(), pagingSpec, PagedQueryAsync, cancellationToken).ConfigureAwait(false);
            pagingSpec.TotalRecords = await GetPagedRecordCountAsync(PagedBuilder().SelectCount(), pagingSpec, cancellationToken: cancellationToken).ConfigureAwait(false);

            return pagingSpec;
        }

        protected async Task<List<TModel>> GetPagedRecordsAsync(SqlBuilder builder, PagingSpec<TModel> pagingSpec, Func<SqlBuilder, CancellationToken, Task<IEnumerable<TModel>>> queryFuncAsync, CancellationToken cancellationToken = default)
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

            var result = await queryFuncAsync(builder, cancellationToken).ConfigureAwait(false);
            return result.ToList();
        }

        protected async Task<int> GetPagedRecordCountAsync(SqlBuilder builder, PagingSpec<TModel> pagingSpec, string template = null, CancellationToken cancellationToken = default)
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

            using var conn = await _database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            var cmd = new CommandDefinition(sql.RawSql, sql.Parameters, cancellationToken: cancellationToken);
            return await conn.ExecuteScalarAsync<int>(cmd).ConfigureAwait(false);
        }

        protected Task VacuumAsync(CancellationToken cancellationToken = default)
        {
            return _database.VacuumAsync(cancellationToken);
        }
    }
}
