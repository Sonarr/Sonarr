using System;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Messaging
{
    public static class MessageExtensions
    {
        public static string GetExecutorName(this Type commandType)
        {
            if (!typeof(Command).IsAssignableFrom(commandType))
            {
                throw new ArgumentException("commandType must implement ICommand");
            }

            return string.Format("I{0}Executor", commandType.Name);
        }
    }
}