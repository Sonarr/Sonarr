using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(158)]
    public class update_pendingreleases : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("PendingReleases")
                .AddColumn("MovieId").AsInt32().WithDefaultValue(0)
                .AddColumn("ParsedMovieInfo").AsString().Nullable()
                .AlterColumn("ParsedEpisodeInfo").AsString().Nullable();
        }
    }
}
