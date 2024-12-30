using Workarr.Tv;

namespace Workarr.Notifications
{
    public class SeriesAddMessage
    {
        public string Message { get; set; }
        public Series Series { get; set; }

        public override string ToString()
        {
            return Message;
        }
    }
}
