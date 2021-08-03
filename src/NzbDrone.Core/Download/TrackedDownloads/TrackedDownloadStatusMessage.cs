using System.Collections.Generic;

namespace NzbDrone.Core.Download.TrackedDownloads
{
    public class TrackedDownloadStatusMessage
    {
        public string Title { get; set; }
        public List<string> Messages { get; set; }

        public TrackedDownloadStatusMessage(string title, List<string> messages)
        {
            Title = title;
            Messages = messages;
        }

        public TrackedDownloadStatusMessage(string title, string message)
        {
            Title = title;
            Messages = new List<string> { message };
        }

        //Constructor for use when deserializing JSON
        private TrackedDownloadStatusMessage()
        {
        }
    }
}
