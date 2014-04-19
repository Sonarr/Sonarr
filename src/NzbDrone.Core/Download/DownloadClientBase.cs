using System;
using System.Linq;
using System.Collections.Generic;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.ThingiProvider;
using NLog;

namespace NzbDrone.Core.Download
{
    public abstract class DownloadClientBase<TSettings> : IDownloadClient
        where TSettings : IProviderConfig, new()
    {
        private readonly IParsingService _parsingService;
        protected readonly Logger _logger;

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

        protected DownloadClientBase(IParsingService parsingService, Logger logger)
        {
            _parsingService = parsingService;
            _logger = logger;
        }

        public override string ToString()
        {
            return GetType().Name;
        }



        public abstract DownloadProtocol Protocol
        {
            get;
        }

        public abstract string Download(RemoteEpisode remoteEpisode);
        public abstract IEnumerable<DownloadClientItem> GetItems();
        public abstract void RemoveItem(string id);
        public abstract void RetryDownload(string id);
        public abstract void Test();

        protected RemoteEpisode GetRemoteEpisode(String title)
        {
            var parsedEpisodeInfo = Parser.Parser.ParseTitle(title);
            if (parsedEpisodeInfo == null) return null;

            var remoteEpisode = _parsingService.Map(parsedEpisodeInfo, 0);
            if (remoteEpisode.Series == null) return null;

            return remoteEpisode;
        }
    }
}
