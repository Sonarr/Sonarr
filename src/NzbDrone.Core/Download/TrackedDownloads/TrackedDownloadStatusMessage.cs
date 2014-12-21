using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Download.TrackedDownloads
{
    public class TrackedDownloadStatusMessage
    {
        public String Title { get; set; }
        public List<String> Messages { get; set; }


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

        //Constructor for use when deserializing JSON
        private TrackedDownloadStatusMessage()
        {
        }
    }
}
