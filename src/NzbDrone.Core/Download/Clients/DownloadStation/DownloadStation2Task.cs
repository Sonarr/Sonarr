namespace NzbDrone.Core.Download.Clients.DownloadStation
{
    public class DownloadStation2Task
    {
        public string Username { get; set; }

        public string Id { get; set; }

        public string Title { get; set; }

        public long Size { get; set; }

        /// <summary>
        /// /// Possible values are: BT, NZB, http, ftp, eMule and https
        /// </summary>
        public string Type { get; set; }

        public int Status { get; set; }

        public DownloadStationTaskAdditional Additional { get; set; }

        public override string ToString()
        {
            return this.Title;
        }
    }
}
