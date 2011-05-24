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

using System.Data;

namespace Migrator.Framework.SchemaBuilder
{
	public class FluentColumn : IFluentColumn
	{
		private Column _inner;
		private ForeignKeyConstraint _constraint;
		private ForeignKey _fk;

		public FluentColumn(string columnName)
		{
			_inner = new Column(columnName);
		}

		public ColumnProperty ColumnProperty
		{
			get { return _inner.ColumnProperty; }
			set { _inner.ColumnProperty = value; }
		}

		public string Name
		{
			get { return _inner.Name; }
			set { _inner.Name = value; }
		}

		public DbType Type
		{
			get { return _inner.Type; }
			set { _inner.Type = value; }
		}

		public int Size
		{
			get { return _inner.Size; }
			set { _inner.Size = value; }
		}

		public bool IsIdentity
		{
			get { return _inner.IsIdentity; }
		}

		public bool IsPrimaryKey
		{
			get { return _inner.IsPrimaryKey; }
		}

		public object DefaultValue
		{
			get { return _inner.DefaultValue; }
			set { _inner.DefaultValue = value; }
		}

		public ForeignKeyConstraint Constraint
		{
			get { return _constraint; }
			set { _constraint = value; }
		}
		public ForeignKey ForeignKey
		{
			get { return _fk; }
			set { _fk = value; }
		}
	}
}