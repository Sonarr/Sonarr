using System;

namespace NzbDrone.Core.Download.Clients.Transmission
{
    public class TransmissionException : Exception
    {
        public TransmissionException(String message)
            : base(message)
        {

        }
    }
}
