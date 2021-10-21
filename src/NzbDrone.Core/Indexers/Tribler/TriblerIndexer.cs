using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Http.CloudFlare;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Validation;
using Tribler.Api;

namespace NzbDrone.Core.Indexers.Tribler
{
    public class TriblerIndexer : HttpIndexerBase<TriblerIndexerSettings>
    {
        public override string Name => "Tribler";

        public override DownloadProtocol Protocol => DownloadProtocol.Torrent;

        // TODO: Consider using channels as rss feed.
        public override bool SupportsRss => false;
        public override bool SupportsSearch => true;

        public override int PageSize => 50;

        // this is not a public service so we do no not have to handle rate limitations.
        public override TimeSpan RateLimit => TimeSpan.FromSeconds(0);

        public TriblerIndexer(IHttpClient httpClient, IIndexerStatusService indexerStatusService, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, indexerStatusService, configService, parsingService, logger)
        {
        }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new TriblerIndexerRequestGenerator() { Settings = Settings };
        }

        public override IParseIndexerResponse GetParser()
        {
            return new TriblerIndexerResponseParser(Settings);
        }

        public override IList<ReleaseInfo> FetchRecent()
        {
            return new List<ReleaseInfo>();
        }

        // override the default Test command as it requires support for rss feed.
        protected override void Test(List<ValidationFailure> failures)
        {
            NzbDroneValidationResult nzbDroneValidationResult = Settings.Validate();
            failures.AddRange(nzbDroneValidationResult.Errors);

            try
            {
                // fake a search for "Ubuntu"
                var requestGenerator = (TriblerIndexerRequestGenerator)GetRequestGenerator();

                var indexerRequest = requestGenerator.GetRequest("Ubuntu").First();
                var httpResponse = _httpClient.Execute(indexerRequest.HttpRequest);
                var releaseInfos = GetParser().ParseResponse(new IndexerResponse(indexerRequest, httpResponse));

                if (releaseInfos == null || releaseInfos.Count == 0)
                {
                    failures.Add(new ValidationFailure("search", "returned no results"));
                }

            }
            catch (ApiKeyException ex)
            {
                _logger.Warn("Indexer returned result for RSS URL, API Key appears to be invalid: " + ex.Message);

                failures.Add(new ValidationFailure("ApiKey", "Invalid API Key"));
            }
            catch (IndexerException ex)
            {
                _logger.Warn(ex, "Unable to connect to indexer");

                failures.Add(new ValidationFailure(string.Empty, "Unable to connect to indexer. " + ex.Message));
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Unable to connect to indexer");

                failures.Add(new ValidationFailure(string.Empty, "Unable to connect to indexer, check the log for more details"));
            }
        }

    }
}