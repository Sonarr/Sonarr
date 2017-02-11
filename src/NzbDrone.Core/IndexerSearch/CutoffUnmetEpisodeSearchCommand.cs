using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.IndexerSearch
{
    public class CutoffUnmetEpisodeSearchCommand : Command
    {
        public int? SeriesId { get; set; }

        public override bool SendUpdatesToClient
        {
            get
            {
                return true;
            }
        }

        public CutoffUnmetEpisodeSearchCommand()
        {
        }

        public CutoffUnmetEpisodeSearchCommand(int seriesId)
        {
            SeriesId = seriesId;
        }
    }
}