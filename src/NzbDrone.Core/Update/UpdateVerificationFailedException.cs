namespace NzbDrone.Core.Update
{
    public class UpdateVerificationFailedException : UpdateFailedException
    {
        public UpdateVerificationFailedException(string message, params object[] args)
            : base(message, args)
        {
        }

        public UpdateVerificationFailedException(string message)
            : base(message)
        {
        }
    }
}
