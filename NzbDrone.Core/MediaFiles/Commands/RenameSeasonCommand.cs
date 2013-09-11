using NzbDrone.Core.Messaging;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.MediaFiles.Commands
{
    public class RenameSeasonCommand : Command
    {
        public int SeriesId { get; set; }
        public int SeasonNumber { get; set; }

        public override bool SendUpdatesToClient
        {
            get
            {
                return true;
            }
        }

        public RenameSeasonCommand(int seriesId, int seasonNumber)
        {
            SeriesId = seriesId;
            SeasonNumber = seasonNumber;
        }
    }
}