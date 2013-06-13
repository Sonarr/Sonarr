using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Notifications.Prowl
{
    public class InvalidApiKeyException : Exception
    {
        public InvalidApiKeyException()
        {
        }

        public InvalidApiKeyException(string message) : base(message)
        {
        }
    }
}
