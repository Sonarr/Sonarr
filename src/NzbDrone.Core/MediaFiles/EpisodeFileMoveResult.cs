using System.Collections.Generic;

namespace NzbDrone.Core.MediaFiles
{
    public class EpisodeFileMoveResult
    {
        public EpisodeFileMoveResult()
        {
            OldFiles = new List<DeletedEpisodeFile>();
        }

        public EpisodeFile EpisodeFile { get; set; }
        public List<DeletedEpisodeFile> OldFiles { get; set; }
    }
}
