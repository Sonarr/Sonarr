using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(19)]
    public class restore_unique_constraints : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            SqLiteAlter.AddIndexes("Series", 
                new SQLiteIndex { Column = "TvdbId", Table = "Series", Unique = true },
                new SQLiteIndex { Column = "TitleSlug", Table = "Series", Unique = true });

            SqLiteAlter.AddIndexes("Episodes", 
                new SQLiteIndex { Column = "TvDbEpisodeId", Table = "Episodes", Unique = true });
        }

    }
}
