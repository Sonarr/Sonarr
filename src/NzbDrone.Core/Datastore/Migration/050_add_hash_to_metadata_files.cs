using NzbDrone.Core.Datastore.Migration.Framework;
using FluentMigrator;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(50)]
    public class add_hash_to_metadata_files : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("MetadataFiles").AddColumn("Hash").AsString().Nullable();
        }
    }
}
