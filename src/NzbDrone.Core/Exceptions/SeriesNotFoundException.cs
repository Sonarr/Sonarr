using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Exceptions
{
    public class SeriesNotFoundException : NzbDroneException
    {
        public int TvdbSeriesId { get; set; }

        public SeriesNotFoundException(int tvdbSeriesId)
            : base(string.Format("Series with tvdbid {0} was not found, it may have been removed from TheTVDB.", tvdbSeriesId))
        {
            TvdbSeriesId = tvdbSeriesId;
        }

        public SeriesNotFoundException(int tvdbSeriesId, string message, params object[] args)
            : base(message, args)
        {
            TvdbSeriesId = tvdbSeriesId;
        }

        public SeriesNotFoundException(int tvdbSeriesId, string message)
            : base(message)
        {
            TvdbSeriesId = tvdbSeriesId;
        }
    }
}
