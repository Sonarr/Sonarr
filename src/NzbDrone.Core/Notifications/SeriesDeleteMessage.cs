using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications
{
    public class SeriesDeleteMessage
    {
        public string Message { get; set; }
        public Series Series { get; set; }

        public bool DeleteFiles { get; set; }

        public override string ToString()
        {
            return Message;
        }
    }
}