using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Indexers.Newznab
{
    public class SizeParsingException : NzbDroneException
    {
        public SizeParsingException(string message, params object[] args) : base(message, args)
        {
        }
    }
}
