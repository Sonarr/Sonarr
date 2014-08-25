using System;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;
using NzbDrone.Core.MediaFiles.MediaInfo;

namespace NzbDrone.Core.MediaFiles
{
    public class EpisodeFile : ModelBase
    {
        public Int32 SeriesId { get; set; }
        public Int32 SeasonNumber { get; set; }
        public String RelativePath { get; set; }
        public String Path { get; set; }
        public Int64 Size { get; set; }
        public DateTime DateAdded { get; set; }
        public String SceneName { get; set; }
        public String ReleaseGroup { get; set; }
        public QualityModel Quality { get; set; }
        public MediaInfoModel MediaInfo { get; set; }
        public LazyList<Episode> Episodes { get; set; }

        public override String ToString()
        {
            return String.Format("[{0}] {1}", Id, RelativePath);
        }
//
//        public String Path(Series series)
//        {
//            return System.IO.Path.Combine(series.Path, RelativePath);
//        }
    }
}