using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Indexers.Newznab
{
    public class NewznabException : NzbDroneException
    {
        public NewznabException(string message, params object[] args) : base(message, args)
        {
        }

        public NewznabException(string message) : base(message)
        {
        }
    }
}
