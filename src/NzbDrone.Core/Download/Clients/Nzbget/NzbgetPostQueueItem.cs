namespace NzbDrone.Core.Download.Clients.Nzbget
{
    public class NzbgetPostQueueItem
    {
        public int NzbId { get; set; }
        public string NzbName { get; set; }
        public string Stage { get; set; }
        public string ProgressLabel { get; set; }
        public int FileProgress { get; set; }
        public int StageProgress { get; set; }
        public int TotalTimeSec { get; set; }
        public int StageTimeSec { get; set; }
    }
}
