using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Indexers.Newznab
{
    public class SizeParsingException : Exception
    {
        public SizeParsingException(string message) : base(message)
        {
        }
    }
}
