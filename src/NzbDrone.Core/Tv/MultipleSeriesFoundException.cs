using System.Collections.Generic;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Tv
{
    public class MultipleSeriesFoundException : NzbDroneException
    {
        public List<Series> Series { get; set; }

        public MultipleSeriesFoundException(List<Series> series, string message, params object[] args)
            : base(message, args)
        {
            Series = series;
        }
    }
}
