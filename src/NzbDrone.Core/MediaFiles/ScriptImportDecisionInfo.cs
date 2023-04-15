using System.Collections.Generic;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Download;
using NzbDrone.Core.Extras.Subtitles;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles
{
    public class ScriptImportDecisionInfo
    {
        // needed
        public bool isEpisodeFile { get; set; }

        // one layer passable
        public TransferMode mode { get; set; }

        // one layer passable
        public EpisodeFile episodeFile { get; set; }

        // two layer passable
        public LocalEpisode localEpisode { get; set; }

        // needed
        public DownloadClientItemClientInfo downloadClientItemInfo { get; set; }

        // needed
        public string downloadId { get; set; }

        // needed
        public List<EpisodeFile> OldFiles { get; set; }

        // needed
        public SubtitleFile subtitleFile { get; set; }
    }
}
