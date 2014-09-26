using System;
using System.Collections.Generic;
using NzbDrone.Core.ThingiProvider;
using FluentValidation.Results;
using System.Linq;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;
using NLog;

namespace NzbDrone.Core.Indexers.Eztv
{
    public class Eztv : HttpIndexerBase<EztvSettings>
    {
        public override DownloadProtocol Protocol { get { return DownloadProtocol.Torrent; } }

        public Eztv(IHttpClient httpClient, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, configService, parsingService, logger)
        {

        }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new EztvRequestGenerator() { Settings = Settings };
        }

        public override IParseIndexerResponse GetParser()
        {
            return new EzrssTorrentRssParser();
        }
    }
}