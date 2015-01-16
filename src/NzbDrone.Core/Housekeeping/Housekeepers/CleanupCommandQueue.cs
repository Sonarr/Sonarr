using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupCommandQueue : IHousekeepingTask
    {
        private readonly ICommandRepository _commandRepository;

        public CleanupCommandQueue(ICommandRepository commandRepository)
        {
            _commandRepository = commandRepository;
        }

        public void Clean()
        {
            _commandRepository.Trim();
        }
    }
}
