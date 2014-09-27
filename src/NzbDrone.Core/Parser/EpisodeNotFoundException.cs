using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Parser
{
    public class EpisodeNotFoundException : NzbDroneException
    {
        public EpisodeNotFoundException(string message, params object[] args) : base(message, args)
        {
        }
    }
}
