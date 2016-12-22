using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Profiles.Qualities
{
    public class ProfileInUseException : NzbDroneException
    {
        public ProfileInUseException(int profileId)
            : base("Profile [{0}] is in use.", profileId)
        {

        }
    }
}