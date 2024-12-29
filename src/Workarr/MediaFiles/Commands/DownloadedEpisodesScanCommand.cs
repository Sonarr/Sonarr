using Workarr.MediaFiles.EpisodeImport;
using Workarr.Messaging.Commands;

namespace Workarr.MediaFiles.Commands
{
    public class DownloadedEpisodesScanCommand : Command
    {
        // Properties used by third-party apps, do not modify.
        public string Path { get; set; }
        public string DownloadClientId { get; set; }
        public ImportMode ImportMode { get; set; }
        public override bool RequiresDiskAccess => true;
        public override bool IsLongRunning => true;
    }
}
