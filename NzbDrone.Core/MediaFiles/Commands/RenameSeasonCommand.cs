using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.MediaFiles.Commands
{
    public class RenameSeasonCommand : ICommand
    {
        public int SeriesId { get; private set; }
        public int SeasonNumber { get; private set; }

        public RenameSeasonCommand(int seriesId, int seasonNumber)
        {
            SeriesId = seriesId;
            SeasonNumber = seasonNumber;
        }
    }
}