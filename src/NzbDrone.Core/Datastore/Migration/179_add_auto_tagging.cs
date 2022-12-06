using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(179)]
    public class add_auto_tagging : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("AutoTagging")
                .WithColumn("Name").AsString().Unique()
                .WithColumn("Specifications").AsString().WithDefaultValue("[]")
                .WithColumn("RemoveTagsAutomatically").AsBoolean().WithDefaultValue(false)
                .WithColumn("Tags").AsString().WithDefaultValue("[]");
        }
    }
}
