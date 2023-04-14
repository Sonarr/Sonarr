using System.Collections.Generic;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles
{
    public class ScriptImportDecisionInfo
    {
        public bool isEpisodeFile { get; set; }
        public TransferMode mode { get; set; }
        public EpisodeFile episodeFile { get; set; }
        public LocalEpisode localEpisode { get; set; }
        public DownloadClientItemClientInfo downloadClientItemInfo { get; set; }
        public string downloadId { get; set; }
        public List<EpisodeFile> OldFiles { get; set; }
    }
}
