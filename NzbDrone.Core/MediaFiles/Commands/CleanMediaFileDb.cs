using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.MediaFiles.Commands
{
    public class CleanMediaFileDb : ICommand
    {
        public int SeriesId { get; private set; }

        public CleanMediaFileDb(int seriesId)
        {
            SeriesId = seriesId;
        }
    }
}