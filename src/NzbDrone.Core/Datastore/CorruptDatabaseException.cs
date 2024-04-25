using System;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Datastore
{
    public class CorruptDatabaseException : SonarrStartupException
    {
        public CorruptDatabaseException(string message, params object[] args)
            : base(message, args)
        {
        }

        public CorruptDatabaseException(string message)
            : base(message)
        {
        }

        public CorruptDatabaseException(string message, Exception innerException, params object[] args)
            : base(innerException, message, args)
        {
        }

        public CorruptDatabaseException(string message, Exception innerException)
            : base(innerException, message)
        {
        }
    }
}
