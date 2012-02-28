using Migrator.Framework;

namespace NzbDrone.Core.Datastore.Migrations
{

    [Migration(20120227)]
    public class Migration20120227 : NzbDroneMigration
    {
        protected override void MainDbUpgrade()
        {
            Database.ExecuteNonQuery(@"DELETE FROM Seasons WHERE SeasonNumber NOT IN
                                            (
                                                SELECT DISTINCT SeasonNumber
                                                FROM Episodes
                                                WHERE Seasons.SeriesId = Episodes.SeriesId
                                            )");
        }
    }
}