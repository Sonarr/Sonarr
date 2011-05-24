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

namespace Migrator.Framework
{
    /// <summary>
    /// Represents a table column.
    /// </summary>
    public class Column : IColumn
    {
        private string _name;
        private DbType _type;
        private int _size;
        private ColumnProperty _property;
        private object _defaultValue;

		public Column(string name)
		{
			Name = name;
		}

    	public Column(string name, DbType type)
        {
            Name = name;
            Type = type;
        }

        public Column(string name, DbType type, int size)
        {
            Name = name;
            Type = type;
            Size = size;
        }

		public Column(string name, DbType type, object defaultValue)
		{
			Name = name;
			Type = type;
			DefaultValue = defaultValue;
		}

    	public Column(string name, DbType type, ColumnProperty property)
        {
            Name = name;
            Type = type;
            ColumnProperty = property;
        }

        public Column(string name, DbType type, int size, ColumnProperty property)
        {
            Name = name;
            Type = type;
            Size = size;
            ColumnProperty = property;
        }

        public Column(string name, DbType type, int size, ColumnProperty property, object defaultValue)
        {
            Name = name;
            Type = type;
            Size = size;
            ColumnProperty = property;
            DefaultValue = defaultValue;
        }

        public Column(string name, DbType type, ColumnProperty property, object defaultValue)
        {
            Name = name;
            Type = type;
            ColumnProperty = property;
            DefaultValue = defaultValue;
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public DbType Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public int Size
        {
            get { return _size; }
            set { _size = value; }
        }

        public ColumnProperty ColumnProperty
        {
            get { return _property; }
            set { _property = value; }
        }

        public object DefaultValue
        {
            get { return _defaultValue; }
            set { _defaultValue = value; }
        }
        
        public bool IsIdentity 
        {
            get { return (ColumnProperty & ColumnProperty.Identity) == ColumnProperty.Identity; }
        }
        
        public bool IsPrimaryKey 
        {
            get { return (ColumnProperty & ColumnProperty.PrimaryKey) == ColumnProperty.PrimaryKey; }
        }
    }
}
