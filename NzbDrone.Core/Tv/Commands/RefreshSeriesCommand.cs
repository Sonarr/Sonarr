using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Tv.Commands
{
    public class RefreshSeriesCommand : ICommand
    {
        public int? SeriesId { get; private set; }

        public RefreshSeriesCommand(int? seriesId)
        {
            SeriesId = seriesId;
        }
    }
}