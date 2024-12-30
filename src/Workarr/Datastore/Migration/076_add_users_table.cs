using FluentMigrator;
using Workarr.Datastore.Migrations.Framework;

namespace Workarr.Datastore.Migrations
{
    [Migration(76)]
    public class add_users_table : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            MigrationExtension.TableForModel(Create, "Users")
                  .WithColumn("Identifier").AsString().NotNullable().Unique()
                  .WithColumn("Username").AsString().NotNullable().Unique()
                  .WithColumn("Password").AsString().NotNullable();
        }
    }
}
