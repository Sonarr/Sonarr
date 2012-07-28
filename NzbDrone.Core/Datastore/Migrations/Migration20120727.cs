using System;
using System.Data;
using Migrator.Framework;
using NzbDrone.Common;

namespace NzbDrone.Core.Datastore.Migrations
{

    [Migration(20120727)]
    public class Migration20120727 : NzbDroneMigration
    {
        protected override void MainDbUpgrade()
        {
            Database.ExecuteNonQuery(@"DELETE FROM History
                                        WHERE HistoryId IN
                                        (
                                            SELECT History.HistoryId
                                            FROM History
                                            LEFT OUTER JOIN Episodes
                                            ON History.EpisodeId = Episodes.EpisodeId
                                            WHERE Episodes.Title is null
                                        )");
            }
    }
}