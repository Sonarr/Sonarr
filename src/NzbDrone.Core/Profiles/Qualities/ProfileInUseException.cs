using System.Net;
using NzbDrone.Core.Exceptions;

namespace NzbDrone.Core.Profiles.Qualities
{
    public class ProfileInUseException : NzbDroneClientException
    {
        public ProfileInUseException(string name)
            : base(HttpStatusCode.BadRequest, "Profile [{0}] is in use.", name)
        {
        }
    }
}
