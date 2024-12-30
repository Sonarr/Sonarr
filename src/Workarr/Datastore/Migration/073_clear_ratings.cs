using FluentMigrator;
using Workarr.Datastore.Migrations.Framework;

namespace Workarr.Datastore.Migrations
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
