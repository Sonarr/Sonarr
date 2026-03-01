using System.Net;
using NzbDrone.Core.Exceptions;

namespace NzbDrone.Core.Download;

public class ResolveDownloadClientException : NzbDroneClientException
{
    public ResolveDownloadClientException(string message, params object[] args)
        : base(HttpStatusCode.BadRequest, message, args)
    {
    }
}
