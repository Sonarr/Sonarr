using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Tv.Commands
{
    public class RefreshSeriesCommand : Command
    {
        public int? SeriesId { get; set; }
        public bool IsNewSeries { get; set; }

        public RefreshSeriesCommand()
        {
        }

        public RefreshSeriesCommand(int? seriesId, bool isNewSeries = false)
        {
            SeriesId = seriesId;
            IsNewSeries = isNewSeries;
        }

        public override bool SendUpdatesToClient => true;

        public override bool UpdateScheduledTask => !SeriesId.HasValue;

        public override bool IsLongRunning => true;
    }
}
