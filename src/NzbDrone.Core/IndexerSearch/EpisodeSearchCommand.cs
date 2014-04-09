using System.Collections.Generic;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.IndexerSearch
{
    public class EpisodeSearchCommand : Command
    {
        public List<int> EpisodeIds { get; set; }

        public override bool SendUpdatesToClient
        {
            get
            {
                return true;
            }
        }
    }
}