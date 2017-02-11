using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Tv.Commands
{
    public class MoveSeriesCommand : Command
    {
        public int SeriesId { get; set; }
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }

        public override bool SendUpdatesToClient => true;
    }
}
