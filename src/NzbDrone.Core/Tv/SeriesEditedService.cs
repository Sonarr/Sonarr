using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv.Commands;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.Tv
{
    public class SeriesEditedService : IHandle<SeriesEditedEvent>
    {
        private readonly CommandExecutor _commandExecutor;

        public SeriesEditedService(CommandExecutor commandExecutor)
        {
            _commandExecutor = commandExecutor;
        }

        public void Handle(SeriesEditedEvent message)
        {
            //TODO: Refresh if path has changed (also move files)

            if (message.Series.SeriesType != message.OldSeries.SeriesType)
            {
                _commandExecutor.PublishCommandAsync(new RefreshSeriesCommand(message.Series.Id));
            }
        }
    }
}
