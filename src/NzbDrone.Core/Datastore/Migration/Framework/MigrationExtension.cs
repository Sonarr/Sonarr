using System.Data;
using FluentMigrator;
using FluentMigrator.Builders.Create;
using FluentMigrator.Builders.Create.Table;
using FluentMigrator.Runner;
using FluentMigrator.Runner.BatchParser;
using FluentMigrator.Runner.Generators.SQLite;
using FluentMigrator.Runner.Processors.SQLite;
using Microsoft.Extensions.DependencyInjection;

namespace NzbDrone.Core.Datastore.Migration.Framework
{
    public static class MigrationExtension
    {
        public static ICreateTableColumnOptionOrWithColumnSyntax TableForModel(this ICreateExpressionRoot expressionRoot, string name)
        {
            return expressionRoot.Table(name).WithColumn("Id").AsInt32().PrimaryKey().Identity();
        }

        public static IDbCommand CreateCommand(this IDbConnection conn, IDbTransaction tran, string query)
        {
            var command = conn.CreateCommand();
            command.Transaction = tran;
            command.CommandText = query;

            return command;
        }

        public static void AddParameter(this System.Data.IDbCommand command, object value)
        {
            var parameter = command.CreateParameter();
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }

        public static IMigrationRunnerBuilder AddNzbDroneSQLite(this IMigrationRunnerBuilder builder)
        {
            builder.Services
                .AddTransient<SQLiteBatchParser>()
                .AddScoped<SQLiteDbFactory>()
                .AddScoped<NzbDroneSQLiteProcessor>()
                .AddScoped<IMigrationProcessor>(sp => sp.GetRequiredService<NzbDroneSQLiteProcessor>())
                .AddScoped<SQLiteQuoter>()
                .AddScoped<SQLiteGenerator>()
                .AddScoped<IMigrationGenerator>(sp => sp.GetRequiredService<SQLiteGenerator>());
            return builder;
        }
    }
}
