using Workarr.Exceptions;

namespace Workarr.Tv
{
    public class MultipleSeriesFoundException : WorkarrException
    {
        public List<Series> Series { get; set; }

        public MultipleSeriesFoundException(List<Series> series, string message, params object[] args)
            : base(message, args)
        {
            Series = series;
        }
    }
}
