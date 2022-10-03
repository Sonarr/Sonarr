using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Core.Languages;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(176)]
    public class original_language : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Series")
                .AddColumn("OriginalLanguage").AsInt32().WithDefaultValue((int)Language.English);
        }
    }
}
