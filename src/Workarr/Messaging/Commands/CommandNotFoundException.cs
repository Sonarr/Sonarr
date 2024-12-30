using Workarr.Exceptions;

namespace Workarr.Messaging.Commands
{
    public class CommandNotFoundException : WorkarrException
    {
        public CommandNotFoundException(string contract)
            : base("Couldn't find command " + contract)
        {
        }
    }
}
