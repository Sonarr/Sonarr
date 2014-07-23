using System;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Tv.Commands
{
    public class MoveSeriesCommand : Command
    {
        public Int32 SeriesId { get; set; }
        public String SourcePath { get; set; }
        public String DestinationPath { get; set; }
        public String DestinationRootFolder { get; set; }
    }
}
