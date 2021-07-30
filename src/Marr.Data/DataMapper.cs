/*  Copyright (C) 2008 - 2011 Jordan Marr

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 3 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library. If not, see <http://www.gnu.org/licenses/>. */

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Collections;
using Marr.Data.Mapping;
using Marr.Data.Parameters;
using Marr.Data.QGen;
using System.Linq.Expressions;
using System.Diagnostics;

namespace Marr.Data
{
    /// <summary>
    /// This class is the main access point for making database related calls.
    /// </summary>
    public class DataMapper : IDataMapper
    {

        #region - Contructor, Members -

        private DbCommand _command;

        /// <summary>
        /// A database provider agnostic initialization.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        public DataMapper(DbProviderFactory dbProviderFactory, string connectionString)
        {
            SqlMode = SqlModes.StoredProcedure;
            if (dbProviderFactory == null)
                throw new ArgumentNullException("dbProviderFactory");

            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException("connectionString");

            ProviderFactory = dbProviderFactory;

            ConnectionString = connectionString;
        }

        public string ConnectionString { get; private set; }

        public DbProviderFactory ProviderFactory { get; private set; }

        /// <summary>
        /// Creates a new command utilizing the connection string.
        /// </summary>
        private DbCommand CreateNewCommand()
        {
            DbConnection conn = ProviderFactory.CreateConnection();
            conn.ConnectionString = ConnectionString;
            DbCommand cmd = conn.CreateCommand();
            SetSqlMode(cmd);
            return cmd;
        }

        /// <summary>
        /// Creates a new command utilizing the connection string with a given SQL command.
        /// </summary>
        private DbCommand CreateNewCommand(string sql)
        {
            DbCommand cmd = CreateNewCommand();
            cmd.CommandText = sql;
            return cmd;
        }

        /// <summary>
        /// Gets or creates a DbCommand object.
        /// </summary>
        public DbCommand Command
        {
            get
            {
                // Lazy load
                if (_command == null)
                    _command = CreateNewCommand();
                else
                    SetSqlMode(_command); // Set SqlMode every time.

                return _command;
            }
        }

        #endregion

        #region - Parameters -

        public DbParameterCollection Parameters
        {
            get
            {
                return Command.Parameters;
            }
        }

        public ParameterChainMethods AddParameter(string name, object value)
        {
            return new ParameterChainMethods(Command, name, value);
        }

        public IDbDataParameter AddParameter(IDbDataParameter parameter)
        {
            // Convert null values to DBNull.Value
            if (parameter.Value == null)
                parameter.Value = DBNull.Value;

            Parameters.Add(parameter);
            return parameter;
        }

        #endregion

        #region - SP / SQL Mode -

        /// <summary>
        /// Gets or sets a value that determines whether the DataMapper will 
        /// use a stored procedure or a sql text command to access 
        /// the database.  The default is stored procedure.
        /// </summary>
        public SqlModes SqlMode { get; set; }

        /// <summary>
        /// Sets the DbCommand objects CommandType to the current SqlMode.
        /// </summary>
        /// <param name="command">The DbCommand object we are modifying.</param>
        /// <returns>Returns the same DbCommand that was passed in.</returns>
        private DbCommand SetSqlMode(DbCommand command)
        {
            if (SqlMode == SqlModes.StoredProcedure)
                command.CommandType = CommandType.StoredProcedure;
            else
                command.CommandType = CommandType.Text;

            return command;
        }

        #endregion

        #region - ExecuteScalar, ExecuteNonQuery, ExecuteReader -

        /// <summary>
        /// Executes a stored procedure that returns a scalar value.
        /// </summary>
        /// <param name="sql">The SQL command to execute.</param>
        /// <returns>A scalar value</returns>
        public object ExecuteScalar(string sql)
        {
            if (string.IsNullOrEmpty(sql))
                throw new ArgumentNullException("sql", "A SQL query or stored procedure name is required");
            Command.CommandText = sql;

            try
            {
                OpenConnection();
                return Command.ExecuteScalar();
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Executes a non query that returns an integer.
        /// </summary>
        /// <param name="sql">The SQL command to execute.</param>
        /// <returns>An integer value</returns>
        public int ExecuteNonQuery(string sql)
        {
            if (string.IsNullOrEmpty(sql))
                throw new ArgumentNullException("sql", "A SQL query or stored procedure name is required");
            Command.CommandText = sql;

            try
            {
                OpenConnection();
                return Command.ExecuteNonQuery();
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Executes a DataReader that can be controlled using a Func delegate.
        /// (Note that reader.Read() will be called automatically).
        /// </summary>
        /// <typeparam name="TResult">The type that will be return in the result set.</typeparam>
        /// <param name="sql">The sql statement that will be executed.</param>
        /// <param name="func">The function that will build the the TResult set.</param>
        /// <returns>An IEnumerable of TResult.</returns>
        public IEnumerable<TResult> ExecuteReader<TResult>(string sql, Func<DbDataReader, TResult> func)
        {
            if (string.IsNullOrEmpty(sql))
                throw new ArgumentNullException("sql", "A SQL query or stored procedure name is required");
            Command.CommandText = sql;

            try
            {
                OpenConnection();

                var list = new List<TResult>();
                DbDataReader reader = null;
                try
                {
                    reader = Command.ExecuteReader();

                    while (reader.Read())
                    {
                        list.Add(func(reader));
                    }

                    return list;
                }
                finally
                {
                    if (reader != null) reader.Close();
                }
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Executes a DataReader that can be controlled using an Action delegate.
        /// </summary>
        /// <param name="sql">The sql statement that will be executed.</param>
        /// <param name="action">The delegate that will work with the result set.</param>
        public void ExecuteReader(string sql, Action<DbDataReader> action)
        {
            if (string.IsNullOrEmpty(sql))
                throw new ArgumentNullException("sql", "A SQL query or stored procedure name is required");

            Command.CommandText = sql;

            try
            {
                OpenConnection();

                DbDataReader reader = null;
                try
                {
                    reader = Command.ExecuteReader();

                    while (reader.Read())
                    {
                        action(reader);
                    }
                }
                finally
                {
                    if (reader != null) reader.Close();
                }
            }
            finally
            {
                CloseConnection();
            }
        }

        #endregion

        #region - DataSets -

        public DataSet GetDataSet(string sql)
        {
            return GetDataSet(sql, new DataSet(), null);
        }

        public DataSet GetDataSet(string sql, DataSet ds, string tableName)
        {
            if (string.IsNullOrEmpty(sql))
                throw new ArgumentNullException("sql", "A SQL query or stored procedure name is required");

            try
            {
                using (DbDataAdapter adapter = ProviderFactory.CreateDataAdapter())
                {
                    Command.CommandText = sql;
                    adapter.SelectCommand = Command;

                    if (ds == null)
                        ds = new DataSet();

                    OpenConnection();

                    if (string.IsNullOrEmpty(tableName))
                        adapter.Fill(ds);
                    else
                        adapter.Fill(ds, tableName);

                    return ds;
                }
            }
            finally
            {
                CloseConnection();  // Clears parameters
            }
        }

        public DataTable GetDataTable(string sql)
        {
            return GetDataTable(sql, null, null);
        }

        public DataTable GetDataTable(string sql, DataTable dt, string tableName)
        {
            if (string.IsNullOrEmpty(sql))
                throw new ArgumentNullException("sql", "A SQL query or stored procedure name is required");

            try
            {
                using (DbDataAdapter adapter = ProviderFactory.CreateDataAdapter())
                {
                    Command.CommandText = sql;
                    adapter.SelectCommand = Command;

                    if (dt == null)
                        dt = new DataTable();

                    adapter.Fill(dt);

                    if (!string.IsNullOrEmpty(tableName))
                        dt.TableName = tableName;

                    return dt;
                }
            }
            finally
            {
                CloseConnection();  // Clears parameters
            }
        }

        public int UpdateDataSet(DataSet ds, string sql)
        {
            if (string.IsNullOrEmpty(sql))
                throw new ArgumentNullException("sql", "A SQL query or stored procedure name is required");

            if (ds == null)
                throw new ArgumentNullException("ds", "DataSet cannot be null.");

            DbDataAdapter adapter = null;

            try
            {
                adapter = ProviderFactory.CreateDataAdapter();

                adapter.UpdateCommand = Command;
                adapter.UpdateCommand.CommandText = sql;

                return adapter.Update(ds);
            }
            finally
            {
                if (adapter.UpdateCommand != null)
                    adapter.UpdateCommand.Dispose();

                adapter.Dispose();
            }
        }

        public int InsertDataTable(DataTable table, string insertSP)
        {
            return InsertDataTable(table, insertSP, UpdateRowSource.None);
        }

        public int InsertDataTable(DataTable dt, string sql, UpdateRowSource updateRowSource)
        {
            if (string.IsNullOrEmpty(sql))
                throw new ArgumentNullException("sql", "A SQL query or stored procedure name is required");

            if (dt == null)
                throw new ArgumentNullException("dt", "DataTable cannot be null.");

            DbDataAdapter adapter = null;

            try
            {
                adapter = ProviderFactory.CreateDataAdapter();

                adapter.InsertCommand = Command;
                adapter.InsertCommand.CommandText = sql;

                adapter.InsertCommand.UpdatedRowSource = updateRowSource;

                return adapter.Update(dt);
            }
            finally
            {
                if (adapter.InsertCommand != null)
                    adapter.InsertCommand.Dispose();

                adapter.Dispose();
            }
        }

        public int DeleteDataTable(DataTable dt, string sql)
        {
            if (string.IsNullOrEmpty(sql))
                throw new ArgumentNullException("sql", "A SQL query or stored procedure name is required");

            if (dt == null)
                throw new ArgumentNullException("dt", "DataSet cannot be null.");

            DbDataAdapter adapter = null;

            try
            {
                adapter = ProviderFactory.CreateDataAdapter();

                adapter.DeleteCommand = Command;
                adapter.DeleteCommand.CommandText = sql;

                return adapter.Update(dt);
            }
            finally
            {
                if (adapter.DeleteCommand != null)
                    adapter.DeleteCommand.Dispose();

                adapter.Dispose();
            }
        }

        #endregion

        #region - Find -

        public T Find<T>(string sql)
        {
            return Find<T>(sql, default(T));
        }

        /// <summary>
        /// Returns an entity of type T.
        /// </summary>
        /// <typeparam name="T">The type of entity that is to be instantiated and loaded with values.</typeparam>
        /// <param name="sql">The SQL command to execute.</param>
        /// <returns>An instantiated and loaded entity of type T.</returns>
        public T Find<T>(string sql, T ent)
        {
            if (string.IsNullOrEmpty(sql))
                throw new ArgumentNullException("sql", "A stored procedure name has not been specified for 'Find'.");

            Type entityType = typeof(T);
            Command.CommandText = sql;

            MapRepository repository = MapRepository.Instance;
            ColumnMapCollection mappings = repository.GetColumns(entityType);

            bool isSimpleType = DataHelper.IsSimpleType(typeof(T));

            try
            {
                OpenConnection();
                var mappingHelper = new MappingHelper(this);

                using (DbDataReader reader = Command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        if (isSimpleType)
                        {
                            return mappingHelper.LoadSimpleValueFromFirstColumn<T>(reader);
                        }
                        else
                        {
                            if (ent == null)
                                ent = (T)mappingHelper.CreateAndLoadEntity<T>(mappings, reader, false);
                            else
                                mappingHelper.LoadExistingEntity(mappings, reader, ent, false);
                        }
                    }
                }
            }
            finally
            {
                CloseConnection();
            }

            return ent;
        }

        #endregion

        #region - Query -

        /// <summary>
        /// Creates a QueryBuilder that allows you to build a query.
        /// </summary>
        /// <typeparam name="T">The type of object that will be queried.</typeparam>
        /// <returns>Returns a QueryBuilder of T.</returns>
        public QueryBuilder<T> Query<T>()
        {
            var dialect = QueryFactory.CreateDialect(this);
            return new QueryBuilder<T>(this, dialect);
        }

        /// <summary>
        /// Returns the results of a query.
        /// Uses a List of type T to return the data.
        /// </summary>
        /// <returns>Returns a list of the specified type.</returns>
        public List<T> Query<T>(string sql)
        {
            return (List<T>)Query<T>(sql, new List<T>());
        }

        /// <summary>
        /// Returns the results of a SP query.
        /// </summary>
        /// <returns>Returns a list of the specified type.</returns>
        public ICollection<T> Query<T>(string sql, ICollection<T> entityList)
        {
            return Query<T>(sql, entityList, false);
        }

        internal ICollection<T> Query<T>(string sql, ICollection<T> entityList, bool useAltName)
        {
            if (entityList == null)
                throw new ArgumentNullException("entityList", "ICollection instance cannot be null.");

            if (string.IsNullOrEmpty(sql))
                throw new ArgumentNullException("sql", "A query or stored procedure has not been specified for 'Query'.");

            var mappingHelper = new MappingHelper(this);
            Type entityType = typeof(T);
            Command.CommandText = sql;
            ColumnMapCollection mappings = MapRepository.Instance.GetColumns(entityType);

            bool isSimpleType = DataHelper.IsSimpleType(typeof(T));

            try
            {
                OpenConnection();
                using (DbDataReader reader = Command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (isSimpleType)
                        {
                            entityList.Add(mappingHelper.LoadSimpleValueFromFirstColumn<T>(reader));
                        }
                        else
                        {
                            entityList.Add((T)mappingHelper.CreateAndLoadEntity<T>(mappings, reader, useAltName));
                        }
                    }
                }
            }
            finally
            {
                CloseConnection();
            }

            return entityList;
        }

        #endregion

        #region - Query to Graph -

        public List<T> QueryToGraph<T>(string sql)
        {
            return (List<T>)QueryToGraph<T>(sql, new List<T>());
        }

        public ICollection<T> QueryToGraph<T>(string sql, ICollection<T> entityList)
        {
            EntityGraph graph = new EntityGraph(typeof(T), (IList)entityList);
            return QueryToGraph<T>(sql, graph, new List<MemberInfo>());
        }

        /// <summary>
        /// Queries a view that joins multiple tables and returns an object graph.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="entityList"></param>
        /// <param name="entityGraph">Coordinates loading all objects in the graph..</param>
        /// <returns></returns>
        internal ICollection<T> QueryToGraph<T>(string sql, EntityGraph graph, List<MemberInfo> childrenToLoad)
        {
            if (string.IsNullOrEmpty(sql))
                throw new ArgumentNullException("sql", "sql");

            var mappingHelper = new MappingHelper(this);
            Type parentType = typeof(T);
            Command.CommandText = sql;

            try
            {
                OpenConnection();
                using (DbDataReader reader = Command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // The entire EntityGraph is traversed for each record, 
                        // and multiple entities are created from each view record.
                        foreach (EntityGraph lvl in graph)
                        {
                            if (lvl.IsParentReference)
                            {
                                // A child specified a circular reference to its previously loaded parent
                                lvl.AddParentReference();
                            }
                            else if (childrenToLoad.Count > 0 && !lvl.IsRoot && !childrenToLoad.ContainsMember(lvl.Member))
                            {
                                // A list of relationships-to-load was specified and this relationship was not included
                                continue;
                            }
                            else if (lvl.IsNewGroup(reader))
                            {
                                // Create a new entity with the data reader
                                var newEntity = mappingHelper.CreateAndLoadEntity(lvl.EntityType, lvl.Columns, reader, true);

                                // Add entity to the appropriate place in the object graph
                                lvl.AddEntity(newEntity);
                            }
                        }
                    }
                }
            }
            finally
            {
                CloseConnection();
            }

            return (ICollection<T>)graph.RootList;
        }

        #endregion

        #region - Update -

        public UpdateQueryBuilder<T> Update<T>()
        {
            return new UpdateQueryBuilder<T>(this);
        }

        public int Update<T>(T entity, Expression<Func<T, bool>> filter)
        {
            return Update<T>()
                .Entity(entity)
                .Where(filter)
                .Execute();
        }

        public int Update<T>(string tableName, T entity, Expression<Func<T, bool>> filter)
        {
            return Update<T>()
                .TableName(tableName)
                .Entity(entity)
                .Where(filter)
                .Execute();
        }

        public int Update<T>(T entity, string sql)
        {
            return Update<T>()
                .Entity(entity)
                .QueryText(sql)
                .Execute();
        }

        #endregion

        #region - Insert -

        /// <summary>
        /// Creates an InsertQueryBuilder that allows you to build an insert statement.
        /// This method gives you the flexibility to manually configure all options of your insert statement.
        /// Note: You must manually call the Execute() chaining method to run the query.
        /// </summary>
        public InsertQueryBuilder<T> Insert<T>()
        {
            return new InsertQueryBuilder<T>(this);
        }

        /// <summary>
        /// Generates and executes an insert query for the given entity.
        /// This overload will automatically run an identity query if you have mapped an auto-incrementing column,
        /// and if an identity query has been implemented for your current database dialect.
        /// </summary>
        public object Insert<T>(T entity)
        {
            var columns = MapRepository.Instance.GetColumns(typeof(T));
            var dialect = QueryFactory.CreateDialect(this);
            var builder = Insert<T>().Entity(entity);

            // If an auto-increment column exists and this dialect has an identity query...
            if (columns.Exists(c => c.ColumnInfo.IsAutoIncrement) && dialect.HasIdentityQuery)
            {
                builder.GetIdentity();
            }

            return builder.Execute();
        }

        /// <summary>
        /// Generates and executes an insert query for the given entity.
        /// This overload will automatically run an identity query if you have mapped an auto-incrementing column,
        /// and if an identity query has been implemented for your current database dialect.
        /// </summary>
        public object Insert<T>(string tableName, T entity)
        {
            var columns = MapRepository.Instance.GetColumns(typeof(T));
            var dialect = QueryFactory.CreateDialect(this);
            var builder = Insert<T>().Entity(entity).TableName(tableName);

            // If an auto-increment column exists and this dialect has an identity query...
            if (columns.Exists(c => c.ColumnInfo.IsAutoIncrement) && dialect.HasIdentityQuery)
            {
                builder.GetIdentity();
            }

            return builder.Execute();
        }

        /// <summary>
        /// Executes an insert query for the given entity using the given sql insert statement.
        /// This overload will automatically run an identity query if you have mapped an auto-incrementing column,
        /// and if an identity query has been implemented for your current database dialect.
        /// </summary>
        public object Insert<T>(T entity, string sql)
        {
            var columns = MapRepository.Instance.GetColumns(typeof(T));
            var dialect = QueryFactory.CreateDialect(this);
            var builder = Insert<T>().Entity(entity).QueryText(sql);

            // If an auto-increment column exists and this dialect has an identity query...
            if (columns.Exists(c => c.ColumnInfo.IsAutoIncrement) && dialect.HasIdentityQuery)
            {
                builder.GetIdentity();
            }

            return builder.Execute();
        }

        #endregion

        #region - Delete -

        public int Delete<T>(Expression<Func<T, bool>> filter)
        {
            return Delete<T>(null, filter);
        }

        public int Delete<T>(string tableName, Expression<Func<T, bool>> filter)
        {
            // Remember sql mode
            var previousSqlMode = SqlMode;
            SqlMode = SqlModes.Text;

            var mappingHelper = new MappingHelper(this);
            if (tableName == null)
            {
                tableName = MapRepository.Instance.GetTableName(typeof(T));
            }
            var dialect = QueryFactory.CreateDialect(this);
            TableCollection tables = new TableCollection();
            tables.Add(new Table(typeof(T)));
            var where = new WhereBuilder<T>(Command, dialect, filter, tables, false, false);
            IQuery query = QueryFactory.CreateDeleteQuery(dialect, tables[0], where.ToString());
            Command.CommandText = query.Generate();

            int rowsAffected = 0;

            try
            {
                OpenConnection();
                rowsAffected = Command.ExecuteNonQuery();
            }
            finally
            {
                CloseConnection();
            }

            // Return to previous sql mode
            SqlMode = previousSqlMode;

            return rowsAffected;
        }

        #endregion

        #region - Events -

        public event EventHandler OpeningConnection;

        public event EventHandler ClosingConnection;

        #endregion

        #region - Connections / Transactions -

        protected virtual void OnOpeningConnection()
        {
            if (OpeningConnection != null)
                OpeningConnection(this, EventArgs.Empty);
        }

        protected virtual void OnClosingConnection()
        {
            WriteToTraceLog();

            if (ClosingConnection != null)
                ClosingConnection(this, EventArgs.Empty);
        }

        protected internal void OpenConnection()
        {
            OnOpeningConnection();

            if (Command.Connection.State != ConnectionState.Open)
                Command.Connection.Open();
        }

        protected internal void CloseConnection()
        {
            OnClosingConnection();

            Command.Parameters.Clear();
            Command.CommandText = string.Empty;

            if (Command.Transaction == null)
                Command.Connection.Close(); // Only close if no transaction is present

            UnbindEvents();
        }

        private void WriteToTraceLog()
        {
            if (MapRepository.Instance.EnableTraceLogging)
            {
                var sb = new StringBuilder();
                sb.AppendLine();
                sb.AppendLine("==== Begin Query Trace ====");
                sb.AppendLine();
                sb.AppendLine("QUERY TYPE:");
                sb.AppendLine(Command.CommandType.ToString());
                sb.AppendLine();
                sb.AppendLine("QUERY TEXT:");
                sb.AppendLine(Command.CommandText);
                sb.AppendLine();
                sb.AppendLine("PARAMETERS:");
                foreach (IDbDataParameter p in Parameters)
                {
                    object val = (p.Value != null && p.Value is string) ? string.Format("\"{0}\"", p.Value) : p.Value;
                    sb.AppendFormat("{0} = [{1}]", p.ParameterName, val ?? "NULL").AppendLine();
                }
                sb.AppendLine();
                sb.AppendLine("==== End Query Trace ====");
                sb.AppendLine();

                Trace.Write(sb.ToString());
            }
        }

        private void UnbindEvents()
        {
            OpeningConnection = null;
            ClosingConnection = null;
        }

        public void BeginTransaction(IsolationLevel isolationLevel)
        {
            OpenConnection();
            DbTransaction trans = Command.Connection.BeginTransaction(isolationLevel);
            Command.Transaction = trans;
        }

        public void RollBack()
        {
            try
            {
                if (Command.Transaction != null)
                    Command.Transaction.Rollback();
            }
            finally
            {
                Command.Connection.Close();
            }
        }

        public void Commit()
        {
            try
            {
                if (Command.Transaction != null)
                    Command.Transaction.Commit();
            }
            finally
            {
                Command.Connection.Close();
            }
        }

        #endregion

        #region - IDisposable Members -

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); // In case a derived class implements a finalizer
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Command.Transaction != null)
                {
                    Command.Transaction.Dispose();
                    Command.Transaction = null;
                }

                if (Command.Connection != null)
                {
                    Command.Connection.Dispose();
                    Command.Connection = null;
                }

                if (Command != null)
                {
                    Command.Dispose();
                    _command = null;
                }
            }
        }

        #endregion

    }
}
