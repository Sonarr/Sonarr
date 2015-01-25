using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Update
{
    public class UpdateFailedException : NzbDroneException
    {
        public UpdateFailedException(string message, params object[] args)
            : base(message, args)
        {
        }

        public UpdateFailedException(string message)
            : base(message)
        {
        }
    }
}
