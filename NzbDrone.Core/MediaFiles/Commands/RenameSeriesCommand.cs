using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.MediaFiles.Commands
{
    public class RenameSeriesCommand : Command
    {
        public int SeriesId { get; set; }

        public override bool SendUpdatesToClient
        {
            get
            {
                return true;
            }
        }

        public RenameSeriesCommand()
        {
        }

        public RenameSeriesCommand(int seriesId)
        {
            SeriesId = seriesId;
        }
    }
}