using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(4)]
    public class updated_history : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.Table("History");

            Create.TableForModel("History")
                  .WithColumn("EpisodeId").AsInt32()
                  .WithColumn("SeriesId").AsInt32()
                  .WithColumn("SourceTitle").AsString()
                  .WithColumn("Date").AsDateTime()
                  .WithColumn("Quality").AsString()
                  .WithColumn("Data").AsString();
        }
    }
}
