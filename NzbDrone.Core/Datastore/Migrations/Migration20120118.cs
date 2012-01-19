using Migrator.Framework;

namespace NzbDrone.Core.Datastore.Migrations
{

    [Migration(20120118)]
    public class Migration20120118 : NzbDroneMigration
    {
        protected override void MainDbUpgrade()
        {
            Database.ExecuteNonQuery("DELETE FROM SERIES WHERE  SeriesID = 0");
        }
    }
}