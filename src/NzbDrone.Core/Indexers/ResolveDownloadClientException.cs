using System.Net;
using NzbDrone.Core.Exceptions;

namespace NzbDrone.Core.Indexers;

public class ResolveIndexerException : NzbDroneClientException
{
    public ResolveIndexerException(string message, params object[] args)
        : base(HttpStatusCode.BadRequest, message, args)
    {
    }
}
