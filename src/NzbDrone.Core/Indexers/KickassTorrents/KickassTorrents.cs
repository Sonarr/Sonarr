using System;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Indexers.KickassTorrents
{
    public class KickassTorrents : HttpIndexerBase<KickassTorrentsSettings>
    {
        public override string Name
        {
            get
            {
                return "Kickass Torrents";
            }
        }

        public override DownloadProtocol Protocol { get { return DownloadProtocol.Torrent; } }
        public override int PageSize { get { return 25; } }

        public KickassTorrents(IHttpClient httpClient, IIndexerStatusService indexerStatusService, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, indexerStatusService, configService, parsingService, logger)
        {

        }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new KickassTorrentsRequestGenerator() { Settings = Settings, PageSize = PageSize };
        }

        public override IParseIndexerResponse GetParser()
        {
            return new KickassTorrentsRssParser() { Settings = Settings };
        }
    }
}