using NLog;

namespace NzbDrone.Core.Messaging.Commands
{
    public class UnknownCommandExecutor : IExecute<UnknownCommand>
    {
        private readonly Logger _logger;

        public UnknownCommandExecutor(Logger logger)
        {
            _logger = logger;
        }

        public void Execute(UnknownCommand message)
        {
            _logger.Debug("Ignoring unknown command {0}", message.ContractName);
        }
    }
}
