namespace Workarr.Exceptions
{
    public abstract class WorkarrException : ApplicationException
    {
        protected WorkarrException(string message, params object[] args)
            : base(string.Format(message, args))
        {
        }

        protected WorkarrException(string message)
            : base(message)
        {
        }

        protected WorkarrException(string message, Exception innerException, params object[] args)
            : base(string.Format(message, args), innerException)
        {
        }

        protected WorkarrException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
