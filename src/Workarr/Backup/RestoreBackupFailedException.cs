using System.Net;
using Workarr.Exceptions;

namespace Workarr.Backup
{
    public class RestoreBackupFailedException : WorkarrClientException
    {
        public RestoreBackupFailedException(HttpStatusCode statusCode, string message, params object[] args)
            : base(statusCode, message, args)
        {
        }

        public RestoreBackupFailedException(HttpStatusCode statusCode, string message)
            : base(statusCode, message)
        {
        }
    }
}
