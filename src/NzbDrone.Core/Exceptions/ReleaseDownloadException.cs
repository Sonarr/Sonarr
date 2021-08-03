using System;
using NzbDrone.Common.Exceptions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Exceptions
{
    public class ReleaseDownloadException : NzbDroneException
    {
        public ReleaseInfo Release { get; set; }

        public ReleaseDownloadException(ReleaseInfo release, string message, params object[] args)
            : base(message, args)
        {
            Release = release;
        }

        public ReleaseDownloadException(ReleaseInfo release, string message)
            : base(message)
        {
            Release = release;
        }

        public ReleaseDownloadException(ReleaseInfo release, string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
            Release = release;
        }

        public ReleaseDownloadException(ReleaseInfo release, string message, Exception innerException)
            : base(message, innerException)
        {
            Release = release;
        }
    }
}
