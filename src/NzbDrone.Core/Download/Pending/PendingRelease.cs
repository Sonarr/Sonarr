using System;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download.Pending
{
    public class PendingRelease : ModelBase
    {
        public Int32 SeriesId { get; set; }
        public Int32 MovieId { get; set; }
        public String Title { get; set; }
        public DateTime Added { get; set; }
        public ParsedEpisodeInfo ParsedEpisodeInfo { get; set; }
        public ParsedMovieInfo ParsedMovieInfo { get; set; }

        public ParsedInfo ParsedInfo
        {
            get
            {
                if (SeriesId > 0) return ParsedEpisodeInfo;
                else if (MovieId > 0) return ParsedMovieInfo;
                else return null;
            }
            set
            {
                if (SeriesId > 0) ParsedEpisodeInfo = value as ParsedEpisodeInfo;
                else if (MovieId > 0) ParsedMovieInfo = value as ParsedMovieInfo;
            }
        }
        public ReleaseInfo Release { get; set; }

        //Not persisted
        public RemoteItem RemoteItem { get; set; }
    }
}
