using System.Collections.Generic;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Tv.Commands
{
    public class BulkMoveSeriesCommand : Command
    {
        public List<BulkMoveSeries> Series { get; set; }
        public string DestinationRootFolder { get; set; }

        public override bool SendUpdatesToClient => true;
    }

    public class BulkMoveSeries
    {
        public int SeriesId { get; set; }
        public string SourcePath { get; set; }
    }
}
