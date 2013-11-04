using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(28)]
    public class add_blacklist_table : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("Blacklist")
                .WithColumn("SeriesId").AsInt32()
                .WithColumn("EpisodeIds").AsString()
                .WithColumn("SourceTitle").AsString()
                .WithColumn("Quality").AsString()
                .WithColumn("Date").AsDateTime();
        }
    }
}
