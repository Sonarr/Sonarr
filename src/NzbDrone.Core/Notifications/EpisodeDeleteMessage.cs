using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications
{
    public class EpisodeDeleteMessage
    {
        public string Message { get; set; }
        public Series Series { get; set; }
        public EpisodeFile EpisodeFile { get; set; }

        public DeleteMediaFileReason Reason { get; set; }

        public override string ToString()
        {
            return Message;
        }
    }
}
