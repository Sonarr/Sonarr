using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Profiles.Languages
{
    public class LanguageProfileInUseException : NzbDroneException
    {
        public LanguageProfileInUseException(int profileId)
            : base("Language profile [{0}] is in use.", profileId)
        {
        }
    }
}
