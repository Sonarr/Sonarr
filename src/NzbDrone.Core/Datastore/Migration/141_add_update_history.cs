using System;
using System.Collections.Generic;
using System.Data;
using FluentMigrator;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Converters;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(141)]
    public class add_update_history : NzbDroneMigrationBase
    {
        protected override void LogDbUpgrade()
        {
            Create.TableForModel("UpdateHistory")
                  .WithColumn("Date").AsDateTime().NotNullable().Indexed()
                  .WithColumn("Version").AsString().NotNullable()
                  .WithColumn("EventType").AsInt32().NotNullable();
        }
    }
}
