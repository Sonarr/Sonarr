using System;

namespace NzbDrone.Common.Messaging
{
    public static class MessageExtensions
    {
        public static string GetExecutorName(this Type commandType)
        {
            if (!typeof(ICommand).IsAssignableFrom(commandType))
            {
                throw new ArgumentException("commandType must implement ICommand");
            }

            return string.Format("I{0}Executor", commandType.Name);
        }
    }
}