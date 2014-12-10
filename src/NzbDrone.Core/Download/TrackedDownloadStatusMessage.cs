using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Download
{
    public class TrackedDownloadStatusMessage
    {
        public String Title { get; set; }
        public List<String> Messages { get; set; }

        private TrackedDownloadStatusMessage()
        {
        }

        public TrackedDownloadStatusMessage(String title, List<String> messages)
        {
            Title = title;
            Messages = messages;
        }

        public TrackedDownloadStatusMessage(String title, String message)
        {
            Title = title;
            Messages = new List<String>{ message };
        }
    }
}
