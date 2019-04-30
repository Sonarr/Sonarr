using System;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Exceptions
{
    public class DownloadClientRejectedReleaseException : ReleaseDownloadException
    {
        public DownloadClientRejectedReleaseException(ReleaseInfo release, string message, params object[] args)
            : base(release, message, args)
        {
        }

        public DownloadClientRejectedReleaseException(ReleaseInfo release, string message)
            : base(release, message)
        {
        }

        public DownloadClientRejectedReleaseException(ReleaseInfo release, string message, Exception innerException, params object[] args)
            : base(release, message, innerException, args)
        {
        }

        public DownloadClientRejectedReleaseException(ReleaseInfo release, string message, Exception innerException)
            : base(release, message, innerException)
        {
        }
    }
}
