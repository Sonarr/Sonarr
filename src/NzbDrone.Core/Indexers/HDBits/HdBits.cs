using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;
using System;

namespace NzbDrone.Core.Indexers.HDBits
{
    public class HdBits : HttpIndexerBase<HdBitsSettings>
    {
        public override DownloadProtocol Protocol { get { return DownloadProtocol.Torrent; } }
        public override bool SupportsRss { get { return true; } }
        public override bool SupportsSearch { get { return true; } }
        public override int PageSize { get { return 30; } }

        public HdBits(IHttpClient httpClient,
                      IConfigService configService,
                      IParsingService parsingService,
                      Logger logger)
            : base(httpClient, configService, parsingService, logger)
        { }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new HdBitsRequestGenerator() { Settings = Settings };
        }

        public override IParseIndexerResponse GetParser()
        {
            return new HdBitsParser(Settings);
        }
    }
}
