using System;

namespace NzbDrone.Host
{
    public class TerminateApplicationException : ApplicationException
    {
        public TerminateApplicationException(string reason)
        {
            Reason = reason;
        }

        public string Reason { get; private set; }
    }
}