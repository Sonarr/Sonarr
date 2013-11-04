using System;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download
{
    public class HistoryItem
    {
        public String Id { get; set; }
        public String Title { get; set; }
        public String Size { get; set; }
        public String Category { get; set; }
        public Int32 DownloadTime { get; set; }
        public String Storage { get; set; }
        public String Message { get; set; }
        public HistoryStatus Status { get; set; }
    }

    public enum HistoryStatus
    {
        Completed = 0,
        Failed = 1
    }
}
