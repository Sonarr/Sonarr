using System.Collections.Generic;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Indexers.TorrentRssIndexer;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Test.IndexerTests.TorrentRssIndexerTests
{
    public class TestTorrentRssIndexer : TorrentRssIndexer
    {
        public TestTorrentRssIndexer(IHttpClient httpClient, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, configService, parsingService, logger)
        {
        }

        public TorrentRssIndexerParserSettings ParserSettingsTest
        {
            get
            {
                return ParserSettings;
            }

            set
            {
                ParserSettings = value;
            }
        }

        public List<ValidationFailure> TestPublic()
        {
            var result = new List<ValidationFailure>();
            Test(result);
            return result;
        }
    }
}
