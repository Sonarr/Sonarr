using FluentMigrator;
using FluentMigrator.Infrastructure;
using NzbDrone.Core.Datastore.Migration.Framework;
using System.Data;
using System.Data.SQLite;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(128)]
    public class rename_quality_profiles_add_upgrade_allowed : NzbDroneMigrationBase
    {
        private IMigrationContext _capturedContext;

        protected override void MainDbUpgrade()
        {
            FixupMigration111();

            Rename.Table("Profiles").To("QualityProfiles");

            Alter.Table("QualityProfiles").AddColumn("UpgradeAllowed").AsInt32().Nullable();
            Alter.Table("LanguageProfiles").AddColumn("UpgradeAllowed").AsInt32().Nullable();

            // Set upgrade allowed for existing profiles (default will be false for new profiles)
            Update.Table("QualityProfiles").Set(new { UpgradeAllowed = true }).AllRows();
            Update.Table("LanguageProfiles").Set(new { UpgradeAllowed = true }).AllRows();

            Rename.Column("ProfileId").OnTable("Series").To("QualityProfileId");
        }

        public override void GetUpExpressions(IMigrationContext context)
        {
            _capturedContext = context;

            base.GetUpExpressions(context);

            _capturedContext = null;
        }

        // Early Radarr versions can mess up Sonarr's database if they point to the same config. Fixup the damage.        
        private void FixupMigration111()
        {
            var builder = new SQLiteConnectionStringBuilder(ConnectionString);
            builder.Pooling = false;

            // In order to get the expressions we need to check the database directly
            using (var connection = new SQLiteConnection(builder.ToString()))
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = "SELECT Description FROM VersionInfo WHERE Version = 111";

                var description = command.ExecuteScalar() as string;
                connection.Close();

                if (description == "remove_bitmetv")
                {
                    // Get the migration expressions from the 111 migration
                    var migration111 = new create_language_profiles();
                    migration111.GetUpExpressions(_capturedContext);

                    Execute.Sql("UPDATE VersionInfo SET Description = 'create_language_profiles fixup' WHERE Version = 111");
                }
            }
        }
    }
}
