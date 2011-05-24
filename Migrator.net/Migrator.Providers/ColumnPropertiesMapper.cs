using System;
using System.Collections.Generic;
using Migrator.Framework;

namespace Migrator.Providers
{
    /// <summary>
    /// This is basically a just a helper base class
    /// per-database implementors may want to override ColumnSql
    /// </summary>
    public class ColumnPropertiesMapper
    {
        protected Dialect dialect;
        
        /// <summary>The SQL type</summary>
        protected string type;

        /// <summary>The name of the column</summary>
        protected string name;

        /// <summary>
        /// the type of the column
        /// </summary>
        protected string columnSql;

        /// <summary>
        /// Sql if This column is Indexed
        /// </summary>
        protected bool indexed = false;

        /// <summary>
        /// Sql if this column has a default value
        /// </summary>
        protected object defaultVal;

        public ColumnPropertiesMapper(Dialect dialect, string type)
        {
            this.dialect = dialect;
            this.type = type;
        }

        /// <summary>
        /// The sql for this column, override in database-specific implementation classes
        /// </summary>
        public virtual string ColumnSql
        {
            get { return columnSql; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        
        public object Default
        {
             get { return defaultVal; }
             set { defaultVal = value; }
         }

        public string QuotedName
        {
            get { return dialect.Quote(Name); }
        }

        public string IndexSql
        {
            get
            {
                if (dialect.SupportsIndex && indexed)
                    return String.Format("INDEX({0})", dialect.Quote(name));
                return null;
            }
        }

        public void MapColumnProperties(Column column)
        {
            Name = column.Name;
            indexed = PropertySelected(column.ColumnProperty, ColumnProperty.Indexed);
            
            List<string> vals = new List<string>();
            vals.Add(dialect.ColumnNameNeedsQuote ? QuotedName : Name);
            
            vals.Add(type);
            
            if (! dialect.IdentityNeedsType)
                AddValueIfSelected(column, ColumnProperty.Identity, vals);
                
            AddValueIfSelected(column, ColumnProperty.Unsigned, vals);
            if (! PropertySelected(column.ColumnProperty, ColumnProperty.PrimaryKey) || dialect.NeedsNotNullForIdentity)
                AddValueIfSelected(column, ColumnProperty.NotNull, vals);
                
            AddValueIfSelected(column, ColumnProperty.PrimaryKey, vals);
            
            if (dialect.IdentityNeedsType)
                AddValueIfSelected(column, ColumnProperty.Identity, vals);
            
            AddValueIfSelected(column, ColumnProperty.Unique, vals);
            AddValueIfSelected(column, ColumnProperty.ForeignKey, vals);

            if (column.DefaultValue != null)
                vals.Add(dialect.Default(column.DefaultValue));

            columnSql = String.Join(" ", vals.ToArray());
        }

        private void AddValueIfSelected(Column column, ColumnProperty property, ICollection<string> vals)
        {
            if (PropertySelected(column.ColumnProperty, property))
                vals.Add(dialect.SqlForProperty(property));
        }

        public static bool PropertySelected(ColumnProperty source, ColumnProperty comparison)
        {
            return (source & comparison) == comparison;
        }
    }
}
