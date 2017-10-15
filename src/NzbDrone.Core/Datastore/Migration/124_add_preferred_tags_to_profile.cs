using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;
using System.Data;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(124)]
     public class add_preferred_tags_to_profile : NzbDroneMigrationBase
     {
         protected override void MainDbUpgrade()
         {
             Alter.Table("Profiles").AddColumn("PreferredTags").AsString().Nullable();
         }

     }
}
