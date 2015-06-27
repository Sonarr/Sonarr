using System;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Indexers.HDBits
{
    public class HDBits : HttpIndexerBase<HDBitsSettings>
    {
        public override string Name { get { return "HDBits"; } }
        public override DownloadProtocol Protocol { get { return DownloadProtocol.Torrent; } }
        public override bool SupportsRss { get { return true; } }
        public override bool SupportsSearch { get { return true; } }
        public override int PageSize { get { return 30; } }

        public HDBits(IHttpClient httpClient, IIndexerStatusService indexerStatusService, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, indexerStatusService, configService, parsingService, logger)
        { }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new HDBitsRequestGenerator() { Settings = Settings };
        }

        public override IParseIndexerResponse GetParser()
        {
            return new HDBitsParser(Settings);
        }
    }
}
