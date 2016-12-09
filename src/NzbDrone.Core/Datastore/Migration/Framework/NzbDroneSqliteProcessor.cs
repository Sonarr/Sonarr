using System;
using System.Data;
using System.Linq;
using FluentMigrator;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators.SQLite;
using FluentMigrator.Runner.Processors.SQLite;

namespace NzbDrone.Core.Datastore.Migration.Framework
{
    public class NzbDroneSqliteProcessor : SQLiteProcessor
    {
        public NzbDroneSqliteProcessor(IDbConnection connection, IMigrationGenerator generator, IAnnouncer announcer, IMigrationProcessorOptions options, FluentMigrator.Runner.Processors.IDbFactory factory)
            : base(connection, generator, announcer, options, factory)
        {

        }

        public override bool SupportsTransactions => true;

        public override void Process(AlterColumnExpression expression)
        {
            var tableDefinition = GetTableSchema(expression.TableName);

            var columnDefinitions = tableDefinition.Columns.ToList();
            var columnIndex = columnDefinitions.FindIndex(c => c.Name == expression.Column.Name);

            if (columnIndex == -1)
            {
                throw new ApplicationException(string.Format("Column {0} does not exist on table {1}.", expression.Column.Name, expression.TableName));
            }

            columnDefinitions[columnIndex] = expression.Column;

            tableDefinition.Columns = columnDefinitions;

            ProcessAlterTable(tableDefinition);
        }

        public override void Process(DeleteColumnExpression expression)
        {
            var tableDefinition = GetTableSchema(expression.TableName);

            var columnDefinitions = tableDefinition.Columns.ToList();
            var indexDefinitions = tableDefinition.Indexes.ToList();

            var columnsToRemove = expression.ColumnNames.ToList();

            columnDefinitions.RemoveAll(c => columnsToRemove.Remove(c.Name));
            indexDefinitions.RemoveAll(i => i.Columns.Any(c => expression.ColumnNames.Contains(c.Name)));

            tableDefinition.Columns = columnDefinitions;
            tableDefinition.Indexes = indexDefinitions;

            if (columnsToRemove.Any())
            {
                throw new ApplicationException(string.Format("Column {0} does not exist on table {1}.", columnsToRemove.First(), expression.TableName));
            }

            ProcessAlterTable(tableDefinition);
        }

        protected virtual TableDefinition GetTableSchema(string tableName)
        {
            var schemaDumper = new  SqliteSchemaDumper(this, Announcer);
            var schema = schemaDumper.ReadDbSchema();

            return schema.Single(v => v.Name == tableName);
        }

        protected virtual void ProcessAlterTable(TableDefinition tableDefinition)
        {
            var tableName = tableDefinition.Name;
            var tempTableName = tableName + "_temp";

            var uid = 0;
            while (TableExists(null, tempTableName))
            {
                tempTableName = tableName + "_temp" + uid++;
            }

            // What is the cleanest way to do this? Add function to Generator?
            var quoter = new SQLiteQuoter();
            var columnsToTransfer = string.Join(", ", tableDefinition.Columns.Select(c => quoter.QuoteColumnName(c.Name)));

            Process(new CreateTableExpression() { TableName = tempTableName, Columns = tableDefinition.Columns.ToList() });

            Process(string.Format("INSERT INTO {0} SELECT {1} FROM {2}", quoter.QuoteTableName(tempTableName), columnsToTransfer, quoter.QuoteTableName(tableName)));

            Process(new DeleteTableExpression() { TableName = tableName });

            Process(new RenameTableExpression() { OldName = tempTableName, NewName = tableName });

            foreach (var index in tableDefinition.Indexes)
            {
                Process(new CreateIndexExpression() { Index = index });
            }
        }
    }
}
