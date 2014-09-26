using System;
using System.Collections.Generic;
using NzbDrone.Core.ThingiProvider;
using FluentValidation.Results;
using System.Linq;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NLog;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Indexers.BitMeTv
{
    public class BitMeTv : HttpIndexerBase<BitMeTvSettings>
    {
        public override DownloadProtocol Protocol { get { return DownloadProtocol.Torrent; } }
        public override Boolean SupportsSearch { get { return false; } }
        public override Int32 PageSize { get { return 0; } }

        public BitMeTv(IHttpClient httpClient, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, configService, parsingService, logger)
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