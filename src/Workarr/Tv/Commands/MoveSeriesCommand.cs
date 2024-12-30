using Workarr.Messaging.Commands;

namespace Workarr.Tv.Commands
{
    public class MoveSeriesCommand : Command
    {
        public int SeriesId { get; set; }
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }

        public override bool SendUpdatesToClient => true;
        public override bool RequiresDiskAccess => true;
    }
}
