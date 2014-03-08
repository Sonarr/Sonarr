using System.Collections.Generic;

namespace NzbDrone.Core.MediaFiles
{
    public class EpisodeFileMoveResult
    {
        public EpisodeFileMoveResult()
        {
            OldFiles = new List<EpisodeFile>();
        }

        public EpisodeFile EpisodeFile { get; set; }
        public List<EpisodeFile> OldFiles { get; set; }
    }
}
