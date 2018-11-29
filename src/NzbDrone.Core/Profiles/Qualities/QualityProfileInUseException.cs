using System.Net;
using NzbDrone.Core.Exceptions;

namespace NzbDrone.Core.Profiles.Qualities
{
    public class QualityProfileInUseException : NzbDroneClientException
    {
        public QualityProfileInUseException(string name)
            : base(HttpStatusCode.BadRequest, "QualityProfile [{0}] is in use.", name)
        {
        }
    }
}
