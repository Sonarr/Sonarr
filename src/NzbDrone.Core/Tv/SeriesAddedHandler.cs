using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv.Commands;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.Tv
{
    public class SeriesAddedHandler : IHandle<SeriesAddedEvent>
    {
        private readonly IManageCommandQueue _commandQueueManager;

        public SeriesAddedHandler(IManageCommandQueue commandQueueManager)
        {
            _commandQueueManager = commandQueueManager;
        }

        public void Handle(SeriesAddedEvent message)
        {
            _commandQueueManager.Push(new RefreshSeriesCommand(message.Series.Id));
        }
    }
}
