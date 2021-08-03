using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Tv
{
    public class MultipleSeriesFoundException : NzbDroneException
    {
        public MultipleSeriesFoundException(string message, params object[] args)
            : base(message, args)
        {
        }
    }
}
