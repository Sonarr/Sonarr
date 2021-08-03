using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(73)]
    public class clear_ratings : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Update.Table("Series")
                  .Set(new { Ratings = "{}" })
                  .AllRows();

            Update.Table("Episodes")
                  .Set(new { Ratings = "{}" })
                  .AllRows();
        }
    }
}
