using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Indexers.Exceptions
{
    public class IndexerException : NzbDroneException
    {
        public IndexerException(string message, params object[] args)
            : base(message, args)
        {
        }
    }
}
