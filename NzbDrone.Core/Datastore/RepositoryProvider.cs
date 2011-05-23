using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Migrator.Providers;
using Migrator.Providers.SQLite;
using SubSonic.DataProviders;
using SubSonic.Extensions;
using SubSonic.Schema;
using Migrator.Framework;


namespace NzbDrone.Core.Datastore
{
    public class RepositoryProvider
    {
        public virtual IList<Type> GetRepositoryTypes()
        {
            var coreAssembly = Assembly.GetExecutingAssembly();
            var repoTypes = coreAssembly.GetTypes().Where(t => !String.IsNullOrWhiteSpace(t.Namespace) && t.Namespace.StartsWith("NzbDrone.Core.Repository"));

            repoTypes = repoTypes.Where(r => !r.IsEnum);
            return repoTypes.ToList();
        }

        public virtual ITable GetSchemaFromType(Type type)
        {
            return type.ToSchemaTable(Connection.MainDataProvider);
        }

        public virtual Column[] GetColumnsFromDatabase(string connectionString, string tableName)
        {
            var dialact = new SQLiteDialect();
            var mig = new SQLiteTransformationProvider(dialact, connectionString);

            return mig.GetColumns(tableName);
        }


        public virtual List<Column> GetDeletedColumns(ITable typeSchema, Column[] dbColumns)
        {
            var deleteColumns = new List<Column>();
            foreach (var dbColumn in dbColumns)
            {
                if (!typeSchema.Columns.ToList().Exists(c => c.Name == dbColumn.Name.Trim('[', ']')))
                {
                    deleteColumns.Add(dbColumn);
                }
            }

            return deleteColumns;
        }


        public virtual List<Column> GetNewColumns(ITable typeSchema, Column[] dbColumns)
        {
            var newColumns = new List<Column>();
            foreach (var typeColumn in typeSchema.Columns)
            {
                if (!dbColumns.ToList().Exists(c => c.Name.Trim('[', ']') == typeColumn.Name))
                {
                    newColumns.Add(ConvertToMigratorColumn(typeColumn));
                }
            }

            return newColumns;
        }

        public virtual Column ConvertToMigratorColumn(SubSonic.Schema.IColumn subsonicColumns)
        {
            var migColumn = new Column(subsonicColumns.Name, subsonicColumns.DataType);

            if (subsonicColumns.IsPrimaryKey)
            {
                migColumn.ColumnProperty = ColumnProperty.PrimaryKey;
            }

            if (subsonicColumns.IsNullable)
            {
                migColumn.ColumnProperty = migColumn.ColumnProperty | ColumnProperty.Null;
            }
            else
            {
                migColumn.ColumnProperty = migColumn.ColumnProperty | ColumnProperty.NotNull;
                migColumn.DefaultValue = false;
            }

            return migColumn;
        }
    }
}
