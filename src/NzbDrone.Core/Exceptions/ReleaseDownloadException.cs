using System;
using NzbDrone.Common.Exceptions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Exceptions
{
    public class ReleaseDownloadException : NzbDroneException
    {
        public ReleaseInfo Release { get; set; }

        public ReleaseDownloadException(ReleaseInfo release, String message, params Object[] args) : base(message, args)
        {
            Release = release;
        }

        public ReleaseDownloadException(ReleaseInfo release, String message)
            : base(message)
        {
            Release = release;
        }

        public ReleaseDownloadException(ReleaseInfo release, String message, Exception innerException, params Object[] args)
            : base(message, innerException, args)
        {
            Release = release;
        }

        public ReleaseDownloadException(ReleaseInfo release, String message, Exception innerException)
            : base(message, innerException)
        {
            Release = release;
        }
    }
}
