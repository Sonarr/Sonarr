using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(148)]
    public class mediainfo_channels : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE EpisodeFiles SET MediaInfo = Replace(MediaInfo, '\"audioChannels\"', '\"audioChannelsContainer\"');");
            Execute.Sql("UPDATE EpisodeFiles SET MediaInfo = Replace(MediaInfo, '\"audioChannelPositionsText\"', '\"audioChannelPositionsTextContainer\"');");
        }
    }
}
