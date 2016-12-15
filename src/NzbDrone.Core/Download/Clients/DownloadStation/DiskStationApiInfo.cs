namespace NzbDrone.Core.Download.Clients.DownloadStation
{
    public class DiskStationApiInfo
    {
        private string _path;

        public int MaxVersion { get; set; }

        public int MinVersion { get; set; }

        public string Path
        {
            get { return _path; }

            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _path = value.TrimStart(new char[] { '/', '\\' });
                }
            }
        }
    }
}
