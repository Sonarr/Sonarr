using System;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Indexers.TorrentRssIndexer
{
    public class TorrentRssIndexer : HttpIndexerBase<TorrentRssIndexerSettings>
    {
        public override DownloadProtocol Protocol { get { return DownloadProtocol.Torrent; } }
        public override Boolean SupportsSearch { get { return false; } }
        public override Int32 PageSize { get { return 0; } }

        private readonly ITorrentRssParserFactory _torrentRssParserFactory;

        private readonly IIndexerRequestGenerator _indexerRequestGenerator;

        public TorrentRssIndexer(IHttpClient httpClient, IConfigService configService, IParsingService parsingService, ITorrentRssParserFactory torrentRssParserFactory, IIndexerRequestGenerator indexerRequestGenerator, Logger logger)
            : base(httpClient, configService, parsingService, logger)
        {
            _torrentRssParserFactory = torrentRssParserFactory;
            _indexerRequestGenerator = indexerRequestGenerator;
        }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return _indexerRequestGenerator;
        }

        public override IParseIndexerResponse GetParser()
        {
            return _torrentRssParserFactory.GetParser(Settings, FetchIndexerResponse);
        }
    }
}
