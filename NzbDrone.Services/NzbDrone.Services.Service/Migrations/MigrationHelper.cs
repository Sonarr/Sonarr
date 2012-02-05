using System;
using System.Data;
using System.Reflection;
using System.Web.Hosting;
using Migrator.Framework;
using NLog;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Services.Service.Migrations
{
    public class MigrationsHelper
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static void Run(string connetionString)
        {
            logger.Info("Preparing to run database migration");

            VerifyConnectionString(connetionString);

            try
            {
                var migrator = new Migrator.Migrator("sqlserver", connetionString, Assembly.GetAssembly(typeof(MigrationsHelper)), true, new MigrationLogger());
                migrator.MigrateToLastVersion();
                logger.Info("Database migration completed");
            }
            catch (Exception e)
            {
                logger.FatalException("An error has occurred while migrating database", e);
            }
        }

        private static void VerifyConnectionString(string connectionString)
        {
            if(connectionString == null) throw new ArgumentNullException("connectionString");

            if (HostingEnvironment.ApplicationPhysicalPath != null && HostingEnvironment.ApplicationPhysicalPath.ToLower().Contains("stage") &&
                !connectionString.ToLower().Contains("stage"))
            {
                throw new InvalidOperationException("Attempting to migrate production database from staging environment");
            }
        }

        public static string GetIndexName(string tableName, params string[] columns)
        {
            return String.Format("IX_{0}_{1}", tableName, String.Join("_", columns));
        }

        public static readonly Column VersionColumn = new Column("Version", DbType.String, 50, ColumnProperty.NotNull);
        public static readonly Column ProductionColumn = new Column("IsProduction", DbType.Boolean, ColumnProperty.NotNull);
        public static readonly Column TimestampColumn = new Column("TimeStamp", DbType.DateTime, ColumnProperty.NotNull);
        public static readonly Column UGuidColumn = new Column("UGuid", DbType.Guid, ColumnProperty.Null);

    }
}