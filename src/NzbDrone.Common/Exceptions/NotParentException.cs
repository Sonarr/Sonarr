namespace NzbDrone.Common.Exceptions
{
    public class NotParentException : NzbDroneException
    {
        public NotParentException(string message, params object[] args) : base(message, args)
        {
        }

        public NotParentException(string message) : base(message)
        {
        }
    }
}
