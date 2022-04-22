﻿using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(10)]
    public class add_monitored : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Episodes").AddColumn("Monitored").AsBoolean().Nullable();
            Alter.Table("Seasons").AddColumn("Monitored").AsBoolean().Nullable();

            IfDatabase("sqlite").Execute.Sql("UPDATE Episodes SET Monitored = 1 WHERE Ignored = 0");
            IfDatabase("sqlite").Execute.Sql("UPDATE Episodes SET Monitored = 0 WHERE Ignored = 1");

            IfDatabase("sqlite").Execute.Sql("UPDATE Seasons SET Monitored = 1 WHERE Ignored = 0");
            IfDatabase("sqlite").Execute.Sql("UPDATE Seasons SET Monitored = 0 WHERE Ignored = 1");
        }
    }
}
