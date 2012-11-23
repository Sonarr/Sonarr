using System;
using System.Data;
using Migrator.Framework;
using NzbDrone.Common;

namespace NzbDrone.Core.Datastore.Migrations
{
    [Migration(20121122)]
    public class Migration20121122 : NzbDroneMigration
    {
        protected override void MainDbUpgrade()
        {
            Database.ExecuteNonQuery("UPDATE Config SET [KEY] = 'SabBacklogTvPriority' WHERE [KEY] = 'SabTvPriority'");
            Database.ExecuteNonQuery("UPDATE Config SET [KEY] = 'DownloadClientTvDirectory' WHERE [KEY] = 'SabTvDropDirectory'");

            var priority = Database.ExecuteScalar("SELECT [Value] FROM Config WHERE [Key] = 'SabBacklogTvPriority'");

            if (priority != null)
                Database.ExecuteNonQuery("INSERT INTO Config ([Key], [Value]) VALUES('SabRecentTvPriority', '" + priority + "')");
        }
    }
}