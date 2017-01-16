using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.IndexerSearch
{
    public class MissingEpisodeSearchCommand : Command
    {
        public int? SeriesId { get; set; }

        public override bool SendUpdatesToClient => true;

        public MissingEpisodeSearchCommand()
        {
        }

        public MissingEpisodeSearchCommand(int seriesId)
        {
            SeriesId = seriesId;
        }
    }
}