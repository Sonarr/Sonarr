using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Messaging.Commands
{
    public class CommandNotFoundException : NzbDroneException
    {
        public CommandNotFoundException(string contract)
            : base("Couldn't find command " + contract)
        {
        }
    }
}
