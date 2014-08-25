using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(24)]
    public class drop_tvdb_episodeid : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.Column("TvDbEpisodeId").FromTable("Episodes");
        }
    }
}
