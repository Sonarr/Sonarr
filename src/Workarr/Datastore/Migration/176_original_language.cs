using FluentMigrator;
using Workarr.Datastore.Migrations.Framework;
using Workarr.Languages;

namespace Workarr.Datastore.Migrations
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
