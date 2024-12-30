namespace Workarr.Exceptions
{
    public class WorkarrStartupException : WorkarrException
    {
        public WorkarrStartupException(string message, params object[] args)
            : base("Sonarr failed to start: " + string.Format(message, args))
        {
        }

        public WorkarrStartupException(string message)
            : base("Sonarr failed to start: " + message)
        {
        }

        public WorkarrStartupException()
            : base("Sonarr failed to start")
        {
        }

        public WorkarrStartupException(Exception innerException, string message, params object[] args)
            : base("Sonarr failed to start: " + string.Format(message, args), innerException)
        {
        }

        public WorkarrStartupException(Exception innerException, string message)
            : base("Sonarr failed to start: " + message, innerException)
        {
        }

        public WorkarrStartupException(Exception innerException)
            : base("Sonarr failed to start: " + innerException.Message)
        {
        }
    }
}
