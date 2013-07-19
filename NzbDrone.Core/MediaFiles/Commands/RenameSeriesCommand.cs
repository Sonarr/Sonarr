using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.MediaFiles.Commands
{
    public class RenameSeriesCommand : ICommand
    {
        public int SeriesId { get; private set; }

        public RenameSeriesCommand(int seriesId)
        {
            SeriesId = seriesId;
        }
    }
}