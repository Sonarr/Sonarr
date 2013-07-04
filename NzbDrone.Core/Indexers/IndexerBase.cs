using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Indexers
{
    public abstract class IndexerBase : IIndexer
    {
        public abstract string Name { get; }

        public virtual bool EnableByDefault { get { return true; } }

        public IndexerDefinition InstanceDefinition { get; set; }

        public virtual IEnumerable<IndexerDefinition> DefaultDefinitions
        {
            get
            {
                yield return new IndexerDefinition
                {
                    Name = Name,
                    Enable = true,
                    Implementation = GetType().Name,
                    Settings = String.Empty
                };
            }
        }

        public virtual IParseFeed Parser
        {
            get
            {
                return new BasicRssParser();
            }
        }

        public abstract IEnumerable<string> RecentFeed { get; }
        public abstract IEnumerable<string> GetEpisodeSearchUrls(string seriesTitle, int seasonNumber, int episodeNumber);
        public abstract IEnumerable<string> GetDailyEpisodeSearchUrls(string seriesTitle, DateTime date);
        public abstract IEnumerable<string> GetSeasonSearchUrls(string seriesTitle, int seasonNumber);
        public abstract IEnumerable<string> GetPartialSeasonSearchUrls(string seriesTitle, int seasonNumber, int episodeWildcard);
    }


}