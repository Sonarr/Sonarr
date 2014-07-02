using System;
using NzbDrone.Api.REST;

namespace NzbDrone.Api.Logs
{
    public class LogFileResource : RestResource
    {
        public String Filename { get; set; }
        public DateTime LastWriteTime { get; set; }
        public String ContentsUrl { get; set; }
        public String DownloadUrl { get; set; }
    }
}
