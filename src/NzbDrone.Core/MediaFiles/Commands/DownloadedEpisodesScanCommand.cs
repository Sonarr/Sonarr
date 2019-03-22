using NzbDrone.Core.MediaFiles.EpisodeImport;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.MediaFiles.Commands
{
    public class DownloadedEpisodesScanCommand : Command
    {
        // Properties used by third-party apps, do not modify.
        public string Path { get; set; }
        public string DownloadClientId { get; set; }
        public ImportMode ImportMode { get; set; }
    }
}
