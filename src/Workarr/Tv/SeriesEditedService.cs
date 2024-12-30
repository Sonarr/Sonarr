using Workarr.Messaging.Commands;
using Workarr.Messaging.Events;
using Workarr.Tv.Commands;
using Workarr.Tv.Events;

namespace Workarr.Tv
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
                _commandQueueManager.Push(new RefreshSeriesCommand(new List<int> { message.Series.Id }, false));
            }
        }
    }
}
