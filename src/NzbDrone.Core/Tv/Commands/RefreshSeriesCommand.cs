using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Tv.Commands
{
    public class RefreshSeriesCommand : Command
    {
        public int? SeriesId { get; set; }
        public List<int> SeriesIds { get; set; }
        public bool IsNewSeries { get; set; }

        public RefreshSeriesCommand()
        {
            SeriesIds = new List<int>();
        }

        public RefreshSeriesCommand(int seriesId, bool isNewSeries = false)
        {
            SeriesIds = new List<int> { seriesId };
        }

        public RefreshSeriesCommand(List<int> seriesIds, bool isNewSeries = false)
        {
            SeriesIds = seriesIds;
            IsNewSeries = isNewSeries;
        }

        public override bool SendUpdatesToClient => true;

        public override bool UpdateScheduledTask => !SeriesIds.Any();

        public override bool IsLongRunning => true;
    }
}
