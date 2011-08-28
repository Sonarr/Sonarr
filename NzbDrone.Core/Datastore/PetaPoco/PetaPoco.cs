/* PetaPoco v4.0.3 - A Tiny ORMish thing for your POCO's.
 * Copyright © 2011 Topten Software.  All Rights Reserved.
 * 
 * Apache License 2.0 - http://www.toptensoftware.com/petapoco/license
 * 
 * Special thanks to Rob Conery (@robconery) for original inspiration (ie:Massive) and for 
 * use of Subsonic's T4 templates, Rob Sullivan (@DataChomp) for hard core DBA advice 
 * and Adam Schroder (@schotime) for lots of suggestions, improvements and Oracle support
 */

//#define PETAPOCO_NO_DYNAMIC //in your project settings on .NET 3.5

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data.Common;
using System.Data;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq.Expressions;


namespace PetaPoco
{
    // Poco's marked [Explicit] require all column properties to be marked
    [AttributeUsage(AttributeTargets.Class)]
    public class ExplicitColumnsAttribute : Attribute
    {
    }
    // For non-explicit pocos, causes a property to be ignored
    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreAttribute : Attribute
    {
    }

    // For explicit pocos, marks property as a column
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        public ColumnAttribute() { }
        public ColumnAttribute(string name) { Name = name; }
        public string Name { get; set; }
    }

    // For explicit pocos, marks property as a column
    [AttributeUsage(AttributeTargets.Property)]
    public class ResultColumnAttribute : ColumnAttribute
    {
        public ResultColumnAttribute() { }
        public ResultColumnAttribute(string name) : base(name) { }
    }

    // Specify the table name of a poco
    [AttributeUsage(AttributeTargets.Class)]
    public class TableNameAttribute : Attribute
    {
        public TableNameAttribute(string tableName)
        {
            Value = tableName;
        }
        public string Value { get; private set; }
    }

    // Specific the primary key of a poco class (and optional sequence name for Oracle)
    [AttributeUsage(AttributeTargets.Class)]
    public class PrimaryKeyAttribute : Attribute
    {
        public PrimaryKeyAttribute(string primaryKey)
        {
            Value = primaryKey;
            autoIncrement = true;
        }

        public string Value { get; private set; }
        public string sequenceName { get; set; }
        public bool autoIncrement { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class AutoJoinAttribute : Attribute
    {
        public AutoJoinAttribute() { }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class VersionColumnAttribute : ColumnAttribute
    {
        public VersionColumnAttribute() { }
        public VersionColumnAttribute(string name) : base(name) { }
    }

    // Results from paged request
    public class Page<T>
    {
        public long CurrentPage { get; set; }
        public long TotalPages { get; set; }
        public long TotalItems { get; set; }
        public long ItemsPerPage { get; set; }
        public List<T> Items { get; set; }
        public object Context { get; set; }
    }

    // Pass as parameter value to force to DBType.AnsiString
    public class AnsiString
    {
        public AnsiString(string str)
        {
            Value = str;
        }
        public string Value { get; private set; }
    }

    // Used by IMapper to override table bindings for an object
    public class TableInfo
    {
        public string TableName { get; set; }
        public string PrimaryKey { get; set; }
        public bool AutoIncrement { get; set; }
        public string SequenceName { get; set; }
    }

    // Optionally provide an implementation of this to Database.Mapper
    public interface IMapper
    {
        void GetTableInfo(Type t, TableInfo ti);
        bool MapPropertyToColumn(PropertyInfo pi, ref string columnName, ref bool resultColumn);
        Func<object, object> GetFromDbConverter(PropertyInfo pi, Type SourceType);
        Func<object, object> GetToDbConverter(Type SourceType);
    }

    // This will be merged with IMapper in the next major version
    public interface IMapper2 : IMapper
    {
        Func<object, object> GetFromDbConverter(Type DestType, Type SourceType);
    }

    public class DefaultMapper : IMapper2
    {
        public virtual void GetTableInfo(Type t, TableInfo ti) { }
        public virtual bool MapPropertyToColumn(PropertyInfo pi, ref string columnName, ref bool resultColumn)
        {
            return true;
        }
        public virtual Func<object, object> GetFromDbConverter(PropertyInfo pi, Type SourceType)
        {
            return null;
        }
        public virtual Func<object, object> GetToDbConverter(Type SourceType)
        {
            return null;
        }
        public virtual Func<object, object> GetFromDbConverter(Type DestType, Type SourceType)
        {
            return null;
        }
    }

    //public class DestinationInfo
    //{
    //    public DestinationInfo(Type type)
    //    {
    //        Type = type;
    //    }

    //    public DestinationInfo(PropertyInfo propertyInfo)
    //    {
    //        PropertyInfo = propertyInfo;
    //        Type = propertyInfo.PropertyType;
    //    }

    //    public PropertyInfo PropertyInfo { get; private set; }
    //    public Type Type { get; private set; }
    //}

    public interface IDatabaseQuery
    {
        void OpenSharedConnection();
        void CloseSharedConnection();
        int Execute(string sql, params object[] args);
        int Execute(Sql sql);
        T ExecuteScalar<T>(string sql, params object[] args);
        T ExecuteScalar<T>(Sql sql);
        List<T> Fetch<T>();
        List<T> Fetch<T>(string sql, params object[] args);
        List<T> Fetch<T>(Sql sql);
        List<T> Fetch<T>(long page, long itemsPerPage, string sql, params object[] args);
        List<T> Fetch<T>(long page, long itemsPerPage, Sql sql);
        Page<T> Page<T>(long page, long itemsPerPage, string sql, params object[] args);
        Page<T> Page<T>(long page, long itemsPerPage, Sql sql);
        List<T> SkipTake<T>(long skip, long take, string sql, params object[] args);
        List<T> SkipTake<T>(long skip, long take, Sql sql);
        List<TRet> Fetch<T1, T2, TRet>(Func<T1, T2, TRet> cb, string sql, params object[] args);
        List<TRet> Fetch<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> cb, string sql, params object[] args);
        List<TRet> Fetch<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> cb, string sql, params object[] args);
        IEnumerable<TRet> Query<T1, T2, TRet>(Func<T1, T2, TRet> cb, string sql, params object[] args);
        IEnumerable<TRet> Query<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> cb, string sql, params object[] args);
        IEnumerable<TRet> Query<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> cb, string sql, params object[] args);
        List<TRet> Fetch<T1, T2, TRet>(Func<T1, T2, TRet> cb, Sql sql);
        List<TRet> Fetch<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> cb, Sql sql);
        List<TRet> Fetch<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> cb, Sql sql);
        IEnumerable<TRet> Query<T1, T2, TRet>(Func<T1, T2, TRet> cb, Sql sql);
        IEnumerable<TRet> Query<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> cb, Sql sql);
        IEnumerable<TRet> Query<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> cb, Sql sql);
        List<T1> Fetch<T1, T2>(string sql, params object[] args);
        List<T1> Fetch<T1, T2, T3>(string sql, params object[] args);
        List<T1> Fetch<T1, T2, T3, T4>(string sql, params object[] args);
        IEnumerable<T1> Query<T1, T2>(string sql, params object[] args);
        IEnumerable<T1> Query<T1, T2, T3>(string sql, params object[] args);
        IEnumerable<T1> Query<T1, T2, T3, T4>(string sql, params object[] args);
        IEnumerable<TRet> Query<TRet>(Type[] types, object cb, string sql, params object[] args);
        List<T1> Fetch<T1, T2>(Sql sql);
        List<T1> Fetch<T1, T2, T3>(Sql sql);
        List<T1> Fetch<T1, T2, T3, T4>(Sql sql);
        IEnumerable<T1> Query<T1, T2>(Sql sql);
        IEnumerable<T1> Query<T1, T2, T3>(Sql sql);
        IEnumerable<T1> Query<T1, T2, T3, T4>(Sql sql);
        IEnumerable<T> Query<T>(string sql, params object[] args);
        IEnumerable<T> Query<T>(Sql sql);
        T Single<T>(object primaryKey);
        T SingleOrDefault<T>(object primaryKey);
        T Single<T>(string sql, params object[] args);
        T SingleOrDefault<T>(string sql, params object[] args);
        T First<T>(string sql, params object[] args);
        T FirstOrDefault<T>(string sql, params object[] args);
        T Single<T>(Sql sql);
        T SingleOrDefault<T>(Sql sql);
        T First<T>(Sql sql);
        T FirstOrDefault<T>(Sql sql);
        bool Exists<T>(object primaryKey);
        int OneTimeCommandTimeout { get; set; }
        bool Exists<T>(string sql, params object[] args);
    }

    public interface IDatabase : IDatabaseQuery
    {
        void Dispose();
        IDbConnection Connection { get; }
        ITransaction GetTransaction();
        Transaction GetTransaction(IsolationLevel? isolationLevel);
        void BeginTransaction();
        void BeginTransaction(IsolationLevel? isolationLevel);
        void AbortTransaction();
        void CompleteTransaction();
        object Insert(string tableName, string primaryKeyName, bool autoIncrement, object poco);
        object Insert(string tableName, string primaryKeyName, object poco);
        object Insert(object poco);
        int Update(string tableName, string primaryKeyName, object poco, object primaryKeyValue);
        int Update(string tableName, string primaryKeyName, object poco);
        int Update(string tableName, string primaryKeyName, object poco, object primaryKeyValue, IEnumerable<string> columns);
        int Update(string tableName, string primaryKeyName, object poco, IEnumerable<string> columns);
        int Update(object poco, IEnumerable<string> columns);
        int Update(object poco, object primaryKeyValue, IEnumerable<string> columns);
        int Update(object poco);
        int Update(object poco, object primaryKeyValue);
        int Update<T>(string sql, params object[] args);
        int Update<T>(Sql sql);
        void UpdateMany<T>(IEnumerable<T> pocoList);
        int Delete(string tableName, string primaryKeyName, object poco);
        int Delete(string tableName, string primaryKeyName, object poco, object primaryKeyValue);
        int Delete(object poco);
        int Delete<T>(string sql, params object[] args);
        int Delete<T>(Sql sql);
        int Delete<T>(object pocoOrPrimaryKey);
        void Save(string tableName, string primaryKeyName, object poco);
        void Save(object poco);
        void InsertMany<T>(IEnumerable<T> pocoList);
        void SaveMany<T>(IEnumerable<T> pocoList);
    }

    // Database class ... this is where most of the action happens
    public class Database : IDisposable, IDatabase
    {
        public const string MsSqlClientProvider = "System.Data.SqlClient";

        public Database(IDbConnection connection) : this(connection, DBType.NotSet) { }

        public Database(IDbConnection connection, DBType dbType)
        {
            _sharedConnection = connection;
            _connectionString = connection.ConnectionString;

            _dbType = dbType;
            CommonConstruct();
        }

        public Database(string connectionString, string providerName)
        {
            _connectionString = connectionString;
            _providerName = providerName;
            CommonConstruct();
        }

        public Database(string connectionString, DbProviderFactory provider, DBType dbType)
        {
            _connectionString = connectionString;
            _factory = provider;
            _dbType = dbType;
            CommonConstruct();
        }

        public Database(string connectionStringName)
        {
            // Use first?
            if (connectionStringName == "")
                connectionStringName = ConfigurationManager.ConnectionStrings[0].Name;

            // Work out connection string and provider name
            var providerName = "System.Data.SqlClient";
            if (ConfigurationManager.ConnectionStrings[connectionStringName] != null)
            {
                if (!string.IsNullOrEmpty(ConfigurationManager.ConnectionStrings[connectionStringName].ProviderName))
                    providerName = ConfigurationManager.ConnectionStrings[connectionStringName].ProviderName;
            }
            else
            {
                throw new InvalidOperationException("Can't find a connection string with the name '" + connectionStringName + "'");
            }

            // Store factory and connection string
            _connectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
            _providerName = providerName;
            CommonConstruct();
        }

        public enum DBType
        {
            NotSet,
            SqlServer,
            SqlServerCE,
            MySql,
            PostgreSQL,
            Oracle,
            SQLite
        }

        private DBType _dbType = DBType.NotSet;

        // Common initialization
        private void CommonConstruct()
        {
            _transactionDepth = 0;
            ForceDateTimesToUtc = true;
            EnableAutoSelect = true;

            if (_providerName != null)
                _factory = DbProviderFactories.GetFactory(_providerName);

            if (_dbType == DBType.NotSet)
            {
                _dbType = DBType.SqlServer;
                string dbtype = (_factory == null ? _sharedConnection.GetType() : _factory.GetType()).Name;
                if (dbtype.StartsWith("MySql"))
                    _dbType = DBType.MySql;
                else if (dbtype.StartsWith("SqlCe"))
                    _dbType = DBType.SqlServerCE;
                else if (dbtype.StartsWith("Npgsql"))
                    _dbType = DBType.PostgreSQL;
                else if (dbtype.StartsWith("Oracle"))
                    _dbType = DBType.Oracle;
                else if (dbtype.StartsWith("SQLite"))
                    _dbType = DBType.SQLite;
            }

            if (_dbType == DBType.MySql && _connectionString != null && _connectionString.IndexOf("Allow User Variables=true") >= 0)
                _paramPrefix = "?";
            if (_dbType == DBType.Oracle)
                _paramPrefix = ":";
        }

        // Automatically close one open shared connection
        public void Dispose()
        {
            // Automatically close one open connection reference
            //  (Works with KeepConnectionAlive and manually opening a shared connection)
            KeepConnectionAlive = false;
            CloseSharedConnection();
        }

        // Set to true to keep the first opened connection alive until this object is disposed
        public bool KeepConnectionAlive { get; set; }

        // Open a connection (can be nested)
        public void OpenSharedConnection()
        {
            if (_sharedConnection == null || _sharedConnection.State == ConnectionState.Closed || _sharedConnection.State == ConnectionState.Broken)
            {
                _sharedConnection = _factory.CreateConnection();
                _sharedConnection.ConnectionString = _connectionString;
                _sharedConnection.Open();

                _sharedConnection = OnConnectionOpened(_sharedConnection);
            }
        }

        /// <summary>
        /// Close a previously opened connection
        /// </summary>
        // Close a previously opened connection
        public void CloseSharedConnection()
        {
            if (!KeepConnectionAlive && _sharedConnection != null)
            {
                OnConnectionClosing(_sharedConnection);
                _sharedConnection.Dispose();
                _sharedConnection = null;
            }
        }

        // Access to our shared connection
        public IDbConnection Connection
        {
            get { return _sharedConnection; }
        }

        // Helper to create a transaction scope
        public ITransaction GetTransaction()
        {
            return GetTransaction(null);
        }

        public Transaction GetTransaction(IsolationLevel? isolationLevel)
        {
            return new Transaction(this, isolationLevel);
        }

        // Use by derived repo generated by T4 templates
        public virtual void OnBeginTransaction() { }
        public virtual void OnEndTransaction() { }

        public void BeginTransaction()
        {
            BeginTransaction(null);
        }

        // Start a new transaction, can be nested, every call must be
        //	matched by a call to AbortTransaction or CompleteTransaction
        // Use `using (var scope=db.Transaction) { scope.Complete(); }` to ensure correct semantics
        public void BeginTransaction(IsolationLevel? isolationLevel)
        {
            _transactionDepth++;

            if (_transactionDepth == 1)
            {
                OpenSharedConnection();
                _transaction = isolationLevel == null ? _sharedConnection.BeginTransaction() : _sharedConnection.BeginTransaction(isolationLevel.Value);
                _transactionCancelled = false;
                OnBeginTransaction();
            }

        }

        // Internal helper to cleanup transaction stuff
        void CleanupTransaction()
        {
            OnEndTransaction();

            if (_transactionCancelled)
                _transaction.Rollback();
            else
                _transaction.Commit();

            _transaction.Dispose();
            _transaction = null;

            CloseSharedConnection();
        }

        // Abort the entire outer most transaction scope
        public void AbortTransaction()
        {
            _transactionCancelled = true;
            if ((--_transactionDepth) == 0)
                CleanupTransaction();
        }

        // Complete the transaction
        public void CompleteTransaction()
        {
            if ((--_transactionDepth) == 0)
                CleanupTransaction();
        }

        // Helper to handle named parameters from object properties
        static Regex rxParams = new Regex(@"(?<!@)@\w+", RegexOptions.Compiled);
        public static string ProcessParams(string _sql, object[] args_src, List<object> args_dest)
        {
            return rxParams.Replace(_sql, m =>
            {
                string param = m.Value.Substring(1);

                object arg_val;

                int paramIndex;
                if (int.TryParse(param, out paramIndex))
                {
                    // Numbered parameter
                    if (paramIndex < 0 || paramIndex >= args_src.Length)
                        throw new ArgumentOutOfRangeException(string.Format("Parameter '@{0}' specified but only {1} parameters supplied (in `{2}`)", paramIndex, args_src.Length, _sql));
                    arg_val = args_src[paramIndex];
                }
                else
                {
                    // Look for a property on one of the arguments with this name
                    bool found = false;
                    arg_val = null;
                    foreach (var o in args_src)
                    {
                        var pi = o.GetType().GetProperty(param);
                        if (pi != null)
                        {
                            arg_val = pi.GetValue(o, null);
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                        throw new ArgumentException(string.Format("Parameter '@{0}' specified but none of the passed arguments have a property with this name (in '{1}')", param, _sql));
                }

                // Expand collections to parameter lists
                if ((arg_val as System.Collections.IEnumerable) != null &&
                    (arg_val as string) == null &&
                    (arg_val as byte[]) == null)
                {
                    var sb = new StringBuilder();
                    foreach (var i in arg_val as System.Collections.IEnumerable)
                    {
                        var indexOfExistingValue = args_dest.IndexOf(i);
                        if (indexOfExistingValue >= 0)
                        {
                            sb.Append((sb.Length == 0 ? "@" : ",@") + indexOfExistingValue);
                        }
                        else
                        {
                            sb.Append((sb.Length == 0 ? "@" : ",@") + args_dest.Count);
                            args_dest.Add(i);
                        }
                    }
                    return sb.ToString();
                }
                else
                {
                    var indexOfExistingValue = args_dest.IndexOf(arg_val);
                    if (indexOfExistingValue >= 0)
                        return "@" + indexOfExistingValue;

                    args_dest.Add(arg_val);
                    return "@" + (args_dest.Count - 1).ToString();
                }
            }
            );
        }

        // Add a parameter to a DB command
        void AddParam(IDbCommand cmd, object item, string ParameterPrefix)
        {
            // Convert value to from poco type to db type
            if (Database.Mapper != null && item != null)
            {
                var fn = Database.Mapper.GetToDbConverter(item.GetType());
                if (fn != null)
                    item = fn(item);
            }

            // Support passed in parameters
            var idbParam = item as IDbDataParameter;
            if (idbParam != null)
            {
                idbParam.ParameterName = string.Format("{0}{1}", ParameterPrefix, cmd.Parameters.Count);
                cmd.Parameters.Add(idbParam);
                return;
            }
            var p = cmd.CreateParameter();
            p.ParameterName = string.Format("{0}{1}", ParameterPrefix, cmd.Parameters.Count);

            if (item == null)
            {
                p.Value = DBNull.Value;
            }
            else
            {
                var t = item.GetType();
                if (t.IsEnum)		// PostgreSQL .NET driver wont cast enum to int
                {
                    p.Value = (int)item;
                }
                else if (t == typeof(Guid))
                {
                    p.Value = item.ToString();
                    p.DbType = DbType.String;
                    p.Size = 40;
                }
                else if (t == typeof(string))
                {
                    p.Size = Math.Max((item as string).Length + 1, 4000);		// Help query plan caching by using common size
                    p.Value = item;
                }
                else if (t == typeof(AnsiString))
                {
                    // Thanks @DataChomp for pointing out the SQL Server indexing performance hit of using wrong string type on varchar
                    p.Size = Math.Max((item as AnsiString).Value.Length + 1, 4000);
                    p.Value = (item as AnsiString).Value;
                    p.DbType = DbType.AnsiString;
                }
                else if (t == typeof(bool) && _dbType != DBType.PostgreSQL)
                {
                    p.Value = ((bool)item) ? 1 : 0;
                }
                else if (item.GetType().Name == "SqlGeography") //SqlGeography is a CLR Type
                {
                    p.GetType().GetProperty("UdtTypeName").SetValue(p, "geography", null); //geography is the equivalent SQL Server Type
                    p.Value = item;
                }

                else if (item.GetType().Name == "SqlGeometry") //SqlGeometry is a CLR Type
                {
                    p.GetType().GetProperty("UdtTypeName").SetValue(p, "geometry", null); //geography is the equivalent SQL Server Type
                    p.Value = item;
                }
                else
                {
                    p.Value = item;
                }
            }

            cmd.Parameters.Add(p);
        }

        // Create a command
        static Regex rxParamsPrefix = new Regex(@"(?<!@)@\w+", RegexOptions.Compiled);
        IDbCommand CreateCommand(IDbConnection connection, string sql, params object[] args)
        {
            // Perform parameter prefix replacements
            if (_paramPrefix != "@")
                sql = rxParamsPrefix.Replace(sql, m => _paramPrefix + m.Value.Substring(1));
            sql = sql.Replace("@@", "@");		   // <- double @@ escapes a single @

            // Create the command and add parameters
            IDbCommand cmd = connection.CreateCommand();
            cmd.Connection = connection;
            cmd.CommandText = sql;
            cmd.Transaction = _transaction;

            foreach (var item in args)
            {
                AddParam(cmd, item, _paramPrefix);
            }

            if (_dbType == DBType.Oracle)
            {
                cmd.GetType().GetProperty("BindByName").SetValue(cmd, true, null);
            }

            if (!String.IsNullOrEmpty(sql))
                DoPreExecute(cmd);

            return cmd;
        }

        // Create a command
        //IDbCommand CreateCommand(IDbConnection connection, string sql, params object[] args)
        //{
        //    var sqlStatement = new Sql(sql, args);
        //    return CreateCommand(connection, sqlStatement);
        //}

        // Override this to log/capture exceptions
        public virtual void OnException(Exception x)
        {
            System.Diagnostics.Debug.WriteLine(x.ToString());
            System.Diagnostics.Debug.WriteLine(LastCommand);
        }

        // Override this to log commands, or modify command before execution
        public virtual IDbConnection OnConnectionOpened(IDbConnection conn) { return conn; }
        public virtual void OnConnectionClosing(IDbConnection conn) { }
        public virtual void OnExecutingCommand(IDbCommand cmd) { }
        public virtual void OnExecutedCommand(IDbCommand cmd) { }

        // Execute a non-query command
        public int Execute(string sql, params object[] args)
        {
            return Execute(new Sql(sql, args));
        }

        public int Execute(Sql Sql)
        {
            var sql = Sql.SQL;
            var args = Sql.Arguments;

            try
            {
                OpenSharedConnection();
                try
                {
                    using (var cmd = CreateCommand(_sharedConnection, sql, args))
                    {
                        var result = cmd.ExecuteNonQuery();
                        OnExecutedCommand(cmd);
                        return result;
                    }
                }
                finally
                {
                    CloseSharedConnection();
                }
            }
            catch (Exception x)
            {
                OnException(x);
                throw;
            }
        }

        // Execute and cast a scalar property
        public T ExecuteScalar<T>(string sql, params object[] args)
        {
            return ExecuteScalar<T>(new Sql(sql, args));
        }

        public T ExecuteScalar<T>(Sql Sql)
        {
            var sql = Sql.SQL;
            var args = Sql.Arguments;

            try
            {
                OpenSharedConnection();
                try
                {
                    using (var cmd = CreateCommand(_sharedConnection, sql, args))
                    {
                        object val = cmd.ExecuteScalar();
                        OnExecutedCommand(cmd);
                        return (T)Convert.ChangeType(val, typeof(T));
                    }
                }
                finally
                {
                    CloseSharedConnection();
                }
            }
            catch (Exception x)
            {
                OnException(x);
                throw;
            }
        }

        static Regex rxSelect = new Regex(@"\A\s*(SELECT|EXECUTE|CALL)\s", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Multiline);
        static Regex rxFrom = new Regex(@"\A\s*FROM\s", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Multiline);
        string AddSelectClause<T>(string sql)
        {
            if (sql.StartsWith(";"))
                return sql.Substring(1);

            if (!rxSelect.IsMatch(sql))
            {
                var pd = PocoData.ForType(typeof(T));
                var tableName = EscapeTableName(pd.TableInfo.TableName);
                string cols = string.Join(", ", (from c in pd.QueryColumns select EscapeSqlIdentifier(c)).ToArray());
                if (!rxFrom.IsMatch(sql))
                    sql = string.Format("SELECT {0} FROM {1} {2}", cols, tableName, sql);
                else
                    sql = string.Format("SELECT {0} {1}", cols, sql);
            }
            return sql;
        }

        public bool ForceDateTimesToUtc { get; set; }
        public bool EnableAutoSelect { get; set; }

        // Return a typed list of pocos
        public List<T> Fetch<T>(string sql, params object[] args)
        {
            return Fetch<T>(new Sql(sql, args));
        }

        public List<T> Fetch<T>(Sql sql)
        {
            return Query<T>(sql).ToList();
        }

        public List<T> Fetch<T>()
        {
            return Fetch<T>("");
        }

        static Regex rxColumns = new Regex(@"\A\s*SELECT\s+((?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|.)*?)(?<!,\s+)\bFROM\b", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);
        static Regex rxOrderBy = new Regex(@"\bORDER\s+BY\s+(?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|[\w\(\)\.])+(?:\s+(?:ASC|DESC))?(?:\s*,\s*(?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|[\w\(\)\.])+(?:\s+(?:ASC|DESC))?)*", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);
        static Regex rxDistinct = new Regex(@"\ADISTINCT\s", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);
        public static bool SplitSqlForPaging(string sql, out string sqlCount, out string sqlSelectRemoved, out string sqlOrderBy)
        {
            sqlSelectRemoved = null;
            sqlCount = null;
            sqlOrderBy = null;

            // Extract the columns from "SELECT <whatever> FROM"
            var m = rxColumns.Match(sql);
            if (!m.Success)
                return false;

            // Save column list and replace with COUNT(*)
            Group g = m.Groups[1];
            sqlSelectRemoved = sql.Substring(g.Index);

            if (rxDistinct.IsMatch(sqlSelectRemoved))
                sqlCount = sql.Substring(0, g.Index) + "COUNT(" + m.Groups[1].ToString().Trim() + ") " + sql.Substring(g.Index + g.Length);
            else
                sqlCount = sql.Substring(0, g.Index) + "COUNT(*) " + sql.Substring(g.Index + g.Length);


            // Look for an "ORDER BY <whatever>" clause
            m = rxOrderBy.Match(sqlCount);
            if (m.Success)
            {
                g = m.Groups[0];
                sqlOrderBy = g.ToString();
                sqlCount = sqlCount.Substring(0, g.Index) + sqlCount.Substring(g.Index + g.Length);
            }

            return true;
        }

        public void BuildPageQueries<T>(long skip, long take, string sql, ref object[] args, out string sqlCount, out string sqlPage)
        {
            // Add auto select clause
            sql = AddSelectClause<T>(sql);

            // Split the SQL into the bits we need
            string sqlSelectRemoved, sqlOrderBy;
            if (!SplitSqlForPaging(sql, out sqlCount, out sqlSelectRemoved, out sqlOrderBy))
                throw new Exception("Unable to parse SQL statement for paged query");
            if (_dbType == DBType.Oracle && sqlSelectRemoved.StartsWith("*"))
                throw new Exception("Query must alias '*' when performing a paged query.\neg. select t.* from table t order by t.id");

            // Build the SQL for the actual final result
            if (_dbType == DBType.SqlServer || _dbType == DBType.Oracle)
            {
                sqlSelectRemoved = rxOrderBy.Replace(sqlSelectRemoved, "");
                if (rxDistinct.IsMatch(sqlSelectRemoved))
                {
                    sqlSelectRemoved = "peta_inner.* FROM (SELECT " + sqlSelectRemoved + ") peta_inner";
                }
                sqlPage = string.Format("SELECT * FROM (SELECT ROW_NUMBER() OVER ({0}) peta_rn, {1}) peta_paged WHERE peta_rn>@{2} AND peta_rn<=@{3}",
                                        sqlOrderBy == null ? "ORDER BY (SELECT NULL)" : sqlOrderBy, sqlSelectRemoved, args.Length, args.Length + 1);
                args = args.Concat(new object[] { skip, skip + take }).ToArray();
            }
            else if (_dbType == DBType.SqlServerCE)
            {
                sqlPage = string.Format("{0}\nOFFSET @{1} ROWS FETCH NEXT @{2} ROWS ONLY", sql, args.Length, args.Length + 1);
                args = args.Concat(new object[] { skip, take }).ToArray();
            }
            else
            {
                sqlPage = string.Format("{0}\nLIMIT @{1} OFFSET @{2}", sql, args.Length, args.Length + 1);
                args = args.Concat(new object[] { take, skip }).ToArray();
            }

        }

        // Fetch a page	
        public Page<T> Page<T>(long page, long itemsPerPage, string sql, params object[] args)
        {
            string sqlCount, sqlPage;
            BuildPageQueries<T>((page - 1) * itemsPerPage, itemsPerPage, sql, ref args, out sqlCount, out sqlPage);

            // Save the one-time command time out and use it for both queries
            int saveTimeout = OneTimeCommandTimeout;

            // Setup the paged result
            var result = new Page<T>();
            result.CurrentPage = page;
            result.ItemsPerPage = itemsPerPage;
            result.TotalItems = ExecuteScalar<long>(sqlCount, args);
            result.TotalPages = result.TotalItems / itemsPerPage;
            if ((result.TotalItems % itemsPerPage) != 0)
                result.TotalPages++;

            OneTimeCommandTimeout = saveTimeout;

            // Get the records
            result.Items = Fetch<T>(sqlPage, args);

            // Done
            return result;
        }

        public Page<T> Page<T>(long page, long itemsPerPage, Sql sql)
        {
            return Page<T>(page, itemsPerPage, sql.SQL, sql.Arguments);
        }

        public List<T> Fetch<T>(long page, long itemsPerPage, string sql, params object[] args)
        {
            return SkipTake<T>((page - 1) * itemsPerPage, itemsPerPage, sql, args);
        }

        public List<T> Fetch<T>(long page, long itemsPerPage, Sql sql)
        {
            return SkipTake<T>((page - 1) * itemsPerPage, itemsPerPage, sql.SQL, sql.Arguments);
        }

        public List<T> SkipTake<T>(long skip, long take, string sql, params object[] args)
        {
            string sqlCount, sqlPage;
            BuildPageQueries<T>(skip, take, sql, ref args, out sqlCount, out sqlPage);
            return Fetch<T>(sqlPage, args);
        }

        public List<T> SkipTake<T>(long skip, long take, Sql sql)
        {
            return SkipTake<T>(skip, take, sql.SQL, sql.Arguments);
        }

        public Dictionary<TKey, TValue> Dictionary<TKey, TValue>(Sql Sql)
        {
            return Dictionary<TKey, TValue>(Sql.SQL, Sql.Arguments);
        }

        public Dictionary<TKey, TValue> Dictionary<TKey, TValue>(string sql, params object[] args)
        {
            var newDict = new Dictionary<TKey, TValue>();
            bool isConverterSet = false;
            Func<object, object> converter1 = x => x, converter2 = x => x;

            foreach (var line in Query<Dictionary<string, object>>(sql, args))
            {
                object key = line.ElementAt(0).Value;
                object value = line.ElementAt(1).Value;

                if (isConverterSet == false)
                {
                    converter1 = PocoData.GetConverter(ForceDateTimesToUtc, null, typeof(TKey), key.GetType()) ?? (x => x);
                    converter2 = PocoData.GetConverter(ForceDateTimesToUtc, null, typeof(TValue), value.GetType()) ?? (x => x);
                    isConverterSet = true;
                }

                var keyConverted = (TKey)Convert.ChangeType(converter1(key), typeof(TKey));

                var valueType = Nullable.GetUnderlyingType(typeof(TValue)) ?? typeof(TValue);
                var valConv = converter2(value);
                var valConverted = valConv != null ? (TValue)Convert.ChangeType(valConv, valueType) : default(TValue);

                if (keyConverted != null)
                {
                    newDict.Add(keyConverted, valConverted);
                }
            }
            return newDict;
        }

        // Return an enumerable collection of pocos
        public IEnumerable<T> Query<T>(string sql, params object[] args)
        {
            return Query<T>(new Sql(sql, args));
        }

        public IEnumerable<T> Query<T>(Sql Sql)
        {
            var sql = Sql.SQL;
            var args = Sql.Arguments;

            if (EnableAutoSelect)
                sql = AddSelectClause<T>(sql);

            OpenSharedConnection();
            try
            {
                using (var cmd = CreateCommand(_sharedConnection, sql, args))
                {
                    IDataReader r;
                    var pd = PocoData.ForType(typeof(T));
                    try
                    {
                        r = cmd.ExecuteReader();
                        OnExecutedCommand(cmd);
                    }
                    catch (Exception x)
                    {
                        OnException(x);
                        throw;
                    }

                    using (r)
                    {
                        var factory = pd.GetFactory(cmd.CommandText, _sharedConnection.ConnectionString, ForceDateTimesToUtc, 0, r.FieldCount, r) as Func<IDataReader, T>;
                        while (true)
                        {
                            T poco;
                            try
                            {
                                if (!r.Read())
                                    yield break;
                                poco = factory(r);
                            }
                            catch (Exception x)
                            {
                                OnException(x);
                                throw;
                            }

                            yield return poco;
                        }
                    }
                }
            }
            finally
            {
                CloseSharedConnection();
            }
        }

        // Multi Fetch
        public List<TRet> Fetch<T1, T2, TRet>(Func<T1, T2, TRet> cb, string sql, params object[] args) { return Query<T1, T2, TRet>(cb, sql, args).ToList(); }
        public List<TRet> Fetch<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> cb, string sql, params object[] args) { return Query<T1, T2, T3, TRet>(cb, sql, args).ToList(); }
        public List<TRet> Fetch<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> cb, string sql, params object[] args) { return Query<T1, T2, T3, T4, TRet>(cb, sql, args).ToList(); }

        // Multi Query
        public IEnumerable<TRet> Query<T1, T2, TRet>(Func<T1, T2, TRet> cb, string sql, params object[] args) { return Query<TRet>(new Type[] { typeof(T1), typeof(T2) }, cb, sql, args); }
        public IEnumerable<TRet> Query<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> cb, string sql, params object[] args) { return Query<TRet>(new Type[] { typeof(T1), typeof(T2), typeof(T3) }, cb, sql, args); }
        public IEnumerable<TRet> Query<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> cb, string sql, params object[] args) { return Query<TRet>(new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) }, cb, sql, args); }

        // Multi Fetch (SQL builder)
        public List<TRet> Fetch<T1, T2, TRet>(Func<T1, T2, TRet> cb, Sql sql) { return Query<T1, T2, TRet>(cb, sql.SQL, sql.Arguments).ToList(); }
        public List<TRet> Fetch<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> cb, Sql sql) { return Query<T1, T2, T3, TRet>(cb, sql.SQL, sql.Arguments).ToList(); }
        public List<TRet> Fetch<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> cb, Sql sql) { return Query<T1, T2, T3, T4, TRet>(cb, sql.SQL, sql.Arguments).ToList(); }

        // Multi Query (SQL builder)
        public IEnumerable<TRet> Query<T1, T2, TRet>(Func<T1, T2, TRet> cb, Sql sql) { return Query<TRet>(new Type[] { typeof(T1), typeof(T2) }, cb, sql.SQL, sql.Arguments); }
        public IEnumerable<TRet> Query<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> cb, Sql sql) { return Query<TRet>(new Type[] { typeof(T1), typeof(T2), typeof(T3) }, cb, sql.SQL, sql.Arguments); }
        public IEnumerable<TRet> Query<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> cb, Sql sql) { return Query<TRet>(new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) }, cb, sql.SQL, sql.Arguments); }

        // Multi Fetch (Simple)
        public List<T1> Fetch<T1, T2>(string sql, params object[] args) { return Query<T1, T2>(sql, args).ToList(); }
        public List<T1> Fetch<T1, T2, T3>(string sql, params object[] args) { return Query<T1, T2, T3>(sql, args).ToList(); }
        public List<T1> Fetch<T1, T2, T3, T4>(string sql, params object[] args) { return Query<T1, T2, T3, T4>(sql, args).ToList(); }

        // Multi Query (Simple)
        public IEnumerable<T1> Query<T1, T2>(string sql, params object[] args) { return Query<T1>(new Type[] { typeof(T1), typeof(T2) }, null, sql, args); }
        public IEnumerable<T1> Query<T1, T2, T3>(string sql, params object[] args) { return Query<T1>(new Type[] { typeof(T1), typeof(T2), typeof(T3) }, null, sql, args); }
        public IEnumerable<T1> Query<T1, T2, T3, T4>(string sql, params object[] args) { return Query<T1>(new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) }, null, sql, args); }

        // Multi Fetch (Simple) (SQL builder)
        public List<T1> Fetch<T1, T2>(Sql sql) { return Query<T1, T2>(sql.SQL, sql.Arguments).ToList(); }
        public List<T1> Fetch<T1, T2, T3>(Sql sql) { return Query<T1, T2, T3>(sql.SQL, sql.Arguments).ToList(); }
        public List<T1> Fetch<T1, T2, T3, T4>(Sql sql) { return Query<T1, T2, T3, T4>(sql.SQL, sql.Arguments).ToList(); }

        // Multi Query (Simple) (SQL builder)
        public IEnumerable<T1> Query<T1, T2>(Sql sql) { return Query<T1>(new Type[] { typeof(T1), typeof(T2) }, null, sql.SQL, sql.Arguments); }
        public IEnumerable<T1> Query<T1, T2, T3>(Sql sql) { return Query<T1>(new Type[] { typeof(T1), typeof(T2), typeof(T3) }, null, sql.SQL, sql.Arguments); }
        public IEnumerable<T1> Query<T1, T2, T3, T4>(Sql sql) { return Query<T1>(new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) }, null, sql.SQL, sql.Arguments); }

        // Automagically guess the property relationships between various POCOs and create a delegate that will set them up
        object GetAutoMapper(Type[] types)
        {
            // Build a key
            var kb = new StringBuilder();
            foreach (var t in types)
            {
                kb.Append(t.ToString());
                kb.Append(":");
            }
            var key = kb.ToString();

            // Check cache
            RWLock.EnterReadLock();
            try
            {
                object mapper;
                if (AutoMappers.TryGetValue(key, out mapper))
                    return mapper;
            }
            finally
            {
                RWLock.ExitReadLock();
            }

            // Create it
            RWLock.EnterWriteLock();
            try
            {
                // Try again
                object mapper;
                if (AutoMappers.TryGetValue(key, out mapper))
                    return mapper;

                // Create a method
                var m = new DynamicMethod("petapoco_automapper", types[0], types, true);
                var il = m.GetILGenerator();

                for (int i = 1; i < types.Length; i++)
                {
                    bool handled = false;
                    for (int j = i - 1; j >= 0; j--)
                    {
                        // Find the property
                        var candidates = from p in types[j].GetProperties() where p.PropertyType == types[i] select p;
                        if (candidates.Count() == 0)
                            continue;
                        if (candidates.Count() > 1)
                            throw new InvalidOperationException(string.Format("Can't auto join {0} as {1} has more than one property of type {0}", types[i], types[j]));

                        // Generate code
                        il.Emit(OpCodes.Ldarg_S, j);
                        il.Emit(OpCodes.Ldarg_S, i);
                        il.Emit(OpCodes.Callvirt, candidates.First().GetSetMethod(true));
                        handled = true;
                    }

                    if (!handled)
                        throw new InvalidOperationException(string.Format("Can't auto join {0}", types[i]));
                }

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ret);

                // Cache it
                var del = m.CreateDelegate(Expression.GetFuncType(types.Concat(types.Take(1)).ToArray()));
                AutoMappers.Add(key, del);
                return del;
            }
            finally
            {
                RWLock.ExitWriteLock();
            }
        }

        // Find the split point in a result set for two different pocos and return the poco factory for the first
        Delegate FindSplitPoint(Type typeThis, Type typeNext, string sql, IDataReader r, ref int pos)
        {
            // Last?
            if (typeNext == null)
                return PocoData.ForType(typeThis).GetFactory(sql, _sharedConnection.ConnectionString, ForceDateTimesToUtc, pos, r.FieldCount - pos, r);

            // Get PocoData for the two types
            PocoData pdThis = PocoData.ForType(typeThis);
            PocoData pdNext = PocoData.ForType(typeNext);

            // Find split point
            int firstColumn = pos;
            var usedColumns = new Dictionary<string, bool>();
            for (; pos < r.FieldCount; pos++)
            {
                // Split if field name has already been used, or if the field doesn't exist in current poco but does in the next
                string fieldName = r.GetName(pos);
                if (usedColumns.ContainsKey(fieldName) || (!pdThis.Columns.ContainsKey(fieldName) && pdNext.Columns.ContainsKey(fieldName)))
                {
                    return pdThis.GetFactory(sql, _sharedConnection.ConnectionString, ForceDateTimesToUtc, firstColumn, pos - firstColumn, r);
                }
                usedColumns.Add(fieldName, true);
            }

            throw new InvalidOperationException(string.Format("Couldn't find split point between {0} and {1}", typeThis, typeNext));
        }

        // Instance data used by the Multipoco factory delegate - essentially a list of the nested poco factories to call
        class MultiPocoFactory
        {
            public List<Delegate> m_Delegates;
            public Delegate GetItem(int index) { return m_Delegates[index]; }
        }

        // Create a multi-poco factory
        Func<IDataReader, object, TRet> CreateMultiPocoFactory<TRet>(Type[] types, string sql, IDataReader r)
        {
            var m = new DynamicMethod("petapoco_multipoco_factory", typeof(TRet), new Type[] { typeof(MultiPocoFactory), typeof(IDataReader), typeof(object) }, typeof(MultiPocoFactory));
            var il = m.GetILGenerator();

            // Load the callback
            il.Emit(OpCodes.Ldarg_2);

            // Call each delegate
            var dels = new List<Delegate>();
            int pos = 0;
            for (int i = 0; i < types.Length; i++)
            {
                // Add to list of delegates to call
                var del = FindSplitPoint(types[i], i + 1 < types.Length ? types[i + 1] : null, sql, r, ref pos);
                dels.Add(del);

                // Get the delegate
                il.Emit(OpCodes.Ldarg_0);													// callback,this
                il.Emit(OpCodes.Ldc_I4, i);													// callback,this,Index
                il.Emit(OpCodes.Callvirt, typeof(MultiPocoFactory).GetMethod("GetItem"));	// callback,Delegate
                il.Emit(OpCodes.Ldarg_1);													// callback,delegate, datareader

                // Call Invoke
                var tDelInvoke = del.GetType().GetMethod("Invoke");
                il.Emit(OpCodes.Callvirt, tDelInvoke);										// Poco left on stack
            }

            // By now we should have the callback and the N pocos all on the stack.  Call the callback and we're done
            il.Emit(OpCodes.Callvirt, Expression.GetFuncType(types.Concat(new Type[] { typeof(TRet) }).ToArray()).GetMethod("Invoke"));
            il.Emit(OpCodes.Ret);

            // Finish up
            return (Func<IDataReader, object, TRet>)m.CreateDelegate(typeof(Func<IDataReader, object, TRet>), new MultiPocoFactory() { m_Delegates = dels });
        }

        // Various cached stuff
        static Dictionary<string, object> MultiPocoFactories = new Dictionary<string, object>();
        static Dictionary<string, object> AutoMappers = new Dictionary<string, object>();
        static System.Threading.ReaderWriterLockSlim RWLock = new System.Threading.ReaderWriterLockSlim();

        // Get (or create) the multi-poco factory for a query
        Func<IDataReader, object, TRet> GetMultiPocoFactory<TRet>(Type[] types, string sql, IDataReader r)
        {
            // Build a key string  (this is crap, should address this at some point)
            var kb = new StringBuilder();
            kb.Append(typeof(TRet).ToString());
            kb.Append(":");
            foreach (var t in types)
            {
                kb.Append(":");
                kb.Append(t.ToString());
            }
            kb.Append(":"); kb.Append(_sharedConnection.ConnectionString);
            kb.Append(":"); kb.Append(ForceDateTimesToUtc);
            kb.Append(":"); kb.Append(sql);
            string key = kb.ToString();

            // Check cache
            RWLock.EnterReadLock();
            try
            {
                object oFactory;
                if (MultiPocoFactories.TryGetValue(key, out oFactory))
                    return (Func<IDataReader, object, TRet>)oFactory;
            }
            finally
            {
                RWLock.ExitReadLock();
            }

            // Cache it
            RWLock.EnterWriteLock();
            try
            {
                // Check again
                object oFactory;
                if (MultiPocoFactories.TryGetValue(key, out oFactory))
                    return (Func<IDataReader, object, TRet>)oFactory;

                // Create the factory
                var Factory = CreateMultiPocoFactory<TRet>(types, sql, r);

                MultiPocoFactories.Add(key, Factory);
                return Factory;
            }
            finally
            {
                RWLock.ExitWriteLock();
            }

        }

        // Actual implementation of the multi-poco query
        public IEnumerable<TRet> Query<TRet>(Type[] types, object cb, string sql, params object[] args)
        {
            OpenSharedConnection();
            try
            {
                using (var cmd = CreateCommand(_sharedConnection, sql, args))
                {
                    IDataReader r;
                    try
                    {
                        r = cmd.ExecuteReader();
                        OnExecutedCommand(cmd);
                    }
                    catch (Exception x)
                    {
                        OnException(x);
                        throw;
                    }
                    var factory = GetMultiPocoFactory<TRet>(types, sql, r);
                    if (cb == null)
                        cb = GetAutoMapper(types.ToArray());
                    bool bNeedTerminator = false;
                    using (r)
                    {
                        while (true)
                        {
                            TRet poco;
                            try
                            {
                                if (!r.Read())
                                    break;
                                poco = factory(r, cb);
                            }
                            catch (Exception x)
                            {
                                OnException(x);
                                throw;
                            }

                            if (poco != null)
                                yield return poco;
                            else
                                bNeedTerminator = true;
                        }
                        if (bNeedTerminator)
                        {
                            var poco = (TRet)(cb as Delegate).DynamicInvoke(new object[types.Length]);
                            if (poco != null)
                                yield return poco;
                            else
                                yield break;
                        }
                    }
                }
            }
            finally
            {
                CloseSharedConnection();
            }
        }

        public bool Exists<T>(object primaryKey)
        {
            var index = 0;
            var primaryKeyValuePairs = GetPrimaryKeyValues(PocoData.ForType(typeof(T)).TableInfo.PrimaryKey, primaryKey);
            return FirstOrDefault<T>(string.Format("WHERE {0}", BuildPrimaryKeySql(primaryKeyValuePairs, ref index)), primaryKeyValuePairs.Select(x => x.Value).ToArray()) != null;
        }


        public bool Exists<T>(string sql, params object[] args)
        {
            var poco = PocoData.ForType(typeof(T)).TableInfo;

            string existsTemplate;

            switch (_dbType)
            {
                case DBType.SQLite:
                case DBType.MySql:
                    {
                        existsTemplate = "SELECT EXISTS (SELECT 1 FROM {0} {1})";
                        break;
                    }

                case DBType.SqlServer:
                    {
                        existsTemplate = "IF EXISTS (SELECT 1 FROM {0} {1}) SELECT 1 ELSE SELECT 0";
                        break;
                    }
                default:
                    {
                        existsTemplate = "SELECT COUNT(*) FROM {0} {1}";
                        break;
                    }
            }


            return ExecuteScalar<int>(string.Format(existsTemplate, poco.TableName, sql), args) != 0;
        }
        public T Single<T>(object primaryKey)
        {
            var index = 0;
            var primaryKeyValuePairs = GetPrimaryKeyValues(PocoData.ForType(typeof(T)).TableInfo.PrimaryKey, primaryKey);
            return Single<T>(string.Format("WHERE {0}", BuildPrimaryKeySql(primaryKeyValuePairs, ref index)), primaryKeyValuePairs.Select(x => x.Value).ToArray());
        }
        public T SingleOrDefault<T>(object primaryKey)
        {
            var index = 0;
            var primaryKeyValuePairs = GetPrimaryKeyValues(PocoData.ForType(typeof(T)).TableInfo.PrimaryKey, primaryKey);
            return SingleOrDefault<T>(string.Format("WHERE {0}", BuildPrimaryKeySql(primaryKeyValuePairs, ref index)), primaryKeyValuePairs.Select(x => x.Value).ToArray());
        }
        public T Single<T>(string sql, params object[] args)
        {
            return Query<T>(sql, args).Single();
        }
        public T SingleOrDefault<T>(string sql, params object[] args)
        {
            return Query<T>(sql, args).SingleOrDefault();
        }
        public T First<T>(string sql, params object[] args)
        {
            return Query<T>(sql, args).First();
        }
        public T FirstOrDefault<T>(string sql, params object[] args)
        {
            return Query<T>(sql, args).FirstOrDefault();
        }
        public T Single<T>(Sql sql)
        {
            return Query<T>(sql).Single();
        }
        public T SingleOrDefault<T>(Sql sql)
        {
            return Query<T>(sql).SingleOrDefault();
        }
        public T First<T>(Sql sql)
        {
            return Query<T>(sql).First();
        }
        public T FirstOrDefault<T>(Sql sql)
        {
            return Query<T>(sql).FirstOrDefault();
        }

        public string EscapeTableName(string str)
        {
            // Assume table names with "dot" are already escaped
            return str.IndexOf('.') >= 0 ? str : EscapeSqlIdentifier(str);
        }

        public string EscapeSqlIdentifier(string str)
        {
            switch (_dbType)
            {
                case DBType.MySql:
                    return string.Format("`{0}`", str);

                case DBType.PostgreSQL:
                    return string.Format("\"{0}\"", str);

                case DBType.Oracle:
                    return string.Format("\"{0}\"", str.ToUpperInvariant());

                default:
                    return string.Format("[{0}]", str);
            }
        }

        public object Insert(string tableName, string primaryKeyName, object poco)
        {
            return Insert(tableName, primaryKeyName, true, poco);
        }

        // Insert a poco into a table.  If the poco has a property with the same name 
        // as the primary key the id of the new record is assigned to it.  Either way,
        // the new id is returned.
        public object Insert(string tableName, string primaryKeyName, bool autoIncrement, object poco)
        {
            try
            {
                OpenSharedConnection();
                try
                {
                    using (var cmd = CreateCommand(_sharedConnection, ""))
                    {
                        var pd = PocoData.ForObject(poco, primaryKeyName);
                        var names = new List<string>();
                        var values = new List<string>();
                        var index = 0;
                        var versionName = "";

                        foreach (var i in pd.Columns)
                        {
                            // Don't insert result columns
                            if (i.Value.ResultColumn)
                                continue;

                            // Don't insert the primary key (except under oracle where we need bring in the next sequence value)
                            if (autoIncrement && primaryKeyName != null && string.Compare(i.Key, primaryKeyName, true) == 0)
                            {
                                if (_dbType == DBType.Oracle && !string.IsNullOrEmpty(pd.TableInfo.SequenceName))
                                {
                                    names.Add(i.Key);
                                    values.Add(string.Format("{0}.nextval", pd.TableInfo.SequenceName));
                                }
                                continue;
                            }

                            names.Add(EscapeSqlIdentifier(i.Key));
                            values.Add(string.Format("{0}{1}", _paramPrefix, index++));

                            object val = i.Value.GetValue(poco);
                            if (i.Value.VersionColumn)
                            {
                                val = 1;
                                versionName = i.Key;
                            }

                            AddParam(cmd, val, _paramPrefix);
                        }

                        cmd.CommandText = string.Format("INSERT INTO {0} ({1}) VALUES ({2})",
                                EscapeTableName(tableName),
                                string.Join(",", names.ToArray()),
                                string.Join(",", values.ToArray())
                                );

                        object id;

                        if (!autoIncrement)
                        {
                            DoPreExecute(cmd);
                            cmd.ExecuteNonQuery();
                            OnExecutedCommand(cmd);
                            id = true;
                        }
                        else
                        {

                            switch (_dbType)
                            {
                                case DBType.SqlServerCE:
                                    DoPreExecute(cmd);
                                    cmd.ExecuteNonQuery();
                                    OnExecutedCommand(cmd);
                                    id = ExecuteScalar<object>("SELECT @@@IDENTITY AS NewID;");
                                    break;
                                case DBType.SqlServer:
                                    cmd.CommandText += ";\nSELECT SCOPE_IDENTITY() AS NewID;";
                                    DoPreExecute(cmd);
                                    id = cmd.ExecuteScalar();
                                    OnExecutedCommand(cmd);
                                    break;
                                case DBType.PostgreSQL:
                                    if (primaryKeyName != null)
                                    {
                                        cmd.CommandText += string.Format("returning {0} as NewID", EscapeSqlIdentifier(primaryKeyName));
                                        DoPreExecute(cmd);
                                        id = cmd.ExecuteScalar();
                                    }
                                    else
                                    {
                                        id = -1;
                                        DoPreExecute(cmd);
                                        cmd.ExecuteNonQuery();
                                    }
                                    OnExecutedCommand(cmd);
                                    break;
                                case DBType.Oracle:
                                    if (primaryKeyName != null)
                                    {
                                        cmd.CommandText += string.Format(" returning {0} into :newid", EscapeSqlIdentifier(primaryKeyName));
                                        var param = cmd.CreateParameter();
                                        param.ParameterName = ":newid";
                                        param.Value = DBNull.Value;
                                        param.Direction = ParameterDirection.ReturnValue;
                                        param.DbType = DbType.Int64;
                                        cmd.Parameters.Add(param);
                                        DoPreExecute(cmd);
                                        cmd.ExecuteNonQuery();
                                        id = param.Value;
                                    }
                                    else
                                    {
                                        id = -1;
                                        DoPreExecute(cmd);
                                        cmd.ExecuteNonQuery();
                                    }
                                    OnExecutedCommand(cmd);
                                    break;
                                case DBType.SQLite:
                                    if (primaryKeyName != null)
                                    {
                                        cmd.CommandText += ";\nSELECT last_insert_rowid();";
                                        DoPreExecute(cmd);
                                        id = cmd.ExecuteScalar();
                                    }
                                    else
                                    {
                                        id = -1;
                                        DoPreExecute(cmd);
                                        cmd.ExecuteNonQuery();
                                    }
                                    OnExecutedCommand(cmd);
                                    break;
                                default:
                                    cmd.CommandText += ";\nSELECT @@IDENTITY AS NewID;";
                                    DoPreExecute(cmd);
                                    id = cmd.ExecuteScalar();
                                    OnExecutedCommand(cmd);
                                    break;
                            }

                            // Assign the ID back to the primary key property
                            if (primaryKeyName != null)
                            {
                                PocoColumn pc;
                                if (pd.Columns.TryGetValue(primaryKeyName, out pc))
                                {
                                    pc.SetValue(poco, pc.ChangeType(id));
                                }
                            }
                        }

                        // Assign the Version column
                        if (!string.IsNullOrEmpty(versionName))
                        {
                            PocoColumn pc;
                            if (pd.Columns.TryGetValue(versionName, out pc))
                            {
                                pc.SetValue(poco, pc.ChangeType(1));
                            }
                        }

                        return id;
                    }
                }
                finally
                {
                    CloseSharedConnection();
                }
            }
            catch (Exception x)
            {
                OnException(x);
                throw;
            }
        }

        // Insert an annotated poco object
        public object Insert(object poco)
        {
            var pd = PocoData.ForType(poco.GetType());
            return Insert(pd.TableInfo.TableName, pd.TableInfo.PrimaryKey, pd.TableInfo.AutoIncrement, poco);
        }

        public void InsertMany<T>(IEnumerable<T> pocoList)
        {
            using (var tran = GetTransaction())
            {
                foreach (var poco in pocoList)
                {
                    Insert(poco);
                }

                tran.Complete();
            }
        }

        public int Update(string tableName, string primaryKeyName, object poco, object primaryKeyValue)
        {
            return Update(tableName, primaryKeyName, poco, primaryKeyValue, null);
        }


        // Update a record with values from a poco.  primary key value can be either supplied or read from the poco
        public int Update(string tableName, string primaryKeyName, object poco, object primaryKeyValue, IEnumerable<string> columns)
        {
            try
            {
                OpenSharedConnection();
                try
                {
                    using (var cmd = CreateCommand(_sharedConnection, ""))
                    {
                        var sb = new StringBuilder();
                        var index = 0;
                        var pd = PocoData.ForObject(poco, primaryKeyName);
                        string versionName = null;
                        object versionValue = null;

                        var primaryKeyValuePairs = GetPrimaryKeyValues(primaryKeyName, primaryKeyValue);

                        foreach (var i in pd.Columns)
                        {
                            // Don't update the primary key, but grab the value if we don't have it
                            if (primaryKeyValue == null && primaryKeyValuePairs.ContainsKey(i.Key))
                            {
                                primaryKeyValuePairs[i.Key] = i.Value.PropertyInfo.GetValue(poco, null);
                                continue;
                            }

                            // Dont update result only columns
                            if (i.Value.ResultColumn)
                                continue;

                            if (!i.Value.VersionColumn && columns != null && !columns.Contains(i.Value.ColumnName, StringComparer.OrdinalIgnoreCase))
                                continue;

                            object value = i.Value.PropertyInfo.GetValue(poco, null);

                            if (i.Value.VersionColumn)
                            {
                                versionName = i.Key;
                                versionValue = value;
                                value = Convert.ToInt64(value) + 1;
                            }

                            // Build the sql
                            if (index > 0)
                                sb.Append(", ");
                            sb.AppendFormat("{0} = {1}{2}", EscapeSqlIdentifier(i.Key), _paramPrefix, index++);

                            // Store the parameter in the command
                            AddParam(cmd, value, _paramPrefix);
                        }


                        cmd.CommandText = string.Format("UPDATE {0} SET {1} WHERE {2}",
                                            EscapeTableName(tableName), sb.ToString(), BuildPrimaryKeySql(primaryKeyValuePairs, ref index));

                        foreach (var keyValue in primaryKeyValuePairs)
                        {
                            AddParam(cmd, keyValue.Value, _paramPrefix);
                        }

                        if (!string.IsNullOrEmpty(versionName))
                        {
                            cmd.CommandText += string.Format(" AND {0} = {1}{2}", EscapeSqlIdentifier(versionName), _paramPrefix, index++);
                            AddParam(cmd, versionValue, _paramPrefix);
                        }

                        DoPreExecute(cmd);

                        // Do it
                        var result = cmd.ExecuteNonQuery();
                        OnExecutedCommand(cmd);

                        // Set Version
                        if (!string.IsNullOrEmpty(versionName))
                        {
                            PocoColumn pc;
                            if (pd.Columns.TryGetValue(versionName, out pc))
                            {
                                pc.PropertyInfo.SetValue(poco, Convert.ChangeType(Convert.ToInt64(versionValue) + 1, pc.PropertyInfo.PropertyType), null);
                            }
                        }

                        return result;
                    }
                }
                finally
                {
                    CloseSharedConnection();
                }
            }
            catch (Exception x)
            {
                OnException(x);
                throw;
            }
        }

        private string BuildPrimaryKeySql(Dictionary<string, object> primaryKeyValuePair, ref int index)
        {
            var tempIndex = index;
            index += primaryKeyValuePair.Count;
            return string.Join(" AND ", primaryKeyValuePair.Select((x, i) => string.Format("{0} = {1}{2}", EscapeSqlIdentifier(x.Key), _paramPrefix, tempIndex + i)).ToArray());
        }

        private Dictionary<string, object> GetPrimaryKeyValues(string primaryKeyName, object primaryKeyValue)
        {
            Dictionary<string, object> primaryKeyValues;

            var multiplePrimaryKeysNames = primaryKeyName.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
            if (primaryKeyValue != null)
            {
                if (multiplePrimaryKeysNames.Length == 1)
                    primaryKeyValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) { { primaryKeyName, primaryKeyValue } };
                else
                    primaryKeyValues = multiplePrimaryKeysNames.ToDictionary(x => x,
                        x => primaryKeyValue.GetType().GetProperties()
                            .Where(y => string.Equals(x, y.Name, StringComparison.OrdinalIgnoreCase))
                            .Single().GetValue(primaryKeyValue, null), StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                primaryKeyValues = multiplePrimaryKeysNames.ToDictionary(x => x, x => (object)null, StringComparer.OrdinalIgnoreCase);
            }
            return primaryKeyValues;
        }

        public int Update(string tableName, string primaryKeyName, object poco)
        {
            return Update(tableName, primaryKeyName, poco, null);
        }

        public int Update(string tableName, string primaryKeyName, object poco, IEnumerable<string> columns)
        {
            return Update(tableName, primaryKeyName, poco, null, columns);
        }

        public int Update(object poco, IEnumerable<string> columns)
        {
            return Update(poco, null, columns);
        }

        public int Update(object poco)
        {
            return Update(poco, null, null);
        }

        public int Update(object poco, object primaryKeyValue)
        {
            return Update(poco, primaryKeyValue, null);
        }
        public int Update(object poco, object primaryKeyValue, IEnumerable<string> columns)
        {
            var pd = PocoData.ForType(poco.GetType());
            return Update(pd.TableInfo.TableName, pd.TableInfo.PrimaryKey, poco, primaryKeyValue, columns);
        }

        public int Update<T>(string sql, params object[] args)
        {
            var pd = PocoData.ForType(typeof(T));
            return Execute(string.Format("UPDATE {0} {1}", EscapeTableName(pd.TableInfo.TableName), sql), args);
        }

        public int Update<T>(Sql sql)
        {
            var pd = PocoData.ForType(typeof(T));
            return Execute(new Sql(string.Format("UPDATE {0}", EscapeTableName(pd.TableInfo.TableName))).Append(sql));
        }

        public void UpdateMany<T>(IEnumerable<T> pocoList)
        {
            using (var tran = GetTransaction())
            {
                foreach (var poco in pocoList)
                {
                    Update(poco);
                }

                tran.Complete();
            }
        }


        public int Delete(string tableName, string primaryKeyName, object poco)
        {
            return Delete(tableName, primaryKeyName, poco, null);
        }

        public int Delete(string tableName, string primaryKeyName, object poco, object primaryKeyValue)
        {
            var primaryKeyValuePairs = GetPrimaryKeyValues(primaryKeyName, primaryKeyValue);
            // If primary key value not specified, pick it up from the object
            if (primaryKeyValue == null)
            {
                var pd = PocoData.ForObject(poco, primaryKeyName);
                foreach (var i in pd.Columns)
                {
                    if (primaryKeyValuePairs.ContainsKey(i.Key))
                    {
                        primaryKeyValuePairs[i.Key] = i.Value.PropertyInfo.GetValue(poco, null);
                    }
                }
            }

            // Do it
            var index = 0;
            var sql = string.Format("DELETE FROM {0} WHERE {1}", tableName, BuildPrimaryKeySql(primaryKeyValuePairs, ref index));
            return Execute(sql, primaryKeyValuePairs.Select(x => x.Value).ToArray());
        }

        public int Delete(object poco)
        {
            var pd = PocoData.ForType(poco.GetType());
            return Delete(pd.TableInfo.TableName, pd.TableInfo.PrimaryKey, poco);
        }

        public int Delete<T>(object pocoOrPrimaryKey)
        {
            if (pocoOrPrimaryKey.GetType() == typeof(T))
                return Delete(pocoOrPrimaryKey);
            var pd = PocoData.ForType(typeof(T));
            return Delete(pd.TableInfo.TableName, pd.TableInfo.PrimaryKey, null, pocoOrPrimaryKey);
        }

        public int Delete<T>(string sql, params object[] args)
        {
            var pd = PocoData.ForType(typeof(T));
            return Execute(string.Format("DELETE FROM {0} {1}", EscapeTableName(pd.TableInfo.TableName), sql), args);
        }

        public int Delete<T>(Sql sql)
        {
            var pd = PocoData.ForType(typeof(T));
            return Execute(new Sql(string.Format("DELETE FROM {0}", EscapeTableName(pd.TableInfo.TableName))).Append(sql));
        }

        // Check if a poco represents a new record
        public bool IsNew(string primaryKeyName, object poco)
        {
            var pd = PocoData.ForObject(poco, primaryKeyName);
            object pk;
            PocoColumn pc;
            if (pd.Columns.TryGetValue(primaryKeyName, out pc))
            {
                pk = pc.GetValue(poco);
            }
#if !PETAPOCO_NO_DYNAMIC
            else if (poco.GetType() == typeof(System.Dynamic.ExpandoObject))
            {
                return true;
            }
#endif
            else
            {
                var pi = poco.GetType().GetProperty(primaryKeyName);
                if (pi == null)
                    throw new ArgumentException(string.Format("The object doesn't have a property matching the primary key column name '{0}'", primaryKeyName));
                pk = pi.GetValue(poco, null);
            }

            if (pk == null)
                return true;

            var type = pk.GetType();

            if (type.IsValueType)
            {
                // Common primary key types
                if (type == typeof(long))
                    return (long)pk == 0;
                else if (type == typeof(ulong))
                    return (ulong)pk == 0;
                else if (type == typeof(int))
                    return (int)pk == 0;
                else if (type == typeof(uint))
                    return (uint)pk == 0;

                // Create a default instance and compare
                return pk == Activator.CreateInstance(pk.GetType());
            }
            else
            {
                return pk == null;
            }
        }

        public bool IsNew(object poco)
        {
            var pd = PocoData.ForType(poco.GetType());
            if (!pd.TableInfo.AutoIncrement)
                throw new InvalidOperationException("IsNew() and Save() are only supported on tables with auto-increment/identity primary key columns");
            return IsNew(pd.TableInfo.PrimaryKey, poco);
        }

        // Insert new record or Update existing record
        public void Save(string tableName, string primaryKeyName, object poco)
        {
            if (IsNew(primaryKeyName, poco))
            {
                Insert(tableName, primaryKeyName, true, poco);
            }
            else
            {
                Update(tableName, primaryKeyName, poco);
            }
        }

        public void Save(object poco)
        {
            var pd = PocoData.ForType(poco.GetType());
            Save(pd.TableInfo.TableName, pd.TableInfo.PrimaryKey, poco);
        }

        public void SaveMany<T>(IEnumerable<T> pocoList)
        {
            using (var tran = GetTransaction())
            {
                foreach (var poco in pocoList)
                {
                    Save(poco);
                }

                tran.Complete();
            }
        }

        public int CommandTimeout { get; set; }
        public int OneTimeCommandTimeout { get; set; }

        void DoPreExecute(IDbCommand cmd)
        {
            // Setup command timeout
            if (OneTimeCommandTimeout != 0)
            {
                cmd.CommandTimeout = OneTimeCommandTimeout;
                OneTimeCommandTimeout = 0;
            }
            else if (CommandTimeout != 0)
            {
                cmd.CommandTimeout = CommandTimeout;
            }

            // Call hook
            OnExecutingCommand(cmd);

            // Save it
            _lastSql = cmd.CommandText;
            _lastArgs = (from IDataParameter parameter in cmd.Parameters select parameter.Value).ToArray();
        }

        public string LastSQL { get { return _lastSql; } }
        public object[] LastArgs { get { return _lastArgs; } }
        public string LastCommand
        {
            get { return FormatCommand(_lastSql, _lastArgs); }
        }

        public string FormatCommand(IDbCommand cmd)
        {
            return FormatCommand(cmd.CommandText, (from IDataParameter parameter in cmd.Parameters select parameter.Value).ToArray());
        }

        public string FormatCommand(string sql, object[] args)
        {
            var sb = new StringBuilder();
            if (sql == null)
                return "";
            sb.Append(sql);
            if (args != null && args.Length > 0)
            {
                sb.Append("\n");
                for (int i = 0; i < args.Length; i++)
                {
                    sb.AppendFormat("\t -> {0}{1} [{2}] = \"{3}\"\n", _paramPrefix, i, args[i].GetType().Name, args[i]);
                }
                sb.Remove(sb.Length - 1, 1);
            }
            return sb.ToString();
        }


        public static IMapper Mapper
        {
            get;
            set;
        }

        public class PocoColumn
        {
            public string ColumnName;
            public PropertyInfo PropertyInfo;
            public bool ResultColumn;
            public bool VersionColumn;
            public virtual void SetValue(object target, object val) { PropertyInfo.SetValue(target, val, null); }
            public virtual object GetValue(object target) { return PropertyInfo.GetValue(target, null); }
            public virtual object ChangeType(object val) { return Convert.ChangeType(val, PropertyInfo.PropertyType); }
        }
        public class ExpandoColumn : PocoColumn
        {
            public override void SetValue(object target, object val) { (target as IDictionary<string, object>)[ColumnName] = val; }
            public override object GetValue(object target)
            {
                object val = null;
                (target as IDictionary<string, object>).TryGetValue(ColumnName, out val);
                return val;
            }
            public override object ChangeType(object val) { return val; }
        }
        public class PocoData
        {
            public static PocoData ForObject(object o, string primaryKeyName)
            {
                var t = o.GetType();
#if !PETAPOCO_NO_DYNAMIC
                if (t == typeof(System.Dynamic.ExpandoObject))
                {
                    var pd = new PocoData();
                    pd.TableInfo = new TableInfo();
                    pd.Columns = new Dictionary<string, PocoColumn>(StringComparer.OrdinalIgnoreCase);
                    pd.Columns.Add(primaryKeyName, new ExpandoColumn() { ColumnName = primaryKeyName });
                    pd.TableInfo.PrimaryKey = primaryKeyName;
                    pd.TableInfo.AutoIncrement = true;
                    foreach (var col in (o as IDictionary<string, object>).Keys)
                    {
                        if (col != primaryKeyName)
                            pd.Columns.Add(col, new ExpandoColumn() { ColumnName = col });
                    }
                    return pd;
                }
                else
#endif
                    return ForType(t);
            }
            static System.Threading.ReaderWriterLockSlim RWLock = new System.Threading.ReaderWriterLockSlim();
            public static PocoData ForType(Type t)
            {
#if !PETAPOCO_NO_DYNAMIC
                if (t == typeof(System.Dynamic.ExpandoObject))
                    throw new InvalidOperationException("Can't use dynamic types with this method");
#endif
                // Check cache
                RWLock.EnterReadLock();
                PocoData pd;
                try
                {
                    if (m_PocoDatas.TryGetValue(t, out pd))
                        return pd;
                }
                finally
                {
                    RWLock.ExitReadLock();
                }


                // Cache it
                RWLock.EnterWriteLock();
                try
                {
                    // Check again
                    if (m_PocoDatas.TryGetValue(t, out pd))
                        return pd;

                    // Create it
                    pd = new PocoData(t);
                    m_PocoDatas.Add(t, pd);
                }
                finally
                {
                    RWLock.ExitWriteLock();
                }

                return pd;
            }

            public PocoData()
            {
            }

            public PocoData(Type t)
            {
                type = t;
                TableInfo = new TableInfo();

                // Get the table name
                var a = t.GetCustomAttributes(typeof(TableNameAttribute), true);
                TableInfo.TableName = a.Length == 0 ? t.Name : (a[0] as TableNameAttribute).Value;

                // Get the primary key
                a = t.GetCustomAttributes(typeof(PrimaryKeyAttribute), true);
                TableInfo.PrimaryKey = a.Length == 0 ? "ID" : (a[0] as PrimaryKeyAttribute).Value;
                TableInfo.SequenceName = a.Length == 0 ? null : (a[0] as PrimaryKeyAttribute).sequenceName;
                TableInfo.AutoIncrement = a.Length == 0 ? false : (a[0] as PrimaryKeyAttribute).autoIncrement;

                // Set autoincrement false if primary key has multiple columns
                TableInfo.AutoIncrement = TableInfo.AutoIncrement ? !TableInfo.PrimaryKey.Contains(',') : TableInfo.AutoIncrement;

                // Call column mapper
                if (Database.Mapper != null)
                    Database.Mapper.GetTableInfo(t, TableInfo);

                // Work out bound properties
                bool ExplicitColumns = t.GetCustomAttributes(typeof(ExplicitColumnsAttribute), true).Length > 0;
                Columns = new Dictionary<string, PocoColumn>(StringComparer.OrdinalIgnoreCase);
                foreach (var pi in t.GetProperties())
                {
                    // Work out if properties is to be included
                    var ColAttrs = pi.GetCustomAttributes(typeof(ColumnAttribute), true);
                    if (ExplicitColumns)
                    {
                        if (ColAttrs.Length == 0)
                            continue;
                    }
                    else
                    {
                        if (pi.GetCustomAttributes(typeof(IgnoreAttribute), true).Length != 0)
                            continue;
                    }

                    var pc = new PocoColumn();
                    pc.PropertyInfo = pi;

                    // Work out the DB column name
                    if (ColAttrs.Length > 0)
                    {
                        var colattr = (ColumnAttribute)ColAttrs[0];
                        pc.ColumnName = colattr.Name;
                        if ((colattr as ResultColumnAttribute) != null)
                            pc.ResultColumn = true;
                        if ((colattr as VersionColumnAttribute) != null)
                            pc.VersionColumn = true;
                    }
                    if (pc.ColumnName == null)
                    {
                        pc.ColumnName = pi.Name;
                        if (Database.Mapper != null && !Database.Mapper.MapPropertyToColumn(pi, ref pc.ColumnName, ref pc.ResultColumn))
                            continue;
                    }

                    // Store it
                    Columns.Add(pc.ColumnName, pc);
                }

                // Build column list for automatic select
                QueryColumns = (from c in Columns where !c.Value.ResultColumn select c.Key).ToArray();

            }

            static bool IsIntegralType(Type t)
            {
                var tc = Type.GetTypeCode(t);
                return tc >= TypeCode.SByte && tc <= TypeCode.UInt64;
            }

            // Create factory function that can convert a IDataReader record into a POCO
            public Delegate GetFactory(string sql, string connString, bool ForceDateTimesToUtc, int firstColumn, int countColumns, IDataReader r)
            {
                // Check cache
                var key = string.Format("{0}:{1}:{2}:{3}:{4}", sql, connString, ForceDateTimesToUtc, firstColumn, countColumns);
                RWLock.EnterReadLock();
                try
                {
                    // Have we already created it?
                    Delegate factory;
                    if (PocoFactories.TryGetValue(key, out factory))
                        return factory;
                }
                finally
                {
                    RWLock.ExitReadLock();
                }

                // Take the writer lock
                RWLock.EnterWriteLock();

                try
                {

                    // Check again, just in case
                    Delegate factory;
                    if (PocoFactories.TryGetValue(key, out factory))
                        return factory;

                    // Create the method
                    var m = new DynamicMethod("petapoco_factory_" + PocoFactories.Count.ToString(), type, new Type[] { typeof(IDataReader) }, true);
                    var il = m.GetILGenerator();

#if !PETAPOCO_NO_DYNAMIC
                    if (type == typeof(object))
                    {
                        // var poco=new T()
                        il.Emit(OpCodes.Newobj, typeof(System.Dynamic.ExpandoObject).GetConstructor(Type.EmptyTypes));			// obj

                        MethodInfo fnAdd = typeof(IDictionary<string, object>).GetMethod("Add");

                        // Enumerate all fields generating a set assignment for the column
                        for (int i = firstColumn; i < firstColumn + countColumns; i++)
                        {
                            var srcType = r.GetFieldType(i);

                            il.Emit(OpCodes.Dup);						// obj, obj
                            il.Emit(OpCodes.Ldstr, r.GetName(i));		// obj, obj, fieldname

                            // Get the converter
                            Func<object, object> converter = null;
                            if (Database.Mapper != null)
                                converter = Database.Mapper.GetFromDbConverter(null, srcType);
                            if (ForceDateTimesToUtc && converter == null && srcType == typeof(DateTime))
                                converter = delegate(object src) { return new DateTime(((DateTime)src).Ticks, DateTimeKind.Utc); };

                            // Setup stack for call to converter
                            AddConverterToStack(il, converter);

                            // r[i]
                            il.Emit(OpCodes.Ldarg_0);					// obj, obj, fieldname, converter?,    rdr
                            il.Emit(OpCodes.Ldc_I4, i);					// obj, obj, fieldname, converter?,  rdr,i
                            il.Emit(OpCodes.Callvirt, fnGetValue);		// obj, obj, fieldname, converter?,  value

                            // Convert DBNull to null
                            il.Emit(OpCodes.Dup);						// obj, obj, fieldname, converter?,  value, value
                            il.Emit(OpCodes.Isinst, typeof(DBNull));	// obj, obj, fieldname, converter?,  value, (value or null)
                            var lblNotNull = il.DefineLabel();
                            il.Emit(OpCodes.Brfalse_S, lblNotNull);		// obj, obj, fieldname, converter?,  value
                            il.Emit(OpCodes.Pop);						// obj, obj, fieldname, converter?
                            if (converter != null)
                                il.Emit(OpCodes.Pop);					// obj, obj, fieldname, 
                            il.Emit(OpCodes.Ldnull);					// obj, obj, fieldname, null
                            if (converter != null)
                            {
                                var lblReady = il.DefineLabel();
                                il.Emit(OpCodes.Br_S, lblReady);
                                il.MarkLabel(lblNotNull);
                                il.Emit(OpCodes.Callvirt, fnInvoke);
                                il.MarkLabel(lblReady);
                            }
                            else
                            {
                                il.MarkLabel(lblNotNull);
                            }

                            il.Emit(OpCodes.Callvirt, fnAdd);
                        }
                    }
                    else
#endif
                        if (type.IsValueType || type == typeof(string) || type == typeof(byte[]))
                        {
                            // Do we need to install a converter?
                            var srcType = r.GetFieldType(0);
                            var converter = GetConverter(ForceDateTimesToUtc, null, srcType, type);

                            // "if (!rdr.IsDBNull(i))"
                            il.Emit(OpCodes.Ldarg_0);										// rdr
                            il.Emit(OpCodes.Ldc_I4_0);										// rdr,0
                            il.Emit(OpCodes.Callvirt, fnIsDBNull);							// bool
                            var lblCont = il.DefineLabel();
                            il.Emit(OpCodes.Brfalse_S, lblCont);
                            il.Emit(OpCodes.Ldnull);										// null
                            var lblFin = il.DefineLabel();
                            il.Emit(OpCodes.Br_S, lblFin);

                            il.MarkLabel(lblCont);

                            // Setup stack for call to converter
                            AddConverterToStack(il, converter);

                            il.Emit(OpCodes.Ldarg_0);										// rdr
                            il.Emit(OpCodes.Ldc_I4_0);										// rdr,0
                            il.Emit(OpCodes.Callvirt, fnGetValue);							// value

                            // Call the converter
                            if (converter != null)
                                il.Emit(OpCodes.Callvirt, fnInvoke);

                            il.MarkLabel(lblFin);
                            il.Emit(OpCodes.Unbox_Any, type);								// value converted
                        }
                        else if (type == typeof(Dictionary<string, object>))
                        {
                            Func<IDataReader, Dictionary<string, object>> func = reader =>
                            {
                                var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                                for (int i = firstColumn; i < firstColumn + countColumns; i++)
                                {
                                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                                    var name = reader.GetName(i);
                                    if (!dict.ContainsKey(name))
                                        dict.Add(name, value);
                                }
                                return dict;
                            };

                            var localDel = Delegate.CreateDelegate(typeof(Func<IDataReader, Dictionary<string, object>>), func.Target, func.Method);
                            PocoFactories.Add(key, localDel);
                            return localDel;
                        }
                        else
                        {
                            // var poco=new T()
                            il.Emit(OpCodes.Newobj, type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[0], null));

                            // Enumerate all fields generating a set assignment for the column
                            for (int i = firstColumn; i < firstColumn + countColumns; i++)
                            {
                                // Get the PocoColumn for this db column, ignore if not known
                                PocoColumn pc;
                                if (!Columns.TryGetValue(r.GetName(i), out pc) && !Columns.TryGetValue(r.GetName(i).Replace("_", ""), out pc))
                                {
                                    continue;
                                }

                                // Get the source type for this column
                                var srcType = r.GetFieldType(i);
                                var dstType = pc.PropertyInfo.PropertyType;

                                // "if (!rdr.IsDBNull(i))"
                                il.Emit(OpCodes.Ldarg_0);										// poco,rdr
                                il.Emit(OpCodes.Ldc_I4, i);										// poco,rdr,i
                                il.Emit(OpCodes.Callvirt, fnIsDBNull);							// poco,bool
                                var lblNext = il.DefineLabel();
                                il.Emit(OpCodes.Brtrue_S, lblNext);								// poco

                                il.Emit(OpCodes.Dup);											// poco,poco

                                // Do we need to install a converter?
                                var converter = GetConverter(ForceDateTimesToUtc, pc, srcType, dstType);

                                // Fast
                                bool Handled = false;
                                if (converter == null)
                                {
                                    var valuegetter = typeof(IDataRecord).GetMethod("Get" + srcType.Name, new Type[] { typeof(int) });
                                    if (valuegetter != null
                                            && valuegetter.ReturnType == srcType
                                            && (valuegetter.ReturnType == dstType || valuegetter.ReturnType == Nullable.GetUnderlyingType(dstType)))
                                    {
                                        il.Emit(OpCodes.Ldarg_0);										// *,rdr
                                        il.Emit(OpCodes.Ldc_I4, i);										// *,rdr,i
                                        il.Emit(OpCodes.Callvirt, valuegetter);							// *,value

                                        // Convert to Nullable
                                        if (Nullable.GetUnderlyingType(dstType) != null)
                                        {
                                            il.Emit(OpCodes.Newobj, dstType.GetConstructor(new Type[] { Nullable.GetUnderlyingType(dstType) }));
                                        }

                                        il.Emit(OpCodes.Callvirt, pc.PropertyInfo.GetSetMethod(true));		// poco
                                        Handled = true;
                                    }
                                }

                                // Not so fast
                                if (!Handled)
                                {
                                    // Setup stack for call to converter
                                    AddConverterToStack(il, converter);

                                    // "value = rdr.GetValue(i)"
                                    il.Emit(OpCodes.Ldarg_0);										// *,rdr
                                    il.Emit(OpCodes.Ldc_I4, i);										// *,rdr,i
                                    il.Emit(OpCodes.Callvirt, fnGetValue);							// *,value

                                    // Call the converter
                                    if (converter != null)
                                        il.Emit(OpCodes.Callvirt, fnInvoke);

                                    // Assign it
                                    il.Emit(OpCodes.Unbox_Any, pc.PropertyInfo.PropertyType);		// poco,poco,value
                                    il.Emit(OpCodes.Callvirt, pc.PropertyInfo.GetSetMethod(true));		// poco
                                }

                                il.MarkLabel(lblNext);
                            }

                            var fnOnLoaded = RecurseInheritedTypes<MethodInfo>(type, (x) => x.GetMethod("OnLoaded", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[0], null));
                            if (fnOnLoaded != null)
                            {
                                il.Emit(OpCodes.Dup);
                                il.Emit(OpCodes.Callvirt, fnOnLoaded);
                            }
                        }

                    il.Emit(OpCodes.Ret);

                    // Cache it, return it
                    var del = m.CreateDelegate(Expression.GetFuncType(typeof(IDataReader), type));
                    PocoFactories.Add(key, del);
                    return del;
                }
                finally
                {
                    RWLock.ExitWriteLock();
                }
            }

            private static void AddConverterToStack(ILGenerator il, Func<object, object> converter)
            {
                if (converter != null)
                {
                    // Add the converter
                    int converterIndex = m_Converters.Count;
                    m_Converters.Add(converter);

                    // Generate IL to push the converter onto the stack
                    il.Emit(OpCodes.Ldsfld, fldConverters);
                    il.Emit(OpCodes.Ldc_I4, converterIndex);
                    il.Emit(OpCodes.Callvirt, fnListGetItem);					// Converter
                }
            }

            public static Func<object, object> GetConverter(bool forceDateTimesToUtc, PocoColumn pc, Type srcType, Type dstType)
            {
                Func<object, object> converter = null;

                // Get converter from the mapper
                if (Database.Mapper != null)
                {
                    if (pc != null)
                    {
                        converter = Database.Mapper.GetFromDbConverter(pc.PropertyInfo, srcType);
                    }
                    else
                    {
                        var m2 = Database.Mapper as IMapper2;
                        if (m2 != null)
                        {
                            converter = m2.GetFromDbConverter(dstType, srcType);
                        }
                    }
                }

                // Standard DateTime->Utc mapper
                if (forceDateTimesToUtc && converter == null && srcType == typeof(DateTime) && (dstType == typeof(DateTime) || dstType == typeof(DateTime?)))
                {
                    converter = delegate(object src) { return new DateTime(((DateTime)src).Ticks, DateTimeKind.Utc); };
                }

                // Forced type conversion including integral types -> enum
                if (converter == null)
                {
                    if (dstType.IsEnum && IsIntegralType(srcType))
                    {
                        if (srcType != typeof(int))
                        {
                            converter = delegate(object src) { return Convert.ChangeType(src, typeof(int), null); };
                        }
                    }
                    else if (!dstType.IsAssignableFrom(srcType))
                    {
                        converter = delegate(object src) { return Convert.ChangeType(src, dstType, null); };
                    }
                }
                return converter;
            }


            static T RecurseInheritedTypes<T>(Type t, Func<Type, T> cb)
            {
                while (t != null)
                {
                    T info = cb(t);
                    if (info != null)
                        return info;
                    t = t.BaseType;
                }
                return default(T);
            }


            static Dictionary<Type, PocoData> m_PocoDatas = new Dictionary<Type, PocoData>();
            static List<Func<object, object>> m_Converters = new List<Func<object, object>>();
            static MethodInfo fnGetValue = typeof(IDataRecord).GetMethod("GetValue", new Type[] { typeof(int) });
            static MethodInfo fnIsDBNull = typeof(IDataRecord).GetMethod("IsDBNull");
            static FieldInfo fldConverters = typeof(PocoData).GetField("m_Converters", BindingFlags.Static | BindingFlags.GetField | BindingFlags.NonPublic);
            static MethodInfo fnListGetItem = typeof(List<Func<object, object>>).GetProperty("Item").GetGetMethod();
            static MethodInfo fnInvoke = typeof(Func<object, object>).GetMethod("Invoke");
            public Type type;
            public string[] QueryColumns { get; private set; }
            public TableInfo TableInfo { get; private set; }
            public Dictionary<string, PocoColumn> Columns { get; private set; }
            Dictionary<string, Delegate> PocoFactories = new Dictionary<string, Delegate>();
        }

        // Member variables
        string _connectionString;
        string _providerName;
        DbProviderFactory _factory;
        IDbConnection _sharedConnection;
        IDbTransaction _transaction;
        int _transactionDepth;
        bool _transactionCancelled;
        string _lastSql;
        object[] _lastArgs;
        string _paramPrefix = "@";
    }

    // Transaction object helps maintain transaction depth counts
    public interface ITransaction : IDisposable
    {
        void Complete();
    }

    public class Transaction : ITransaction
    {
        public Transaction(Database db) : this(db, null) { }

        public Transaction(Database db, IsolationLevel? isolationLevel)
        {
            _db = db;
            _db.BeginTransaction(isolationLevel);
        }

        public virtual void Complete()
        {
            _db.CompleteTransaction();
            _db = null;
        }

        public void Dispose()
        {
            if (_db != null)
                _db.AbortTransaction();
        }

        Database _db;
    }

    // Simple helper class for building SQL statments
    public class Sql
    {
        public Sql()
        {
        }

        public Sql(string sql, params object[] args)
        {
            _sql = sql;
            _args = args;
        }

        public static Sql Builder
        {
            get { return new Sql(); }
        }

        string _sql;
        object[] _args;
        Sql _rhs;
        string _sqlFinal;
        object[] _argsFinal;

        private void Build()
        {
            // already built?
            if (_sqlFinal != null)
                return;

            // Build it
            var sb = new StringBuilder();
            var args = new List<object>();
            Build(sb, args, null);
            _sqlFinal = sb.ToString();
            _argsFinal = args.ToArray();
        }

        public string SQL
        {
            get
            {
                Build();
                return _sqlFinal;
            }
        }

        public object[] Arguments
        {
            get
            {
                Build();
                return _argsFinal;
            }
        }

        public Sql Append(Sql sql)
        {
            if (_rhs != null)
                _rhs.Append(sql);
            else
                _rhs = sql;

            return this;
        }

        public Sql Append(string sql, params object[] args)
        {
            return Append(new Sql(sql, args));
        }

        static bool Is(Sql sql, string sqltype)
        {
            return sql != null && sql._sql != null && sql._sql.StartsWith(sqltype, StringComparison.InvariantCultureIgnoreCase);
        }

        private void Build(StringBuilder sb, List<object> args, Sql lhs)
        {
            if (!String.IsNullOrEmpty(_sql))
            {
                // Add SQL to the string
                if (sb.Length > 0)
                {
                    sb.Append("\n");
                }

                var sql = Database.ProcessParams(_sql, _args, args);

                if (Is(lhs, "WHERE ") && Is(this, "WHERE "))
                    sql = "AND " + sql.Substring(6);
                if (Is(lhs, "ORDER BY ") && Is(this, "ORDER BY "))
                    sql = ", " + sql.Substring(9);

                sb.Append(sql);
            }

            // Now do rhs
            if (_rhs != null)
                _rhs.Build(sb, args, this);
        }

        public Sql Where(string sql, params object[] args)
        {
            return Append(new Sql("WHERE (" + sql + ")", args));
        }

        public Sql OrderBy(params object[] columns)
        {
            return Append(new Sql("ORDER BY " + String.Join(", ", (from x in columns select x.ToString()).ToArray())));
        }

        public Sql Select(params object[] columns)
        {
            return Append(new Sql("SELECT " + String.Join(", ", (from x in columns select x.ToString()).ToArray())));
        }

        public Sql From(params object[] tables)
        {
            return Append(new Sql("FROM " + String.Join(", ", (from x in tables select x.ToString()).ToArray())));
        }

        public Sql GroupBy(params object[] columns)
        {
            return Append(new Sql("GROUP BY " + String.Join(", ", (from x in columns select x.ToString()).ToArray())));
        }

        private SqlJoinClause Join(string JoinType, string table)
        {
            return new SqlJoinClause(Append(new Sql(JoinType + table)));
        }

        public SqlJoinClause InnerJoin(string table) { return Join("INNER JOIN ", table); }
        public SqlJoinClause LeftJoin(string table) { return Join("LEFT JOIN ", table); }

        public class SqlJoinClause
        {
            private readonly Sql _sql;

            public SqlJoinClause(Sql sql)
            {
                _sql = sql;
            }

            public Sql On(string onClause, params object[] args)
            {
                return _sql.Append("ON " + onClause, args);
            }
        }
    }

}