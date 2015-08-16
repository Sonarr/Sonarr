using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.MediaFiles.Commands.Series
{
    public class RescanSeriesCommand : Command
    {
        public int? SeriesId { get; set; }

        public override bool SendUpdatesToClient
        {
            get
            {
                return true;
            }
        }

        public RescanSeriesCommand()
        {
        }

        public RescanSeriesCommand(int seriesId)
        {
            SeriesId = seriesId;
        }
    }
}