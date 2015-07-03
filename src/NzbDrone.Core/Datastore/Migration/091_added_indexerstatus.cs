using System;
using System.Collections.Generic;
using System.Data;
using FluentMigrator;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(91)]
    public class added_indexerstatus : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("IndexerStatus")
                  .WithColumn("IndexerId").AsInt32().NotNullable().Unique()
                  .WithColumn("FirstFailure").AsDateTime().Nullable()
                  .WithColumn("LastFailure").AsDateTime().Nullable()
                  .WithColumn("FailureEscalation").AsInt32().NotNullable()
                  .WithColumn("BackOffDate").AsDateTime().Nullable()
                  .WithColumn("LastRecentSearch").AsDateTime().Nullable()
                  .WithColumn("LastRecentReleaseInfo").AsString().Nullable();
        }
    }
}
