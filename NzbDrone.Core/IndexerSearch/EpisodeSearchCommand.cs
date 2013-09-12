using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.IndexerSearch
{
    public class EpisodeSearchCommand : Command
    {
        public int EpisodeId { get; set; }

        public override bool SendUpdatesToClient
        {
            get
            {
                return true;
            }
        }
    }
}