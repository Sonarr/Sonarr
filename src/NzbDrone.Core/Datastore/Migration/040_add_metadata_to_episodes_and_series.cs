using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(40)]
    public class add_metadata_to_episodes_and_series : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Series")
                 .AddColumn("Actors").AsString().Nullable()
                 .AddColumn("Ratings").AsString().Nullable()
                 .AddColumn("Genres").AsString().Nullable()
                 .AddColumn("Certification").AsString().Nullable();

            Alter.Table("Episodes")
                 .AddColumn("Ratings").AsString().Nullable()
                 .AddColumn("Images").AsString().Nullable();
        }
    }
}
