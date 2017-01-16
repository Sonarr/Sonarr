using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NLog;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Indexers.BitMeTv
{
    public class BitMeTv : HttpIndexerBase<BitMeTvSettings>
    {
        public override string Name => "BitMeTV";

        public override DownloadProtocol Protocol => DownloadProtocol.Torrent;
        public override bool SupportsSearch => false;
        public override int PageSize => 0;

        public BitMeTv(IHttpClient httpClient, IIndexerStatusService indexerStatusService, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, indexerStatusService, configService, parsingService, logger)
        {

        }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new BitMeTvRequestGenerator() { Settings = Settings };
        }

        public override IParseIndexerResponse GetParser()
        {
            return new TorrentRssParser() { ParseSizeInDescription = true };
        }
    }
}