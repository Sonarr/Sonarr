using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Indexers.Torznab
{
    public class Torznab : HttpIndexerBase<TorznabSettings>
    {
        public override DownloadProtocol Protocol { get { return DownloadProtocol.Torrent; } }
        public override Int32 PageSize { get { return 100; } }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new TorznabRequestGenerator()
            {
                PageSize = PageSize, 
                Settings = Settings
            };
        }

        public override IParseIndexerResponse GetParser()
        {
            return new TorznabRssParser();
        }

        public override IEnumerable<ProviderDefinition> DefaultDefinitions
        {
            get
            {
                yield return GetDefinition("HDAccess.net", GetSettings("http://hdaccess.net"));
            }
        }

        public Torznab(IHttpClient httpClient, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, configService, parsingService, logger)
        {

        }

        private IndexerDefinition GetDefinition(String name, TorznabSettings settings)
        {
            return new IndexerDefinition
                   {
                       EnableRss = false,
                       EnableSearch = false,
                       Name = name,
                       Implementation = GetType().Name,
                       Settings = settings,
                       Protocol = DownloadProtocol.Usenet,
                       SupportsRss = SupportsRss,
                       SupportsSearch = SupportsSearch
                   };
        }

        private TorznabSettings GetSettings(String url, params int[] categories)
        {
            var settings = new TorznabSettings { Url = url };

            if (categories.Any())
            {
                settings.Categories = categories;
            }

            return settings;
        }
    }
}
