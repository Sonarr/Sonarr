using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Qualities
{
    public class QualityProfileInUseException : NzbDroneException
    {
        public QualityProfileInUseException(int profileId)
            : base("QualityProfile [{0}] is in use.", profileId)
        {

        }
    }
}