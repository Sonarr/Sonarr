using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(241)]
    public class add_oidc_logouts : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("OidcLogouts")
                .WithColumn("SessionId").AsString().Nullable()
                .WithColumn("Subject").AsString().Nullable()
                .WithColumn("Timestamp").AsDateTimeOffset();
        }
    }
}
