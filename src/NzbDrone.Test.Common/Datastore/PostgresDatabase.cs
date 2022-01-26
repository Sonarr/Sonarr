using System;
using Npgsql;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Test.Common.Datastore
{
    public static class PostgresDatabase
    {
        public static PostgresOptions GetTestOptions()
        {
            var options = PostgresOptions.GetOptions();

            var uid = TestBase.GetUID();
            options.MainDb = uid + "_main";
            options.LogDb = uid + "_log";

            return options;
        }

        public static void Create(PostgresOptions options, MigrationType migrationType)
        {
            var db = GetDatabaseName(options, migrationType);
            var connectionString = GetConnectionString(options);
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"CREATE DATABASE \"{db}\" WITH OWNER = {options.User} ENCODING = 'UTF8' CONNECTION LIMIT = -1;";
            cmd.ExecuteNonQuery();
        }

        public static void Drop(PostgresOptions options, MigrationType migrationType)
        {
            var db = GetDatabaseName(options, migrationType);
            var connectionString = GetConnectionString(options);
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"DROP DATABASE \"{db}\" WITH (FORCE);";
            cmd.ExecuteNonQuery();
        }

        private static string GetConnectionString(PostgresOptions options)
        {
            var builder = new NpgsqlConnectionStringBuilder()
            {
                Host = options.Host,
                Port = options.Port,
                Username = options.User,
                Password = options.Password,
                Enlist = false
            };

            return builder.ConnectionString;
        }

        private static string GetDatabaseName(PostgresOptions options, MigrationType migrationType)
        {
            return migrationType switch
            {
                MigrationType.Main => options.MainDb,
                MigrationType.Log => options.LogDb,
                _ => throw new NotImplementedException("Unknown migration type")
            };
        }
    }
}
