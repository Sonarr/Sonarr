using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.IndexerSearch
{
    public class SeriesSearchCommand : Command
    {
        public int SeriesId { get; set; }

        public override bool SendUpdatesToClient => true;

        public SeriesSearchCommand()
        {
        }

        public SeriesSearchCommand(int seriesId)
        {
            SeriesId = seriesId;
        }
    }
}
