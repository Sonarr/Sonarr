using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Qualities;
using System;

namespace NzbDrone.Core.Datastore
{
    public class MediaModelBase : ModelBase
    {
        public String Path { get; set; }
        public String RelativePath { get; set; }
        public Int64 Size { get; set; }
        public DateTime DateAdded { get; set; }
        public String SceneName { get; set; }
        public String ReleaseGroup { get; set; }
        public QualityModel Quality { get; set; }
        public MediaInfoModel MediaInfo { get; set; }

        public override String ToString()
        {
            return String.Format("[{0}] {1}", Id, RelativePath);
        }
    }
}
