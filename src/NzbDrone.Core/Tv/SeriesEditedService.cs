using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv.Commands;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.Tv
{
    public class SeriesEditedService : IHandle<SeriesEditedEvent>
    {
        private readonly IManageCommandQueue _commandQueueManager;

        public SeriesEditedService(IManageCommandQueue commandQueueManager)
        {
            _commandQueueManager = commandQueueManager;
        }

        public void Handle(SeriesEditedEvent message)
        {
            if (message.Series.SeriesType != message.OldSeries.SeriesType)
            {
                _commandQueueManager.Push(new RefreshSeriesCommand(message.Series.Id, false));
            }
        }
    }
}
