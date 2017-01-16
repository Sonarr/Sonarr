using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(63)]
    public class add_remotepathmappings : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("RemotePathMappings")
                  .WithColumn("Host").AsString()
                  .WithColumn("RemotePath").AsString()
                  .WithColumn("LocalPath").AsString();
        }
    }
}
