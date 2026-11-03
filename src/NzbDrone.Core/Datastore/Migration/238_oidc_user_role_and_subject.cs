using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(238)]
    public class oidc_user_role_and_subject : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Users").AddColumn("OidcSubject").AsString().Nullable();
            Alter.Table("Users").AddColumn("OidcGroups").AsString().Nullable();
            Alter.Table("Users").AddColumn("Role").AsInt32().NotNullable().WithDefaultValue(3);

            Execute.Sql("CREATE UNIQUE INDEX \"IX_Users_OidcSubject\" ON \"Users\" (\"OidcSubject\") WHERE \"OidcSubject\" IS NOT NULL");
        }
    }
}
