using System;
using System.Collections.Generic;
using NLog;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Download
{
    public abstract class DownloadClientBase<TSettings> : IDownloadClient where TSettings : IProviderConfig, new()
    {
        public Type ConfigContract
        {
            get
            {
                return typeof(TSettings);
            }
        }

        public IEnumerable<ProviderDefinition> DefaultDefinitions
        {
            get
            {
                return new List<ProviderDefinition>();
            }
        }

        public ProviderDefinition Definition { get; set; }

        protected TSettings Settings
        {
            get
            {
                return (TSettings)Definition.Settings;
            }
        }

        public override string ToString()
        {
            return GetType().Name;
        }

        public abstract string DownloadNzb(RemoteEpisode remoteEpisode);
        public abstract IEnumerable<QueueItem> GetQueue();
        public abstract IEnumerable<HistoryItem> GetHistory(int start = 0, int limit = 10);
        public abstract void RemoveFromQueue(string id);
        public abstract void RemoveFromHistory(string id);
    }
}
