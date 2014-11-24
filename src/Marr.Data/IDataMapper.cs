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
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using Marr.Data.Parameters;
using System.Linq.Expressions;
using Marr.Data.QGen;

namespace Marr.Data
{
    public interface IDataMapper : IDisposable
    {
        #region - Contructor, Members -

        string ConnectionString { get; }
        DbProviderFactory ProviderFactory { get; }
        DbCommand Command { get; }

        /// <summary>
        /// Gets or sets a value that determines whether the DataMapper will 
        /// use a stored procedure or a sql text command to access 
        /// the database.  The default is stored procedure.
        /// </summary>
        SqlModes SqlMode { get; set; }        

        #endregion

        #region - Update -

        UpdateQueryBuilder<T> Update<T>();
        int Update<T>(T entity, Expression<Func<T, bool>> filter);
        int Update<T>(string tableName, T entity, Expression<Func<T, bool>> filter);
        int Update<T>(T entity, string sql);

        #endregion

        #region - Insert -

        /// <summary>
        /// Creates an InsertQueryBuilder that allows you to build an insert statement.
        /// This method gives you the flexibility to manually configure all options of your insert statement.
        /// Note: You must manually call the Execute() chaining method to run the query.
        /// </summary>
        InsertQueryBuilder<T> Insert<T>();

        /// <summary>
        /// Generates and executes an insert query for the given entity.
        /// This overload will automatically run an identity query if you have mapped an auto-incrementing column,
        /// and if an identity query has been implemented for your current database dialect.
        /// </summary>
        object Insert<T>(T entity);

        /// <summary>
        /// Generates and executes an insert query for the given entity.
        /// This overload will automatically run an identity query if you have mapped an auto-incrementing column,
        /// and if an identity query has been implemented for your current database dialect.
        /// </summary>
        object Insert<T>(string tableName, T entity);

        /// <summary>
        /// Executes an insert query for the given entity using the given sql insert statement.
        /// This overload will automatically run an identity query if you have mapped an auto-incrementing column,
        /// and if an identity query has been implemented for your current database dialect.
        /// </summary>
        object Insert<T>(T entity, string sql);

        #endregion

        #region - Delete -

        int Delete<T>(Expression<Func<T, bool>> filter);
        int Delete<T>(string tableName, Expression<Func<T, bool>> filter);

        #endregion

        #region - Connections / Transactions -

        void BeginTransaction(IsolationLevel isolationLevel);
        void RollBack();
        void Commit();
        event EventHandler OpeningConnection;

        #endregion

        #region - ExecuteScalar, ExecuteNonQuery, ExecuteReader -

        /// <summary>
        /// Executes a non query that returns an integer.
        /// </summary>
        /// <param name="sql">The SQL command to execute.</param>
        /// <returns>An integer value</returns>
        int ExecuteNonQuery(string sql);

        /// <summary>
        /// Executes a stored procedure that returns a scalar value.
        /// </summary>
        /// <param name="sql">The SQL command to execute.</param>
        /// <returns>A scalar value</returns>
        object ExecuteScalar(string sql);

        /// <summary>
        /// Executes a DataReader that can be controlled using a Func delegate.
        /// (Note that reader.Read() will be called automatically).
        /// </summary>
        /// <typeparam name="TResult">The type that will be return in the result set.</typeparam>
        /// <param name="sql">The sql statement that will be executed.</param>
        /// <param name="func">The function that will build the the TResult set.</param>
        /// <returns>An IEnumerable of TResult.</returns>
        IEnumerable<TResult> ExecuteReader<TResult>(string sql, Func<DbDataReader, TResult> func);

        /// <summary>
        /// Executes a DataReader that can be controlled using an Action delegate.
        /// </summary>
        /// <param name="sql">The sql statement that will be executed.</param>
        /// <param name="action">The delegate that will work with the result set.</param>
        void ExecuteReader(string sql, Action<DbDataReader> action);

        #endregion

        #region - DataSets -

        DataSet GetDataSet(string sql);
        DataSet GetDataSet(string sql, DataSet ds, string tableName);
        DataTable GetDataTable(string sql, DataTable dt, string tableName);
        DataTable GetDataTable(string sql);
        int InsertDataTable(DataTable table, string insertSP);
        int InsertDataTable(DataTable table, string insertSP, UpdateRowSource updateRowSource);
        int UpdateDataSet(DataSet ds, string updateSP);
        int DeleteDataTable(DataTable dt, string deleteSP);

        #endregion

        #region - Parameters -

        DbParameterCollection Parameters { get; }
        ParameterChainMethods AddParameter(string name, object value);
        IDbDataParameter AddParameter(IDbDataParameter parameter);

        #endregion

        #region - Find -

        /// <summary>
        /// Returns an entity of type T.
        /// </summary>
        /// <typeparam name="T">The type of entity that is to be instantiated and loaded with values.</typeparam>
        /// <param name="sql">The SQL command to execute.</param>
        /// <returns>An instantiated and loaded entity of type T.</returns>
        T Find<T>(string sql);

        /// <summary>
        /// Returns an entity of type T.
        /// </summary>
        /// <typeparam name="T">The type of entity that is to be instantiated and loaded with values.</typeparam>
        /// <param name="sql">The SQL command to execute.</param>
        /// <param name="ent">A previously instantiated entity that will be loaded with values.</param>
        /// <returns>An instantiated and loaded entity of type T.</returns>
        T Find<T>(string sql, T ent);

        #endregion

        #region - Query -

        /// <summary>
        /// Creates a QueryBuilder that allows you to build a query.
        /// </summary>
        /// <typeparam name="T">The type of object that will be queried.</typeparam>
        /// <returns>Returns a QueryBuilder of T.</returns>
        QueryBuilder<T> Query<T>();

        /// <summary>
        /// Returns the results of a query.
        /// Uses a List of type T to return the data.
        /// </summary>
        /// <typeparam name="T">The type of object that will be queried.</typeparam>
        /// <returns>Returns a list of the specified type.</returns>
        List<T> Query<T>(string sql);

        /// <summary>
        /// Returns the results of a query or a stored procedure.
        /// </summary>
        /// <typeparam name="T">The type of object that will be queried.</typeparam>
        /// <param name="sql">The sql query or stored procedure name to run.</param>
        /// <param name="entityList">A previously instantiated list to populate.</param>
        /// <returns>Returns a list of the specified type.</returns>
        ICollection<T> Query<T>(string sql, ICollection<T> entityList);

        #endregion

        #region - Query to Graph -

        /// <summary>
        /// Runs a query and then tries to instantiate the entire object graph with entites.
        /// </summary>
        List<T> QueryToGraph<T>(string sql);

        /// <summary>
        /// Runs a query and then tries to instantiate the entire object graph with entites.
        /// </summary>
        ICollection<T> QueryToGraph<T>(string sql, ICollection<T> entityList);

        #endregion
    }
}
