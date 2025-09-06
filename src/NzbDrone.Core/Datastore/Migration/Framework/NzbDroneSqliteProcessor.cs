using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.SQLite;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.SQLite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NzbDrone.Core.Datastore.Migration.Framework
{
    public class NzbDroneSQLiteProcessor : SQLiteProcessor
    {
        private readonly SQLiteQuoter _quoter;

        public NzbDroneSQLiteProcessor(SQLiteDbFactory factory,
                                       SQLiteGenerator generator,
                                       ILogger<NzbDroneSQLiteProcessor> logger,
                                       IOptionsSnapshot<ProcessorOptions> options,
                                       IConnectionStringAccessor connectionStringAccessor,
                                       IServiceProvider serviceProvider,
                                       SQLiteQuoter quoter)
        : base(factory, generator, logger, options, connectionStringAccessor, serviceProvider, quoter)
        {
            _quoter = quoter;
        }

        public override void Process(AlterColumnExpression expression)
        {
            var tableDefinition = GetTableSchema(expression.TableName);

            var columnDefinitions = tableDefinition.Columns.ToList();
            var columnIndex = columnDefinitions.FindIndex(c => c.Name == expression.Column.Name);

            if (columnIndex == -1)
            {
                throw new ApplicationException($"Column {expression.Column.Name} does not exist on table {expression.TableName}.");
            }

            columnDefinitions[columnIndex] = expression.Column;

            tableDefinition.Columns = columnDefinitions;

            ProcessAlterTable(tableDefinition);
        }

        public override void Process(AlterDefaultConstraintExpression expression)
        {
            var tableDefinition = GetTableSchema(expression.TableName);

            var columnDefinitions = tableDefinition.Columns.ToList();
            var columnIndex = columnDefinitions.FindIndex(c => c.Name == expression.ColumnName);

            if (columnIndex == -1)
            {
                throw new ApplicationException($"Column {expression.ColumnName} does not exist on table {expression.TableName}.");
            }

            var changedColumn = columnDefinitions[columnIndex];
            changedColumn.DefaultValue = expression.DefaultValue;

            columnDefinitions[columnIndex] = changedColumn;

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
                throw new ApplicationException($"Column {columnsToRemove.First()} does not exist on table {expression.TableName}.");
            }

            ProcessAlterTable(tableDefinition);
        }

        public override void Process(RenameColumnExpression expression)
        {
            var tableDefinition = GetTableSchema(expression.TableName);

            var oldColumnDefinitions = tableDefinition.Columns.ToList();
            var columnDefinitions = tableDefinition.Columns.ToList();
            var columnIndex = columnDefinitions.FindIndex(c => c.Name == expression.OldName);

            if (columnIndex == -1)
            {
                throw new ApplicationException($"Column {expression.OldName} does not exist on table {expression.TableName}.");
            }

            if (columnDefinitions.Any(c => c.Name == expression.NewName))
            {
                throw new ApplicationException($"Column {expression.NewName} already exists on table {expression.TableName}.");
            }

            oldColumnDefinitions[columnIndex] = (ColumnDefinition)columnDefinitions[columnIndex].Clone();
            columnDefinitions[columnIndex].Name = expression.NewName;

            foreach (var index in tableDefinition.Indexes)
            {
                if (index.Name.StartsWith("IX_"))
                {
                    index.Name = Regex.Replace(index.Name, "(?<=_)" + Regex.Escape(expression.OldName) + "(?=_|$)", Regex.Escape(expression.NewName));
                }

                foreach (var column in index.Columns)
                {
                    if (column.Name == expression.OldName)
                    {
                        column.Name = expression.NewName;
                    }
                }
            }

            ProcessAlterTable(tableDefinition, oldColumnDefinitions);
        }

        protected virtual TableDefinition GetTableSchema(string tableName)
        {
            var schemaDumper = new SqliteSchemaDumper(this);
            var schema = schemaDumper.ReadDbSchema();

            return schema.Single(v => v.Name == tableName);
        }

        protected virtual void ProcessAlterTable(TableDefinition tableDefinition, List<ColumnDefinition> oldColumnDefinitions = null)
        {
            var tableName = tableDefinition.Name;
            var tempTableName = tableName + "_temp";

            var uid = 0;
            while (TableExists(null, tempTableName))
            {
                tempTableName = tableName + "_temp" + uid++;
            }

            // What is the cleanest way to do this? Add function to Generator?
            var columnsToInsert = string.Join(", ", tableDefinition.Columns.Select(c => _quoter.QuoteColumnName(c.Name)));
            var columnsToFetch = string.Join(", ", (oldColumnDefinitions ?? tableDefinition.Columns).Select(c => _quoter.QuoteColumnName(c.Name)));

            Process(new CreateTableExpression { TableName = tempTableName, Columns = tableDefinition.Columns.ToList() });

            Process($"INSERT INTO {_quoter.QuoteTableName(tempTableName)} ({columnsToInsert}) SELECT {columnsToFetch} FROM {_quoter.QuoteTableName(tableName)}");

            Process(new DeleteTableExpression { TableName = tableName });

            Process(new RenameTableExpression { OldName = tempTableName, NewName = tableName });

            foreach (var index in tableDefinition.Indexes)
            {
                Process(new CreateIndexExpression { Index = index });
            }
        }
    }
}
