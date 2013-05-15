using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.MediaFiles.Commands
{
    public class DiskScanCommand : ICommand
    {
        public int? SeriesId { get; private set; }

        public DiskScanCommand(int seriesId = 0)
        {
            if (seriesId != 0)
            {
                SeriesId = seriesId;
            }
        }

        public DiskScanCommand()
        {
        }
    }
}