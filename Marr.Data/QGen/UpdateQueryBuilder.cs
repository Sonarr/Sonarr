using System;
using System.Collections.Generic;
using Marr.Data.Mapping;
using System.Linq.Expressions;

namespace Marr.Data.QGen
{
    public class UpdateQueryBuilder<T>
    {
        private DataMapper _db;
        private string _tableName;
        private T _entity;
        private MappingHelper _mappingHelper;
        private ColumnMapCollection _mappings;
        private SqlModes _previousSqlMode;
        private bool _generateQuery = true;
        private TableCollection _tables;
        private Expression<Func<T, bool>> _filterExpression;
        private Dialects.Dialect _dialect;
        private ColumnMapCollection _columnsToUpdate;

        public UpdateQueryBuilder()
        {
            // Used only for unit testing with mock frameworks
        }

        public UpdateQueryBuilder(DataMapper db)
        {
            _db = db;
            _tableName = MapRepository.Instance.GetTableName(typeof(T));
            _tables = new TableCollection();
            _tables.Add(new Table(typeof(T)));
            _previousSqlMode = _db.SqlMode;
            _mappingHelper = new MappingHelper(_db);
            _mappings = MapRepository.Instance.GetColumns(typeof(T));
            _dialect = QueryFactory.CreateDialect(_db);
        }

        public virtual UpdateQueryBuilder<T> TableName(string tableName)
        {
            _tableName = tableName;
            return this;
        }

        public virtual UpdateQueryBuilder<T> QueryText(string queryText)
        {
            _generateQuery = false;
            _db.Command.CommandText = queryText;
            return this;
        }

        public virtual UpdateQueryBuilder<T> Entity(T entity)
        {
            _entity = entity;
            return this;
        }

        public virtual UpdateQueryBuilder<T> Where(Expression<Func<T, bool>> filterExpression)
        {
            _filterExpression = filterExpression;
            return this;
        }

        public virtual UpdateQueryBuilder<T> ColumnsIncluding(params Expression<Func<T, object>>[] properties)
        {
            List<string> columnList = new List<string>();

            foreach (var column in properties)
            {
                columnList.Add(column.GetMemberName());
            }

            return ColumnsIncluding(columnList.ToArray());
        }

        public virtual UpdateQueryBuilder<T> ColumnsIncluding(params string[] properties)
        {
            _columnsToUpdate = new ColumnMapCollection();

            foreach (string propertyName in properties)
            {
                _columnsToUpdate.Add(_mappings.GetByFieldName(propertyName));
            }

            return this;
        }

        public virtual UpdateQueryBuilder<T> ColumnsExcluding(params Expression<Func<T, object>>[] properties)
        {
            List<string> columnList = new List<string>();

            foreach (var column in properties)
            {
                columnList.Add(column.GetMemberName());
            }

            return ColumnsExcluding(columnList.ToArray());
        }

        public virtual UpdateQueryBuilder<T> ColumnsExcluding(params string[] properties)
        {
            _columnsToUpdate = new ColumnMapCollection();

            _columnsToUpdate.AddRange(_mappings);

            foreach (string propertyName in properties)
            {
                _columnsToUpdate.RemoveAll(c => c.FieldName == propertyName);
            }

            return this;
        }

        public virtual string BuildQuery()
        {
            if (_entity == null)
                throw new ArgumentNullException("You must specify an entity to update.");

            // Override SqlMode since we know this will be a text query
            _db.SqlMode = SqlModes.Text;

            var columnsToUpdate = _columnsToUpdate ?? _mappings;

            _mappingHelper.CreateParameters<T>(_entity, columnsToUpdate, _generateQuery);

            string where = string.Empty;
            if (_filterExpression != null)
            {
                var whereBuilder = new WhereBuilder<T>(_db.Command, _dialect, _filterExpression, _tables, false, false);
                where = whereBuilder.ToString();
            }

            IQuery query = QueryFactory.CreateUpdateQuery(columnsToUpdate, _db, _tableName, where);

            _db.Command.CommandText = query.Generate();

            return _db.Command.CommandText;
        }

        public virtual int Execute()
        {
            if (_generateQuery)
            {
                BuildQuery();
            }
            else
            {
                _mappingHelper.CreateParameters<T>(_entity, _mappings, _generateQuery);
            }

            int rowsAffected = 0;

            try
            {
                _db.OpenConnection();
                rowsAffected = _db.Command.ExecuteNonQuery();
                _mappingHelper.SetOutputValues<T>(_entity, _mappings.OutputFields);                
            }
            finally
            {
                _db.CloseConnection();
            }


            if (_generateQuery)
            {
                // Return to previous sql mode
                _db.SqlMode = _previousSqlMode;
            }

            return rowsAffected;
        }
    }
}
