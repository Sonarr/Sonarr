using System.Collections.Generic;

namespace NzbDrone.Core.Messaging.Commands
{
    public class CommandPriorityComparer : IComparer<CommandStatus>
    {
        public int Compare(CommandStatus x, CommandStatus y)
        {
            if (x == CommandStatus.Started && y != CommandStatus.Started)
            {
                return -1;
            }

            if (x != CommandStatus.Started && y == CommandStatus.Started)
            {
                return 1;
            }

            if (x < y)
            {
                return -1;
            }

            if (x > y)
            {
                return 1;
            }

            return 0;
        }
    }
}
