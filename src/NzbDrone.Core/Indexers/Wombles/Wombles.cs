using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Indexers.Wombles
{
    public class Wombles : RssIndexerBase<NullConfig>
    {
        public override DownloadProtocol Protocol { get { return DownloadProtocol.Usenet; } }
        public override bool SupportsSearch { get { return false; } }

        public override IParseIndexerResponse GetParser()
        {
            return new WomblesRssParser();
        }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new RssIndexerRequestGenerator("http://newshost.co.za/rss/?sec=TV&fr=false");
        }

        public Wombles(IHttpClient httpClient, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, configService, parsingService, logger)
        {

        }
    }
}