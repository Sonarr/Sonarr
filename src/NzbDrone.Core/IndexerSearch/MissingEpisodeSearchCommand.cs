using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.IndexerSearch
{
    public class MissingEpisodeSearchCommand : Command
    {
        public int? SeriesId { get; private set; }

        public override bool SendUpdatesToClient
        {
            get
            {
                return true;
            }
        }

        public MissingEpisodeSearchCommand()
        {
        }

        public MissingEpisodeSearchCommand(int seriesId)
        {
            SeriesId = seriesId;
        }
    }
}