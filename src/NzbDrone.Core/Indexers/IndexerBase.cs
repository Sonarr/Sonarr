using System;
using System.Collections.Generic;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Indexers
{
    public abstract class IndexerBase<TSettings> : IIndexer where TSettings : IProviderConfig, new()
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
                var config = (IProviderConfig)new TSettings();

                yield return new IndexerDefinition
                {
                    Name = GetType().Name,
                    Enable = config.Validate().IsValid,
                    Implementation = GetType().Name,
                    Settings = config
                };
            }
        }

        public virtual ProviderDefinition Definition { get; set; }

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
        public virtual IEnumerable<string> GetSearchUrls(string query, int offset, int limit)
        {
            return new List<string>();
        }

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