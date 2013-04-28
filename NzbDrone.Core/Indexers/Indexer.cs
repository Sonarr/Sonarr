using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Indexers
{
    public abstract class Indexer : IIndexerBase
    {

        public abstract string Name { get; }



        public virtual bool EnabledByDefault
        {
            get
            {
                return true;
            }
        }

        public virtual IParseFeed Parser
        {
            get
            {
                return new BasicRssParser();
            }
        }

        public virtual bool IsConfigured
        {
            get { return true; }
        }



        public abstract IEnumerable<string> RecentFeed { get; }

        public abstract IEnumerable<string> GetEpisodeSearchUrls(string seriesTitle, int seasonNumber, int episodeNumber);
        public abstract IEnumerable<string> GetDailyEpisodeSearchUrls(string seriesTitle, DateTime date);
        public abstract IEnumerable<string> GetSeasonSearchUrls(string seriesTitle, int seasonNumber);
        public abstract IEnumerable<string> GetPartialSeasonSearchUrls(string seriesTitle, int seasonNumber, int episodeWildcard);
    }


}