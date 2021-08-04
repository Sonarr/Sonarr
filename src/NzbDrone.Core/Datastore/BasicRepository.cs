using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Dapper;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Messaging.Events;

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
    }

    public class BasicRepository<TModel> : IBasicRepository<TModel>
        where TModel : ModelBase, new()
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly PropertyInfo _keyProperty;
        private readonly List<PropertyInfo> _properties;
        private readonly string _updateSql;
        private readonly string _insertSql;

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

        protected virtual SqlBuilder Builder() => new SqlBuilder();

        protected virtual List<TModel> Query(SqlBuilder builder) => _database.Query<TModel>(builder).ToList();

        protected virtual List<TModel> QueryDistinct(SqlBuilder builder) => _database.QueryDistinct<TModel>(builder).ToList();

        protected List<TModel> Query(Expression<Func<TModel, bool>> where) => Query(Builder().Where(where));

        public int Count()
        {
            using (var conn = _database.OpenConnection())
            {
                return conn.ExecuteScalar<int>($"SELECT COUNT(*) FROM {_table}");
            }
        }

        public virtual IEnumerable<TModel> All()
        {
            return Query(Builder());
        }

        public TModel Find(int id)
        {
            var model = Query(x => x.Id == id).FirstOrDefault();

            return model;
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

        public IEnumerable<TModel> Get(IEnumerable<int> ids)
        {
            if (!ids.Any())
            {
                return new List<TModel>();
            }

            var result = Query(x => ids.Contains(x.Id));

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

        public TModel Single()
        {
            return All().Single();
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

            return $"INSERT INTO {_table} ({sbColumnList.ToString()}) VALUES ({sbParameterList.ToString()}); SELECT last_insert_rowid() id";
        }

        private TModel Insert(IDbConnection connection, IDbTransaction transaction, TModel model)
        {
            SqlBuilderExtensions.LogQuery(_insertSql, model);
            var multi = connection.QueryMultiple(_insertSql, model, transaction);
            var id = (int)multi.Read().First().id;
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
                using (IDbTransaction tran = conn.BeginTransaction(IsolationLevel.ReadCommitted))
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
            var sql = builder.AddDeleteTemplate(typeof(TModel)).LogQuery();

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
                conn.Execute($"DELETE FROM [{_table}]");
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
            sb.AppendFormat("UPDATE {0} SET ", _table);

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

            connection.Execute(sql, model, transaction: transaction);
        }

        private void UpdateFields(IDbConnection connection, IDbTransaction transaction, IList<TModel> models, List<PropertyInfo> propertiesToUpdate)
        {
            var sql = propertiesToUpdate == _properties ? _updateSql : GetUpdateSql(propertiesToUpdate);

            foreach (var model in models)
            {
                SqlBuilderExtensions.LogQuery(sql, model);
            }

            connection.Execute(sql, models, transaction: transaction);
        }

        protected virtual SqlBuilder PagedBuilder() => Builder();
        protected virtual IEnumerable<TModel> PagedQuery(SqlBuilder sql) => Query(sql);

        public virtual PagingSpec<TModel> GetPaged(PagingSpec<TModel> pagingSpec)
        {
            pagingSpec.Records = GetPagedRecords(PagedBuilder(), pagingSpec, PagedQuery);
            pagingSpec.TotalRecords = GetPagedRecordCount(PagedBuilder().SelectCount(), pagingSpec);

            return pagingSpec;
        }

        private void AddFilters(SqlBuilder builder, PagingSpec<TModel> pagingSpec)
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

            var sortDirection = pagingSpec.SortDirection == SortDirection.Descending ? "DESC" : "ASC";
            var pagingOffset = (pagingSpec.Page - 1) * pagingSpec.PageSize;
            builder.OrderBy($"{pagingSpec.SortKey} {sortDirection} LIMIT {pagingSpec.PageSize} OFFSET {pagingOffset}");

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
