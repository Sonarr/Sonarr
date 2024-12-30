using FluentMigrator;
using Workarr.Datastore.Migrations.Framework;

namespace Workarr.Datastore.Migrations
{
    [Migration(4)]
    public class updated_history : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.Table("History");

            MigrationExtension.TableForModel(Create, "History")
                  .WithColumn("EpisodeId").AsInt32()
                  .WithColumn("SeriesId").AsInt32()
                  .WithColumn("SourceTitle").AsString()
                  .WithColumn("Date").AsDateTime()
                  .WithColumn("Quality").AsString()
                  .WithColumn("Data").AsString();
        }
    }
}
