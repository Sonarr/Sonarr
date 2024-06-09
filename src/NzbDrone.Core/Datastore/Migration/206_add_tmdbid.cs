using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(206)]
    public class add_tmdbid : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Series").AddColumn("TmdbId").AsInt32().WithDefaultValue(0);
            Create.Index().OnTable("Series").OnColumn("TmdbId");
        }
    }
}
