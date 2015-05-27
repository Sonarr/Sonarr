using System;
using System.Collections.Generic;
using System.Threading;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.Rarbg
{
    public class Rarbg : HttpIndexerBase<RarbgSettings>
    {
        private readonly IRarbgTokenProvider _tokenProvider;
        private static DateTime _lastFetch;

        public override string Name { get { return "Rarbg"; } }
        
        public override DownloadProtocol Protocol { get { return DownloadProtocol.Torrent; } }

        public Rarbg(IRarbgTokenProvider tokenProvider, IHttpClient httpClient, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, configService, parsingService, logger)
        {
            _tokenProvider = tokenProvider;
        }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new RarbgRequestGenerator(_tokenProvider) { Settings = Settings };
        }

        public override IParseIndexerResponse GetParser()
        {
            return new RarbgParser();
        }

        protected override IList<ReleaseInfo> FetchPage(IndexerRequest request, IParseIndexerResponse parser)
        {
            var delay = _lastFetch + TimeSpan.FromSeconds(10) - DateTime.UtcNow;
            if (delay.TotalSeconds > 0)
            {
                Thread.Sleep(delay);
            }

            _lastFetch = DateTime.UtcNow;

            return base.FetchPage(request, parser);
        }
    }
}