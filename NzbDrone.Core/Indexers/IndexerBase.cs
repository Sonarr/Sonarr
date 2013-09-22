using System;
using System.Collections.Generic;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Indexers
{
    public abstract class IndexerBase<TSettings> : IIndexer
    {
        public Type ConfigContract
        {
            get
            {
                return typeof(TSettings);
            }
        }

        public virtual IEnumerable<ProviderDefinition> DefaultDefinitions
        {
            get
            {
                yield return new IndexerDefinition
                {
                    Name = this.GetType().Name,
                    Enable = false,
                    Implementation = GetType().Name,
                    Settings = NullConfig.Instance
                };
            }
        }

        public ProviderDefinition Definition { get; set; }

        public abstract DownloadProtocol Protocol { get; }

        protected TSettings Settings
        {
            get
            {
                return (TSettings)Definition.Settings;
            }
        }

        public virtual IParseFeed Parser { get; private set; }

        public abstract IEnumerable<string> RecentFeed { get; }
        public abstract IEnumerable<string> GetEpisodeSearchUrls(string seriesTitle, int tvRageId, int seasonNumber, int episodeNumber);
        public abstract IEnumerable<string> GetDailyEpisodeSearchUrls(string seriesTitle, int tvRageId, DateTime date);
        public abstract IEnumerable<string> GetSeasonSearchUrls(string seriesTitle, int tvRageId, int seasonNumber, int offset);


        public override string ToString()
        {
            return GetType().Name;
        }
    }

    public enum DownloadProtocol
    {
        Usenet,
        Torrent
    }
}