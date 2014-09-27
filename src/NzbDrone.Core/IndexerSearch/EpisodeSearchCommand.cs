using System;
using System.Collections.Generic;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.IndexerSearch
{
    public class EpisodeSearchCommand : Command
    {
        public List<Int32> EpisodeIds { get; set; }

        public override bool SendUpdatesToClient
        {
            get
            {
                return true;
            }
        }

        public EpisodeSearchCommand()
        {
        }

        public EpisodeSearchCommand(List<Int32> episodeIds)
        {
            EpisodeIds = episodeIds;
        }
    }
}