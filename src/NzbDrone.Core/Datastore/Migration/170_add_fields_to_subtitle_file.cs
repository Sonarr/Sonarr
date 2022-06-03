using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(170)]
    public class add_fields_to_subtitle_file : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("SubtitleFiles").AddColumn("LanguageTags").AsString().Nullable();
            Alter.Table("SubtitleFiles").AddColumn("FullPath").AsString().Nullable();
        }
    }
}
