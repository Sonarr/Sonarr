using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Update
{
    public class UpdateVerificationFailedException : NzbDroneException
    {
        public UpdateVerificationFailedException(string message, params object[] args) : base(message, args)
        {
        }

        public UpdateVerificationFailedException(string message) : base(message)
        {
        }
    }
}
