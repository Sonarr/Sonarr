using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Model
{
    public class EpisodeRenameModel
    {
        public string SeriesName { get; set; }
        public string Folder { get; set; }
        public EpisodeFile EpisodeFile { get; set; }
        public bool NewDownload { get; set; }
    }
}