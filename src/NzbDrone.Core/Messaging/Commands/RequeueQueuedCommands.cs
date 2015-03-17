using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Messaging.Commands
{
    public class RequeueQueuedCommands : IHandle<ApplicationStartedEvent>
    {
        private readonly IManageCommandQueue _commandQueueManager;

        public RequeueQueuedCommands(IManageCommandQueue commandQueueManager)
        {
            _commandQueueManager = commandQueueManager;
        }

        public void Handle(ApplicationStartedEvent message)
        {
            _commandQueueManager.Requeue();
        }
    }
}
