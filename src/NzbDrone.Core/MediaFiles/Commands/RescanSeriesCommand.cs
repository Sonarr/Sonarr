using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.MediaFiles.Commands
{
    public class RescanSeriesCommand : Command
    {
        public int? SeriesId { get; set; }

        public override bool SendUpdatesToClient => true;

        public RescanSeriesCommand()
        {
        }

        public RescanSeriesCommand(int seriesId)
        {
            SeriesId = seriesId;
        }
    }
}
