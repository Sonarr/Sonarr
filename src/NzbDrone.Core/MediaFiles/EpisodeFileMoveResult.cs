using System;
using System.Collections.Generic;

namespace NzbDrone.Core.MediaFiles
{
    public class EpisodeFileMoveResult
    {
        public EpisodeFileMoveResult()
        {
            OldFiles = new List<EpisodeFile>();
        }

        public String Path { get; set; }
        public List<EpisodeFile> OldFiles { get; set; }
    }
}
