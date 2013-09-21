using System;
using System.Collections.Generic;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Indexers
{
    public abstract class IndexerBase : IIndexer
    {
        public abstract string Name { get; }

        public abstract IndexerKind Kind { get; }

        public virtual bool EnableByDefault { get { return true; } }

        public IndexerDefinition InstanceDefinition { get; set; }

        public virtual IEnumerable<IndexerDefinition> DefaultDefinitions
        {
            get
            {
                yield return new IndexerDefinition
                {
                    Name = Name,
                    Enable = EnableByDefault,
                    Implementation = GetType().Name,
                    Settings = NullSetting.Instance
                };
            }
        }

        public virtual IParseFeed Parser { get; private set; }

        public abstract IEnumerable<string> RecentFeed { get; }
        public abstract IEnumerable<string> GetEpisodeSearchUrls(string seriesTitle, int tvRageId, int seasonNumber, int episodeNumber);
        public abstract IEnumerable<string> GetDailyEpisodeSearchUrls(string seriesTitle, int tvRageId, DateTime date);
        public abstract IEnumerable<string> GetSeasonSearchUrls(string seriesTitle, int tvRageId, int seasonNumber, int offset);
    }

    public enum IndexerKind
    {
        Usenet,
        Torrent
    }
}