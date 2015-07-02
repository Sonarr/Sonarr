using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Indexers.TitansOfTv
{
    public class TitansOfTv : HttpIndexerBase<TitansOfTvSettings>
    {
        public TitansOfTv(IHttpClient httpClient, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, configService, parsingService, logger)
        {

        }

        public override string Name
        {
            get
            {
                return "TitansOfTv";
            }
        }

        public override DownloadProtocol Protocol
        {
            get
            {
                return DownloadProtocol.Torrent;
            }
        }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new TitansOfTvRequestGenerator() { Settings = Settings };
        }

        public override IParseIndexerResponse GetParser()
        {
            return new TitansOfTvParser();
        }

        public override Boolean SupportsSearch { get { return true; } }
    }
}
