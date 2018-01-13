using System.Net;
using NzbDrone.Core.Exceptions;

namespace NzbDrone.Core.Backup
{
    public class RestoreBackupFailedException : NzbDroneClientException
    {
        public RestoreBackupFailedException(HttpStatusCode statusCode, string message, params object[] args) : base(statusCode, message, args)
        {
        }

        public RestoreBackupFailedException(HttpStatusCode statusCode, string message) : base(statusCode, message)
        {
        }
    }
}
