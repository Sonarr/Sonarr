using System;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Test.IndexerTests
{
    public class TestIndexer : HttpIndexerBase<TestIndexerSettings>
    {
        public override string Name
        {
            get
            {
                return "Test Indexer";
            }
        }

        public override DownloadProtocol Protocol { get { return DownloadProtocol.Usenet; } }

        public Int32 _supportedPageSize;
        public override Int32 PageSize { get { return _supportedPageSize; } }

        public TestIndexer(IHttpClient httpClient, IIndexerStatusService indexerStatusService, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, indexerStatusService, configService, parsingService, logger)
        {

        }

        public IIndexerRequestGenerator _requestGenerator;
        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return _requestGenerator;
        }

        public IParseIndexerResponse _parser;
        public override IParseIndexerResponse GetParser()
        {
            return _parser;
        }
    }
}
