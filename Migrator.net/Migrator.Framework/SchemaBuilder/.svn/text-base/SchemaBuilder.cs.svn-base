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

namespace Migrator.Framework.SchemaBuilder
{
	public class SchemaBuilder : IColumnOptions, IForeignKeyOptions, IDeleteTableOptions
	{
		private string _currentTable;
		private IFluentColumn _currentColumn;
		private IList<ISchemaBuilderExpression> _exprs;

		public SchemaBuilder()
		{
			_exprs = new List<ISchemaBuilderExpression>();
		}

		public IEnumerable<ISchemaBuilderExpression> Expressions
		{
			get { return _exprs; }
		}

		/// <summary>
		/// Adds a Table to be created to the Schema
		/// </summary>
		/// <param name="name">Table name to be created</param>
		/// <returns>SchemaBuilder for chaining</returns>
		public SchemaBuilder AddTable(string name)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			_exprs.Add(new AddTableExpression(name));
			_currentTable = name;

			return this;
		}

		public IDeleteTableOptions DeleteTable(string name)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");
			_currentTable = "";
			_currentColumn = null;

			_exprs.Add(new DeleteTableExpression(name));

			return this;
		}

		/// <summary>
		/// Reference an existing table.
		/// </summary>
		/// <param name="newName">Table to reference</param>
		/// <returns>SchemaBuilder for chaining</returns>
		public SchemaBuilder RenameTable(string newName)
		{
			if (string.IsNullOrEmpty(newName))
				throw new ArgumentNullException("newName");

			_exprs.Add(new RenameTableExpression(_currentTable, newName));
			_currentTable = newName;

			return this;
		}

		/// <summary>
		/// Reference an existing table.
		/// </summary>
		/// <param name="name">Table to reference</param>
		/// <returns>SchemaBuilder for chaining</returns>
		public SchemaBuilder WithTable(string name)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			_currentTable = name;

			return this;
		}

		/// <summary>
		/// Adds a Column to be created
		/// </summary>
		/// <param name="name">Column name to be added</param>
		/// <returns>IColumnOptions to restrict chaining</returns>
		public IColumnOptions AddColumn(string name)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");
			if (string.IsNullOrEmpty(_currentTable))
				throw new ArgumentException("missing referenced table");

			IFluentColumn column = new FluentColumn(name);
			_currentColumn = column;

			_exprs.Add(new AddColumnExpression(_currentTable, column));
			return this;
		}

		public SchemaBuilder OfType(DbType columnType)
		{
			_currentColumn.Type = columnType;

			return this;
		}

		public SchemaBuilder WithProperty(ColumnProperty columnProperty)
		{
			_currentColumn.ColumnProperty = columnProperty;

			return this;
		}

		public SchemaBuilder WithSize(int size)
		{
			if (size == 0)
				throw new ArgumentNullException("size", "Size must be greater than zero");

			_currentColumn.Size = size;

			return this;
		}

		public SchemaBuilder WithDefaultValue(object defaultValue)
		{
			if (defaultValue == null)
				throw new ArgumentNullException("defaultValue", "DefaultValue cannot be null or empty");

			_currentColumn.DefaultValue = defaultValue;

			return this;
		}

		public IForeignKeyOptions AsForeignKey()
		{
			_currentColumn.ColumnProperty = ColumnProperty.ForeignKey;

			return this;
		}

		public SchemaBuilder ReferencedTo(string primaryKeyTable, string primaryKeyColumn)
		{
			_currentColumn.Constraint = ForeignKeyConstraint.NoAction;
			_currentColumn.ForeignKey = new ForeignKey(primaryKeyTable, primaryKeyColumn);
			return this;
		}

		public SchemaBuilder WithConstraint(ForeignKeyConstraint action)
		{
			_currentColumn.Constraint = action;

			return this;
		}
	}
}