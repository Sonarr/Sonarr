using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Indexers.Animezb
{
    public class Animezb : RssIndexerBase<NullConfig>
    {
        public override DownloadProtocol Protocol { get { return DownloadProtocol.Usenet; } }

        public Animezb(IHttpClient httpClient, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, configService, parsingService, logger)
        {

        }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new AnimezbRequestGenerator();
        }

        public override IParseIndexerResponse GetParser()
        {
            return new RssParser() { UseEnclosureLength = true };
        }
    }
}
