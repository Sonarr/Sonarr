#region License

//The contents of this file are subject to the Mozilla Public License
//Version 1.1 (the "License"); you may not use this file except in
//compliance with the License. You may obtain a copy of the License at
//http://www.mozilla.org/MPL/
//Software distributed under the License is distributed on an "AS IS"
//basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
//License for the specific language governing rights and limitations
//under the License.

#endregion

using System;
using System.Collections.Generic;
using System.Data;
using Migrator.Framework;
using Migrator.Framework.SchemaBuilder;
using ForeignKeyConstraint = Migrator.Framework.ForeignKeyConstraint;
using Migrator.Framework.Loggers;

namespace Migrator.Providers
{
	/// <summary>
	/// Base class for every transformation providers.
	/// A 'tranformation' is an operation that modifies the database.
	/// </summary>
	public abstract class TransformationProvider : ITransformationProvider
	{
		private ILogger _logger;
		protected IDbConnection _connection;
		private IDbTransaction _transaction;
		private List<long> _appliedMigrations;

		protected readonly string _connectionString;
		protected Dialect _dialect;

		private readonly ForeignKeyConstraintMapper constraintMapper = new ForeignKeyConstraintMapper();

		protected TransformationProvider(Dialect dialect, string connectionString)
		{
			_dialect = dialect;
			_connectionString = connectionString;
			_logger = new Logger(false);
		}

		/// <summary>
		/// Returns the event logger
		/// </summary>
		public virtual ILogger Logger
		{
			get { return _logger; }
			set { _logger = value; }
		}

		public Dialect Dialect
		{
			get { return _dialect; }
		}

		public ITransformationProvider this[string provider]
		{
			get
			{
				if (null != provider && IsThisProvider(provider))
					return this;

				return NoOpTransformationProvider.Instance;
			}
		}

		public bool IsThisProvider(string provider)
		{
			// XXX: This might need to be more sophisticated. Currently just a convention
			return GetType().Name.ToLower().StartsWith(provider.ToLower());
		}

		public virtual Column[] GetColumns(string table)
		{
			List<Column> columns = new List<Column>();
			using (
				IDataReader reader =
					ExecuteQuery(
						String.Format("select COLUMN_NAME, IS_NULLABLE from information_schema.columns where table_name = '{0}'", table)))
			{
				while (reader.Read())
				{
					Column column = new Column(reader.GetString(0), DbType.String);
					string nullableStr = reader.GetString(1);
					bool isNullable = nullableStr == "YES";
					column.ColumnProperty |= isNullable ? ColumnProperty.Null : ColumnProperty.NotNull;

					columns.Add(column);
				}
			}

			return columns.ToArray();
		}

		public virtual Column GetColumnByName(string table, string columnName)
		{
			return Array.Find(GetColumns(table),
				delegate(Column column)
				{
					return column.Name == columnName;
				});
		}

		public virtual string[] GetTables()
		{
			List<string> tables = new List<string>();
			using (IDataReader reader = ExecuteQuery("SELECT table_name FROM information_schema.tables"))
			{
				while (reader.Read())
				{
					tables.Add((string)reader[0]);
				}
			}
			return tables.ToArray();
		}

		public virtual void RemoveForeignKey(string table, string name)
		{
			RemoveConstraint(table, name);
		}

		public virtual void RemoveConstraint(string table, string name)
		{
			if (TableExists(table) && ConstraintExists(table, name))
			{
				table = _dialect.TableNameNeedsQuote ? _dialect.Quote(table) : table;
				name = _dialect.ConstraintNameNeedsQuote ? _dialect.Quote(name) : name;
				ExecuteNonQuery(String.Format("ALTER TABLE {0} DROP CONSTRAINT {1}", table, name));
			}
		}

		public virtual void AddTable(string table, string engine, string columns)
		{
			table = _dialect.TableNameNeedsQuote ? _dialect.Quote(table) : table;
			string sqlCreate = String.Format("CREATE TABLE {0} ({1})", table, columns);
			ExecuteNonQuery(sqlCreate);
		}

		/// <summary>
		/// Add a new table
		/// </summary>
		/// <param name="name">Table name</param>
		/// <param name="columns">Columns</param>
		/// <example>
		/// Adds the Test table with two columns:
		/// <code>
		/// Database.AddTable("Test",
		///	                  new Column("Id", typeof(int), ColumnProperty.PrimaryKey),
		///	                  new Column("Title", typeof(string), 100)
		///	                 );
		/// </code>
		/// </example>
		public virtual void AddTable(string name, params Column[] columns)
		{
            // Most databases don't have the concept of a storage engine, so default is to not use it.
            AddTable(name, null, columns);
		}

        /// <summary>
        /// Add a new table
        /// </summary>
        /// <param name="name">Table name</param>
        /// <param name="columns">Columns</param>
        /// <param name="engine">the database storage engine to use</param>
        /// <example>
        /// Adds the Test table with two columns:
        /// <code>
        /// Database.AddTable("Test", "INNODB",
        ///	                  new Column("Id", typeof(int), ColumnProperty.PrimaryKey),
        ///	                  new Column("Title", typeof(string), 100)
        ///	                 );
        /// </code>
        /// </example>
        public virtual void AddTable(string name, string engine, params Column[] columns)
        {

            if (TableExists(name))
            {
                Logger.Warn("Table {0} already exists", name);
                return;
            }

            List<string> pks = GetPrimaryKeys(columns);
            bool compoundPrimaryKey = pks.Count > 1;

            List<ColumnPropertiesMapper> columnProviders = new List<ColumnPropertiesMapper>(columns.Length);
            foreach (Column column in columns)
            {
                // Remove the primary key notation if compound primary key because we'll add it back later
                if (compoundPrimaryKey && column.IsPrimaryKey)
                    column.ColumnProperty = ColumnProperty.Unsigned | ColumnProperty.NotNull;

                ColumnPropertiesMapper mapper = _dialect.GetAndMapColumnProperties(column);
                columnProviders.Add(mapper);
            }

            string columnsAndIndexes = JoinColumnsAndIndexes(columnProviders);
            AddTable(name, engine, columnsAndIndexes);

            if (compoundPrimaryKey)
            {
                AddPrimaryKey(String.Format("PK_{0}", name), name, pks.ToArray());
            }
        }

		public List<string> GetPrimaryKeys(IEnumerable<Column> columns)
		{
			List<string> pks = new List<string>();
			foreach (Column col in columns)
			{
				if (col.IsPrimaryKey)
					pks.Add(col.Name);
			}
			return pks;
		}

		public virtual void RemoveTable(string name)
		{
			if (TableExists(name))
				ExecuteNonQuery(String.Format("DROP TABLE {0}", name));
		}

		public virtual void RenameTable(string oldName, string newName)
		{
			if (TableExists(newName))
				throw new MigrationException(String.Format("Table with name '{0}' already exists", newName));

			if (TableExists(oldName))
				ExecuteNonQuery(String.Format("ALTER TABLE {0} RENAME TO {1}", oldName, newName));
		}

		public virtual void RenameColumn(string tableName, string oldColumnName, string newColumnName)
		{
			if (ColumnExists(tableName, newColumnName))
				throw new MigrationException(String.Format("Table '{0}' has column named '{1}' already", tableName, newColumnName));

			if (ColumnExists(tableName, oldColumnName))
				ExecuteNonQuery(String.Format("ALTER TABLE {0} RENAME COLUMN {1} TO {2}", tableName, oldColumnName, newColumnName));
		}

		public virtual void AddColumn(string table, string sqlColumn)
		{
			ExecuteNonQuery(String.Format("ALTER TABLE {0} ADD COLUMN {1}", table, sqlColumn));
		}

		public virtual void RemoveColumn(string table, string column)
		{
			if (ColumnExists(table, column))
			{
				ExecuteNonQuery(String.Format("ALTER TABLE {0} DROP COLUMN {1} ", table, column));
			}
		}

		public virtual bool ColumnExists(string table, string column)
		{
			try
			{
				ExecuteNonQuery(String.Format("SELECT {0} FROM {1}", column, table));
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public virtual void ChangeColumn(string table, Column column)
		{
			if (!ColumnExists(table, column.Name))
			{
				Logger.Warn("Column {0}.{1} does not exist", table, column.Name);
				return;
			}

			ColumnPropertiesMapper mapper = _dialect.GetAndMapColumnProperties(column);
			ChangeColumn(table, mapper.ColumnSql);
		}

		public virtual void ChangeColumn(string table, string sqlColumn)
		{
			ExecuteNonQuery(String.Format("ALTER TABLE {0} ALTER COLUMN {1}", table, sqlColumn));
		}

		public virtual bool TableExists(string table)
		{
			try
			{
				ExecuteNonQuery("SELECT COUNT(*) FROM " + table);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		protected virtual string JoinColumnsAndIndexes(IEnumerable<ColumnPropertiesMapper> columns)
		{
			string indexes = JoinIndexes(columns);
			string columnsAndIndexes = JoinColumns(columns) + (indexes != null ? "," + indexes : String.Empty);
			return columnsAndIndexes;
		}

		protected virtual string JoinIndexes(IEnumerable<ColumnPropertiesMapper> columns)
		{
			List<string> indexes = new List<string>();
			foreach (ColumnPropertiesMapper column in columns)
			{
				string indexSql = column.IndexSql;
				if (indexSql != null)
					indexes.Add(indexSql);
			}

			if (indexes.Count == 0)
				return null;

			return String.Join(", ", indexes.ToArray());
		}

		protected virtual string JoinColumns(IEnumerable<ColumnPropertiesMapper> columns)
		{
			List<string> columnStrings = new List<string>();
			foreach (ColumnPropertiesMapper column in columns)
				columnStrings.Add(column.ColumnSql);
			return String.Join(", ", columnStrings.ToArray());
		}

		/// <summary>
		/// Add a new column to an existing table.
		/// </summary>
		/// <param name="table">Table to which to add the column</param>
		/// <param name="column">Column name</param>
		/// <param name="type">Date type of the column</param>
		/// <param name="size">Max length of the column</param>
		/// <param name="property">Properties of the column, see <see cref="ColumnProperty">ColumnProperty</see>,</param>
		/// <param name="defaultValue">Default value</param>
		public virtual void AddColumn(string table, string column, DbType type, int size, ColumnProperty property,
                                      object defaultValue)
		{
			if (ColumnExists(table, column))
			{
				Logger.Warn("Column {0}.{1} already exists", table, column);
				return;
			}

			ColumnPropertiesMapper mapper =
				_dialect.GetAndMapColumnProperties(new Column(column, type, size, property, defaultValue));

			AddColumn(table, mapper.ColumnSql);
		}

		/// <summary>
		/// <see cref="TransformationProvider.AddColumn(string, string, DbType, int, ColumnProperty, object)">
		/// AddColumn(string, string, Type, int, ColumnProperty, object)
		/// </see>
		/// </summary>
		public virtual void AddColumn(string table, string column, DbType type)
		{
			AddColumn(table, column, type, 0, ColumnProperty.Null, null);
		}

		/// <summary>
		/// <see cref="TransformationProvider.AddColumn(string, string, DbType, int, ColumnProperty, object)">
		/// AddColumn(string, string, Type, int, ColumnProperty, object)
		/// </see>
		/// </summary>
		public virtual void AddColumn(string table, string column, DbType type, int size)
		{
			AddColumn(table, column, type, size, ColumnProperty.Null, null);
		}

        public void AddColumn(string table, string column, DbType type, object defaultValue)
        {
            if (ColumnExists(table, column))
            {
                Logger.Warn("Column {0}.{1} already exists", table, column);
                return;
            }

            ColumnPropertiesMapper mapper =
                _dialect.GetAndMapColumnProperties(new Column(column, type, defaultValue));

            AddColumn(table, mapper.ColumnSql);
            
        }

		/// <summary>
		/// <see cref="TransformationProvider.AddColumn(string, string, DbType, int, ColumnProperty, object)">
		/// AddColumn(string, string, Type, int, ColumnProperty, object)
		/// </see>
		/// </summary>
		public virtual void AddColumn(string table, string column, DbType type, ColumnProperty property)
		{
			AddColumn(table, column, type, 0, property, null);
		}

		/// <summary>
		/// <see cref="TransformationProvider.AddColumn(string, string, DbType, int, ColumnProperty, object)">
		/// AddColumn(string, string, Type, int, ColumnProperty, object)
		/// </see>
		/// </summary>
		public virtual void AddColumn(string table, string column, DbType type, int size, ColumnProperty property)
		{
			AddColumn(table, column, type, size, property, null);
		}

		/// <summary>
		/// Append a primary key to a table.
		/// </summary>
		/// <param name="name">Constraint name</param>
		/// <param name="table">Table name</param>
		/// <param name="columns">Primary column names</param>
		public virtual void AddPrimaryKey(string name, string table, params string[] columns)
		{
			if (ConstraintExists(table, name))
			{
				Logger.Warn("Primary key {0} already exists", name);
				return;
			}
			ExecuteNonQuery(
				String.Format("ALTER TABLE {0} ADD CONSTRAINT {1} PRIMARY KEY ({2}) ", table, name,
							  String.Join(",", columns)));
		}

		public virtual void AddUniqueConstraint(string name, string table, params string[] columns)
		{
			if (ConstraintExists(table, name))
			{
				Logger.Warn("Constraint {0} already exists", name);
				return;
			}
			ExecuteNonQuery(String.Format("ALTER TABLE {0} ADD CONSTRAINT {1} UNIQUE({2}) ", table, name, string.Join(", ", columns)));
		}

		public virtual void AddCheckConstraint(string name, string table, string checkSql)
		{
			if (ConstraintExists(table, name))
			{
				Logger.Warn("Constraint {0} already exists", name);
				return;
			}
			ExecuteNonQuery(String.Format("ALTER TABLE {0} ADD CONSTRAINT {1} CHECK ({2}) ", table, name, checkSql));
		}

		/// <summary>
		/// Guesses the name of the foreign key and add it
		/// </summary>
		public virtual void GenerateForeignKey(string primaryTable, string primaryColumn, string refTable, string refColumn)
		{
			AddForeignKey("FK_" + primaryTable + "_" + refTable, primaryTable, primaryColumn, refTable, refColumn);
		}

		/// <summary>
		/// Guesses the name of the foreign key and add it
		/// </see>
		/// </summary>
		public virtual void GenerateForeignKey(string primaryTable, string[] primaryColumns, string refTable,
                                               string[] refColumns)
		{
			AddForeignKey("FK_" + primaryTable + "_" + refTable, primaryTable, primaryColumns, refTable, refColumns);
		}

		/// <summary>
		/// Guesses the name of the foreign key and add it
		/// </summary>
		public virtual void GenerateForeignKey(string primaryTable, string primaryColumn, string refTable,
                                               string refColumn, ForeignKeyConstraint constraint)
		{
			AddForeignKey("FK_" + primaryTable + "_" + refTable, primaryTable, primaryColumn, refTable, refColumn,
						  constraint);
		}

		/// <summary>
		/// Guesses the name of the foreign key and add it
		/// </see>
		/// </summary>
		public virtual void GenerateForeignKey(string primaryTable, string[] primaryColumns, string refTable,
                                               string[] refColumns, ForeignKeyConstraint constraint)
		{
			AddForeignKey("FK_" + primaryTable + "_" + refTable, primaryTable, primaryColumns, refTable, refColumns,
						  constraint);
		}

		/// <summary>
		/// Append a foreign key (relation) between two tables.
		/// tables.
		/// </summary>
		/// <param name="name">Constraint name</param>
		/// <param name="primaryTable">Table name containing the primary key</param>
		/// <param name="primaryColumn">Primary key column name</param>
		/// <param name="refTable">Foreign table name</param>
		/// <param name="refColumn">Foreign column name</param>
		public virtual void AddForeignKey(string name, string primaryTable, string primaryColumn, string refTable,
                                          string refColumn)
		{
			AddForeignKey(name, primaryTable, new string[] { primaryColumn }, refTable, new string[] { refColumn });
		}

		/// <summary>
		/// <see cref="ITransformationProvider.AddForeignKey(string, string, string, string, string)">
		/// AddForeignKey(string, string, string, string, string)
		/// </see>
		/// </summary>
		public virtual void AddForeignKey(string name, string primaryTable, string[] primaryColumns, string refTable, string[] refColumns)
		{
			AddForeignKey(name, primaryTable, primaryColumns, refTable, refColumns, ForeignKeyConstraint.NoAction);
		}

		public virtual void AddForeignKey(string name, string primaryTable, string primaryColumn, string refTable, string refColumn, ForeignKeyConstraint constraint)
		{
			AddForeignKey(name, primaryTable, new string[] { primaryColumn }, refTable, new string[] { refColumn },
						  constraint);
		}

		public virtual void AddForeignKey(string name, string primaryTable, string[] primaryColumns, string refTable,
                                          string[] refColumns, ForeignKeyConstraint constraint)
		{
			if (ConstraintExists(primaryTable, name))
			{
				Logger.Warn("Constraint {0} already exists", name);
				return;
			}

			string constraintResolved = constraintMapper.SqlForConstraint(constraint);
			ExecuteNonQuery(
				String.Format(
					"ALTER TABLE {0} ADD CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES {3} ({4}) ON UPDATE {5} ON DELETE {6}",
					primaryTable, name, String.Join(",", primaryColumns),
					refTable, String.Join(",", refColumns), constraintResolved, constraintResolved));
		}

		/// <summary>
		/// Determines if a constraint exists.
		/// </summary>
		/// <param name="name">Constraint name</param>
		/// <param name="table">Table owning the constraint</param>
		/// <returns><c>true</c> if the constraint exists.</returns>
		public abstract bool ConstraintExists(string table, string name);

		public virtual bool PrimaryKeyExists(string table, string name)
		{
			return ConstraintExists(table, name);
		}

		public int ExecuteNonQuery(string sql)
		{
            Logger.Trace(sql);
            Logger.ApplyingDBChange(sql);
            using (IDbCommand cmd = BuildCommand(sql))
            {
                try
                {
                    return cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex.Message);
                    throw;
                }
            }		}

		private IDbCommand BuildCommand(string sql)
		{
		    IDbCommand cmd = _connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;
            if (_transaction != null)
            {
                cmd.Transaction = _transaction;
            }
            return cmd;
		}

		/// <summary>
		/// Execute an SQL query returning results.
		/// </summary>
		/// <param name="sql">The SQL command.</param>
		/// <returns>A data iterator, <see cref="System.Data.IDataReader">IDataReader</see>.</returns>
		public IDataReader ExecuteQuery(string sql)
		{
            Logger.Trace(sql);
            using (IDbCommand cmd = BuildCommand(sql))
            {
                try
                {
                    return cmd.ExecuteReader();
                }
                catch
                {
                    Logger.Warn("query failed: {0}", cmd.CommandText);
                    throw;
                }
            }		}

		public object ExecuteScalar(string sql)
		{
            Logger.Trace(sql);
            using (IDbCommand cmd = BuildCommand(sql))
            {
                try
                {
                    return cmd.ExecuteScalar();
                }
                catch
                {
                    Logger.Warn("Query failed: {0}", cmd.CommandText);
                    throw;
                }
            }		}

		public IDataReader Select(string what, string from)
		{
			return Select(what, from, "1=1");
		}

		public virtual IDataReader Select(string what, string from, string where)
		{
			return ExecuteQuery(String.Format("SELECT {0} FROM {1} WHERE {2}", what, from, where));
		}

		public object SelectScalar(string what, string from)
		{
			return SelectScalar(what, from, "1=1");
		}

		public virtual object SelectScalar(string what, string from, string where)
		{
			return ExecuteScalar(String.Format("SELECT {0} FROM {1} WHERE {2}", what, from, where));
		}

		public virtual int Update(string table, string[] columns, string[] values)
		{
			return Update(table, columns, values, null);
		}

		public virtual int Update(string table, string[] columns, string[] values, string where)
		{
            string namesAndValues = JoinColumnsAndValues(columns, values);

		    string query = "UPDATE {0} SET {1}";
			if (!String.IsNullOrEmpty(where))
			{
				query += " WHERE " + where;
			}

			return ExecuteNonQuery(String.Format(query, table, namesAndValues));
		}

	    public virtual int Insert(string table, string[] columns, string[] values)
		{
			return ExecuteNonQuery(String.Format("INSERT INTO {0} ({1}) VALUES ({2})", table, String.Join(", ", columns), String.Join(", ", QuoteValues(values))));
		}

        public virtual int Delete(string table)
        {
            return Delete(table, (string[])null, (string[]) null);
        }

        public virtual int Delete(string table, string[] columns, string[] values)
        {
            if (null == columns || null == values)
            {
                return ExecuteNonQuery(String.Format("DELETE FROM {0}", table));
            }
            else
            {
                return ExecuteNonQuery(String.Format("DELETE FROM {0} WHERE ({1})", table, JoinColumnsAndValues(columns, values)));
            }
        }

		public virtual int Delete(string table, string wherecolumn, string wherevalue)
		{
			return ExecuteNonQuery(String.Format("DELETE FROM {0} WHERE {1} = {2}", table, wherecolumn, QuoteValues(wherevalue)));
		}

		/// <summary>
		/// Starts a transaction. Called by the migration mediator.
		/// </summary>
		public void BeginTransaction()
		{
			if (_transaction == null && _connection != null)
			{
				EnsureHasConnection();
				_transaction = _connection.BeginTransaction(IsolationLevel.ReadCommitted);
			}
		}

		protected void EnsureHasConnection()
		{
			if (_connection.State != ConnectionState.Open)
			{
				_connection.Open();
			}
		}

		/// <summary>
		/// Rollback the current migration. Called by the migration mediator.
		/// </summary>
		public virtual void Rollback()
		{
			if (_transaction != null && _connection != null && _connection.State == ConnectionState.Open)
			{
				try
				{
					_transaction.Rollback();
				}
				finally
				{
					_connection.Close();
				}
			}
			_transaction = null;
		}

		/// <summary>
		/// Commit the current transaction. Called by the migrations mediator.
		/// </summary>
		public void Commit()
		{
			if (_transaction != null && _connection != null && _connection.State == ConnectionState.Open)
			{
				try
				{
					_transaction.Commit();
				}
				finally
				{
					_connection.Close();
				}
			}
			_transaction = null;
		}

        /// <summary>
        /// The list of Migrations currently applied to the database.
        /// </summary>
		public List<long> AppliedMigrations
		{
			get
			{
				if(_appliedMigrations == null)
				{
					_appliedMigrations = new List<long>();
					CreateSchemaInfoTable();
					using(IDataReader reader = Select("version","SchemaInfo")){
						while(reader.Read()){
                            _appliedMigrations.Add(Convert.ToInt64(reader.GetValue(0)));
						}
					}
				}
				return _appliedMigrations;
			}
		}
		
		/// <summary>
        /// Marks a Migration version number as having been applied
        /// </summary>
        /// <param name="version">The version number of the migration that was applied</param>
		public void MigrationApplied(long version)
		{
			CreateSchemaInfoTable();
			Insert("SchemaInfo",new string[]{"version"},new string[]{version.ToString()});
			_appliedMigrations.Add(version);
		}
		
        /// <summary>
        /// Marks a Migration version number as having been rolled back from the database
        /// </summary>
        /// <param name="version">The version number of the migration that was removed</param>
		public void MigrationUnApplied(long version)
		{
			CreateSchemaInfoTable();
			Delete("SchemaInfo", "version", version.ToString());
			_appliedMigrations.Remove(version);
		}

		protected void CreateSchemaInfoTable()
		{
			EnsureHasConnection();
			if (!TableExists("SchemaInfo"))
			{
				AddTable("SchemaInfo", new Column("Version", DbType.Int64, ColumnProperty.PrimaryKey));
			}
		}

		public void AddColumn(string table, Column column)
		{
			AddColumn(table, column.Name, column.Type, column.Size, column.ColumnProperty, column.DefaultValue);
		}

		public void GenerateForeignKey(string primaryTable, string refTable)
		{
			GenerateForeignKey(primaryTable, refTable, ForeignKeyConstraint.NoAction);
		}

		public void GenerateForeignKey(string primaryTable, string refTable, ForeignKeyConstraint constraint)
		{
			GenerateForeignKey(primaryTable, refTable + "Id", refTable, "Id", constraint);
		}

		public IDbCommand GetCommand()
		{
			return BuildCommand(null);
		}

		public void ExecuteSchemaBuilder(SchemaBuilder builder)
		{
			foreach (ISchemaBuilderExpression expr in builder.Expressions)
				expr.Create(this);
		}

        public virtual string QuoteValues(string values)
        {
            return QuoteValues(new string[] {values})[0];
        }

        public virtual string[] QuoteValues(string[] values)
        {
            return Array.ConvertAll<string, string>(values,
				      delegate(string val) {
				        if (null == val)
				          return "null";
				        else
				          return String.Format("'{0}'", val.Replace("'", "''")); 
				  });
        }

        public string JoinColumnsAndValues(string[] columns, string[] values)
        {
            string[] quotedValues = QuoteValues(values);
            string[] namesAndValues = new string[columns.Length];
            for (int i = 0; i < columns.Length; i++)
            {
                namesAndValues[i] = String.Format("{0}={1}", columns[i], quotedValues[i]);
            }

            return String.Join(", ", namesAndValues);
        }

        public void Dispose()
        {
            if (_connection != null && _connection.State == ConnectionState.Open)
            {
                _connection.Close();
            }
        }
	}
}
