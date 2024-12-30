using FluentMigrator;
using Workarr.Datastore.Migrations.Framework;

namespace Workarr.Datastore.Migrations
{
    [Migration(141)]
    public class add_update_history : NzbDroneMigrationBase
    {
        protected override void LogDbUpgrade()
        {
            MigrationExtension.TableForModel(Create, "UpdateHistory")
                  .WithColumn("Date").AsDateTime().NotNullable().Indexed()
                  .WithColumn("Version").AsString().NotNullable()
                  .WithColumn("EventType").AsInt32().NotNullable();
        }
    }
}
