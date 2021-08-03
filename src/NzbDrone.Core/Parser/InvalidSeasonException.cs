using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Parser
{
    public class InvalidSeasonException : NzbDroneException
    {
        public InvalidSeasonException(string message, params object[] args)
            : base(message, args)
        {
        }

        public InvalidSeasonException(string message)
            : base(message)
        {
        }
    }
}
