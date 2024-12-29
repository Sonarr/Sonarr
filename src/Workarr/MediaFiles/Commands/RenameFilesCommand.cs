using Workarr.Messaging.Commands;

namespace Workarr.MediaFiles.Commands
{
    public class RenameFilesCommand : Command
    {
        public int SeriesId { get; set; }
        public List<int> Files { get; set; }

        public override bool SendUpdatesToClient => true;
        public override bool RequiresDiskAccess => true;

        public RenameFilesCommand()
        {
        }

        public RenameFilesCommand(int seriesId, List<int> files)
        {
            SeriesId = seriesId;
            Files = files;
        }
    }
}
