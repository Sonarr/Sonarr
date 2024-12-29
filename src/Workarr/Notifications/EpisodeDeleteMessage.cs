using Workarr.MediaFiles;
using Workarr.Tv;

namespace Workarr.Notifications
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
