using System;
using System.Collections.Generic;
using NzbDrone.Core.MediaFiles.Series;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications
{
    public class DownloadMessage
    {
        public String Message { get; set; }
        public Series Series { get; set; }
        public EpisodeFile EpisodeFile { get; set; }
        public List<EpisodeFile> OldFiles { get; set; }
        public String SourcePath { get; set; }

        public override string ToString()
        {
            return Message;
        }
    }
}
