using FluentMigrator;
using Workarr.Datastore.Migrations.Framework;

namespace Workarr.Datastore.Migrations
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
