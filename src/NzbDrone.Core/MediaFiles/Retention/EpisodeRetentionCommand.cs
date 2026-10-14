using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.MediaFiles.Retention
{
    public class EpisodeRetentionCommand : Command
    {
        public int SeriesId { get; set; }
        public string PreviewToken { get; set; }
        public override bool RequiresDiskAccess => true;
    }
}
