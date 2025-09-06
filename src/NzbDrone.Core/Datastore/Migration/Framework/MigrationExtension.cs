using System.Data;
using FluentMigrator;
using FluentMigrator.Builders.Create;
using FluentMigrator.Builders.Create.Table;
using FluentMigrator.Runner;
using FluentMigrator.Runner.BatchParser;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.SQLite;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.SQLite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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

        public static void AddParameter(this IDbCommand command, object value)
        {
            var parameter = command.CreateParameter();
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }

        public static IMigrationRunnerBuilder AddNzbDroneSQLite(this IMigrationRunnerBuilder builder, bool binaryGuid = false, bool useStrictTables = false)
        {
            builder.Services
                .AddTransient<SQLiteBatchParser>()
                .AddScoped<SQLiteDbFactory>()
                .AddScoped<NzbDroneSQLiteProcessor>(sp =>
                {
                    var factory = sp.GetService<SQLiteDbFactory>();
                    var logger = sp.GetService<ILogger<NzbDroneSQLiteProcessor>>();
                    var options = sp.GetService<IOptionsSnapshot<ProcessorOptions>>();
                    var connectionStringAccessor = sp.GetService<IConnectionStringAccessor>();
                    var sqliteQuoter = new SQLiteQuoter(false);
                    return new NzbDroneSQLiteProcessor(factory, sp.GetService<SQLiteGenerator>(), logger, options, connectionStringAccessor, sp, sqliteQuoter);
                })
                .AddScoped<ISQLiteTypeMap>(_ => new NzbDroneSQLiteTypeMap(useStrictTables))
                .AddScoped<IMigrationProcessor>(sp => sp.GetRequiredService<NzbDroneSQLiteProcessor>())
                .AddScoped(
                    sp =>
                    {
                        var typeMap = sp.GetRequiredService<ISQLiteTypeMap>();
                        return new SQLiteGenerator(
                            new SQLiteQuoter(binaryGuid),
                            typeMap,
                            new OptionsWrapper<GeneratorOptions>(new GeneratorOptions()));
                    })
                .AddScoped<IMigrationGenerator>(sp => sp.GetRequiredService<SQLiteGenerator>());

            return builder;
        }
    }
}
