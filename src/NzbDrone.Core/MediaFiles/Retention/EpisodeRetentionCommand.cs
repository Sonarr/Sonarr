using System.Collections.Generic;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.MediaFiles.Retention
{
    public class EpisodeRetentionCommand : Command
    {
        public int SeriesId { get; set; }
        public List<int> Files { get; set; }
        public List<int> EpisodeIds { get; set; }
        public override bool SendUpdatesToClient => true;
        public override bool RequiresDiskAccess => true;
        public override bool IsExclusive => true;
        public override string CompletionMessage => "Episode retention completed.";
    }
}
