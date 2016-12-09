using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Tv.Commands
{
    public class RefreshSeriesCommand : Command
    {
        public int? SeriesId { get; set; }

        public RefreshSeriesCommand()
        {
        }

        public RefreshSeriesCommand(int? seriesId)
        {
            SeriesId = seriesId;
        }

        public override bool SendUpdatesToClient => true;

        public override bool UpdateScheduledTask => !SeriesId.HasValue;
    }
}