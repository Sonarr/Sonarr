using NzbDrone.Common.Messaging;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaCover
{
    public class MediaCoversUpdatedEvent : IEvent
    {
        public Series Series { get; set; }
        public Movie Movie { get; set; }
        public MediaCoverOrigin CoverOrigin { get; set; }

        public MediaCoversUpdatedEvent(Series series)
        {
            Series = series;
            CoverOrigin = MediaCoverOrigin.Series;
        }

        public MediaCoversUpdatedEvent(Movie movie)
        {
            Movie = movie;
            CoverOrigin = MediaCoverOrigin.Movie;
        }
    }
}
