using System;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;
using NLog;

namespace NzbDrone.Core.Indexers.ImmortalSeed
{
    public class ImmortalSeed : HttpIndexerBase<ImmortalSeedSettings>
    {
        public override DownloadProtocol Protocol { get { return DownloadProtocol.Torrent; } }
        public override Boolean SupportsSearch { get { return false; } }
        public override Int32 PageSize { get { return 0; } }

        public ImmortalSeed(IHttpClient httpClient, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, configService, parsingService, logger)
        {

        }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new ImmortalSeedRequestGenerator() { Settings = Settings };
        }

        public override IParseIndexerResponse GetParser()
        {
            return new TorrentRssParser() { UseGuidInfoUrl = false, ParseSeedersInDescription = true, ParseSizeInDescription = true };
        }
    }
}
