using System;

namespace NzbDrone.Host
{
    public class TerminateApplicationException : ApplicationException
    {
        public TerminateApplicationException(string reason)
            : base("Application is being terminated. Reason : " + reason)
        {
        }
    }
}
