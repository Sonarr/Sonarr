using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(83)]
    public class additonal_blacklist_columns : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Blacklist").AddColumn("Size").AsInt64().Nullable();
            Alter.Table("Blacklist").AddColumn("Protocol").AsInt32().Nullable();
            Alter.Table("Blacklist").AddColumn("Indexer").AsString().Nullable();
            Alter.Table("Blacklist").AddColumn("Message").AsString().Nullable();
            Alter.Table("Blacklist").AddColumn("TorrentInfoHash").AsString().Nullable();

            Update.Table("Blacklist")
                  .Set(new { Protocol = 1 })
                  .AllRows();
        }
    }
}
