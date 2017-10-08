using System;
using System.IO;
using System.Runtime.Serialization;

namespace NzbDrone.Common
{
    public class DestinationAlreadyExistsIOException : IOException
    {
        public DestinationAlreadyExistsIOException()
        {
        }

        public DestinationAlreadyExistsIOException(string message) : base(message)
        {
        }

        public DestinationAlreadyExistsIOException(string message, int hresult) : base(message, hresult)
        {
        }

        public DestinationAlreadyExistsIOException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DestinationAlreadyExistsIOException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
