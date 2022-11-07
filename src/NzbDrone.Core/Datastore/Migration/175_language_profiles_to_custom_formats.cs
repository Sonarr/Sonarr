using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(175)]
    public class language_profiles_to_custom_formats : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("EpisodeFiles")
                 .AddColumn("Languages").AsString().NotNullable().WithDefaultValue("[]");

            Alter.Table("History")
                 .AddColumn("Languages").AsString().NotNullable().WithDefaultValue("[]");

            Alter.Table("Blocklist")
                 .AddColumn("Languages").AsString().NotNullable().WithDefaultValue("[]");

            // Migrate Language to Languages in all tables
            Execute.Sql("UPDATE EpisodeFiles SET Languages = '[' || Language || ']'");
            Execute.Sql("UPDATE History SET Languages = '[' || Language || ']'");
            Execute.Sql("UPDATE Blocklist SET Languages = '[' || Language || ']'");

            // Migrate Language Profiles to CFs

            Delete.Column("Language").FromTable("EpisodeFiles");
            Delete.Column("Language").FromTable("History");
            Delete.Column("Language").FromTable("Blocklist");

            Delete.Column("LanguageProfileId").FromTable("Series");
            Delete.Column("LanguageProfileId").FromTable("ImportLists");

            Delete.Table("LanguageProfiles");
        }
    }
}
