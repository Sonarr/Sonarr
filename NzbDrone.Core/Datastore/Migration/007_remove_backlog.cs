using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
/*    [Tags("")]
    [Migration(7)]
    public class remove_backlog : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            var newSeriesTable = "CREATE TABLE [Series_new] ([Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT, [TvdbId] integer NOT NULL, " +
                                 "[TvRageId] integer NOT NULL, [ImdbId] text NOT NULL, [Title] text NOT NULL, [TitleSlug] text NOT NULL, " +
                                 "[CleanTitle] text NOT NULL, [Status] integer NOT NULL, [Overview] text, [AirTime] text, " +
                                 "[Images] text NOT NULL, [Path] text NOT NULL, [Monitored] integer NOT NULL, [QualityProfileId] integer NOT NULL, " +
                                 "[SeasonFolder] integer NOT NULL, [LastInfoSync] datetime, [LastDiskSync] datetime, [Runtime] integer NOT NULL, " +
                                 "[SeriesType] integer NOT NULL, [Network] text, [CustomStartDate] datetime, " +
                                 "[UseSceneNumbering] integer NOT NULL, [FirstAired] datetime)";

            Execute.Sql(newSeriesTable);


            Execute.Sql("INSERT INTO Series_new SELECT * FROM Series");
        }
    }*/
}
