using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.Indexers.Newznab;

namespace NzbDrone.Core.Indexers
{
    public interface INewznabTestService
    {
        void Test(IIndexer indexer);
    }

    public class NewznabTestService : INewznabTestService
    {
        private readonly IFetchFeedFromIndexers _feedFetcher;
        private readonly IHttpProvider _httpProvider;
        private readonly Logger _logger;

        public NewznabTestService(IFetchFeedFromIndexers feedFetcher, IHttpProvider httpProvider, Logger logger)
        {
            _feedFetcher = feedFetcher;
            _httpProvider = httpProvider;
            _logger = logger;
        }

        public void Test(IIndexer indexer)
        {
            var releases = _feedFetcher.FetchRss(indexer);

            if (releases.Any()) return;

            try
            {
                var url = indexer.RecentFeed.First();
                var xml = _httpProvider.DownloadString(url);

                NewznabPreProcessor.Process(xml, url);
            }
            catch (ApiKeyException)
            {
                _logger.Warn("Indexer returned result for Newznab RSS URL, API Key appears to be invalid");

                var apiKeyFailure = new ValidationFailure("ApiKey", "Invalid API Key");
                throw new ValidationException(new List<ValidationFailure> { apiKeyFailure }.ToArray());
            }
            catch (Exception)
            {
                _logger.Warn("Indexer doesn't appear to be Newznab based");

                var failure = new ValidationFailure("Url", "Invalid Newznab URL entered");
                throw new ValidationException(new List<ValidationFailure> { failure }.ToArray());
            }
        }
    }
}
