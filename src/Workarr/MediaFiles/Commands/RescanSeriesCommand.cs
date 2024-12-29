using Workarr.Messaging.Commands;

namespace Workarr.MediaFiles.Commands
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
