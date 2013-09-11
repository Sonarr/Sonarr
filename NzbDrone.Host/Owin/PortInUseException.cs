using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Host.Owin
{
    public class PortInUseException : NzbDroneException
    {
        public PortInUseException(string message, Exception innerException, params object[] args) : base(message, innerException, args)
        {
        }
    }
}
