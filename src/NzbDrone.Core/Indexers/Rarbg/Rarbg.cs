using System;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Indexers.Rarbg
{
    public class Rarbg : HttpIndexerBase<RarbgSettings>
    {
        private readonly IRarbgTokenProvider _tokenProvider;
        private static DateTime _lastFetch;

        public override string Name { get { return "Rarbg"; } }
        
        public override DownloadProtocol Protocol { get { return DownloadProtocol.Torrent; } }
        public override TimeSpan RateLimit { get { return TimeSpan.FromSeconds(10); } }

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
    }
}