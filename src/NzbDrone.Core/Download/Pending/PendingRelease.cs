using System;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download.Pending
{
    public class PendingRelease : ModelBase
    {
        public int SeriesId { get; set; }
        public int MovieId { get; set; }
        public String Title { get; set; }
        public DateTime Added { get; set; }
        public ParsedEpisodeInfo ParsedEpisodeInfo { get; set; }
        public ParsedMovieInfo ParsedMovieInfo { get; set; }
        public ReleaseInfo Release { get; set; }

        //Not persisted
        public RemoteItem RemoteItem { get; set; }
        public ParsedInfo ParsedInfo
        {
            get
            {
                if (SeriesId > 0) return this.ParsedEpisodeInfo;
                else if (MovieId > 0) return this.ParsedMovieInfo;
                else return null;
            }
            set
            {
                if (SeriesId == 0 && MovieId == 0)
                    throw new InvalidOperationException("SeriesId and MoviesId have no value.");
                if (SeriesId > 0 && MovieId > 0)
                    throw new InvalidOperationException("SeriesId and MoviesId have value.");
                if (SeriesId > 0) this.ParsedEpisodeInfo = value as ParsedEpisodeInfo;
                else if (MovieId > 0) this.ParsedMovieInfo = value as ParsedMovieInfo;
            }
        }
    }
}
