using Workarr.Messaging.Commands;

namespace Workarr.Housekeeping.Housekeepers
{
    public class CleanupCommandQueue : IHousekeepingTask
    {
        private readonly IManageCommandQueue _commandQueueManager;

        public CleanupCommandQueue(IManageCommandQueue commandQueueManager)
        {
            _commandQueueManager = commandQueueManager;
        }

        public void Clean()
        {
            _commandQueueManager.CleanCommands();
        }
    }
}
