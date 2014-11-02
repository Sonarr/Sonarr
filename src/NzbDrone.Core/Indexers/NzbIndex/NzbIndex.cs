using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Indexers.NzbIndex
{
    public class NzbIndex : HttpIndexerBase<NzbIndexSettings>
    {
        public NzbIndex(IHttpClient httpClient, IConfigService configService, IParsingService parsingService, Logger logger) 
            : base(httpClient, configService, parsingService, logger)
        {
        }

        public override DownloadProtocol Protocol
        {
            get
            {
                return DownloadProtocol.Usenet;
            }
        }

        public override bool SupportsRss
        {
            get
            {
                return true;
            }
        }

        public override IEnumerable<ProviderDefinition> DefaultDefinitions
        {
            get
            {
                yield return new IndexerDefinition
                {
                    EnableRss = false,
                    EnableSearch = false,
                    Name = "NzbIndex.com",
                    Implementation = GetType().Name,
                    Settings = new NzbIndexSettings { Url = "http://nzbindex.com/rss/", QueryParam = "q", MinSizeParam = "minsize", MaxSizeParam = "maxsize", ResponseMaxSizeParam = "max", MaxAgeParam = "age", ResponseMaxSize = 50},
                    Protocol = DownloadProtocol.Usenet,
                    SupportsRss = SupportsRss,
                    SupportsSearch = SupportsSearch
                };
            }
        }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new NzbIndexRequestGenerator(Settings);
        }

        public override IParseIndexerResponse GetParser()
        {
            return new NzbIndexRssParser { UseEnclosureUrl = true, UseEnclosureLength = true, UseGuidInfoUrl = false, ParseSizeInDescription = false };
        }
    }
}
