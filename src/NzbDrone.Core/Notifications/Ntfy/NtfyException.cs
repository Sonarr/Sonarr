using System;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Notifications.Ntfy
{
    public class NtfyException : NzbDroneException
    {
        public NtfyException(string message)
            : base(message)
        {
        }

        public NtfyException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
