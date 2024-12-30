using FluentMigrator;
using Workarr.Datastore.Migrations.Framework;

namespace Workarr.Datastore.Migrations
{
    [Migration(28)]
    public class add_blacklist_table : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            MigrationExtension.TableForModel(Create, "Blacklist")
                .WithColumn("SeriesId").AsInt32()
                .WithColumn("EpisodeIds").AsString()
                .WithColumn("SourceTitle").AsString()
                .WithColumn("Quality").AsString()
                .WithColumn("Date").AsDateTime();
        }
    }
}
