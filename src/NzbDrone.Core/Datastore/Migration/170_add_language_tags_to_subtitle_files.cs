using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(170)]
    public class add_language_tags_to_subtitle_files : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("SubtitleFiles").AddColumn("LanguageTags").AsString().Nullable();
        }
    }
}
