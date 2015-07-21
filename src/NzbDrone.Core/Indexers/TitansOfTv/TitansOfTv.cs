using System;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Indexers.TitansOfTv
{
    public class TitansOfTv : HttpIndexerBase<TitansOfTvSettings>
    {
        public TitansOfTv(IHttpClient httpClient, IIndexerStatusService indexerStatusService, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, indexerStatusService, configService, parsingService, logger)
        {

        }

        public override string Name
        {
            get { return "Titans of TV"; }
        }

        public override DownloadProtocol Protocol { get { return DownloadProtocol.Torrent; } }
        public override bool SupportsRss { get { return true; } }
        public override bool SupportsSearch { get { return true; } }
        public override int PageSize { get { return 100; } }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new TitansOfTvRequestGenerator() { Settings = Settings, PageSize = PageSize };
        }

        public override IParseIndexerResponse GetParser()
        {
            return new TitansOfTvParser();
        }
    }
}
