using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Indexers
{
    public abstract class RssIndexerBase<TSettings> : IndexerBase<TSettings>
        where TSettings : IProviderConfig, new()
    {
        private const Int32 MaxNumResultsPerQuery = 1000;

        private readonly IHttpClient _httpClient;

        public override bool SupportsRss { get { return true; } }
        public override bool SupportsSearch { get { return true; } }
        public bool SupportsPaging { get { return PageSize > 0; } }

        public virtual Int32 PageSize { get { return 0; } }

        public abstract IIndexerRequestGenerator GetRequestGenerator();
        public abstract IParseIndexerResponse GetParser();

        public RssIndexerBase(IHttpClient httpClient, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(configService, parsingService, logger)
        {
            _httpClient = httpClient;
        }

        public override IList<ReleaseInfo> FetchRecent()
        {
            if (!SupportsRss)
            {
                return new List<ReleaseInfo>();
            }

            var generator = GetRequestGenerator();

            return FetchReleases(generator.GetRecentRequests());
        }

        public override IList<ReleaseInfo> Fetch(SingleEpisodeSearchCriteria searchCriteria)
        {
            if (!SupportsSearch)
            {
                return new List<ReleaseInfo>();
            }

            var generator = GetRequestGenerator();

            return FetchReleases(generator.GetSearchRequests(searchCriteria));
        }

        public override IList<ReleaseInfo> Fetch(SeasonSearchCriteria searchCriteria)
        {
            if (!SupportsSearch)
            {
                return new List<ReleaseInfo>();
            }

            var generator = GetRequestGenerator();

            return FetchReleases(generator.GetSearchRequests(searchCriteria));
        }

        public override IList<ReleaseInfo> Fetch(DailyEpisodeSearchCriteria searchCriteria)
        {
            if (!SupportsSearch)
            {
                return new List<ReleaseInfo>();
            }

            var generator = GetRequestGenerator();

            return FetchReleases(generator.GetSearchRequests(searchCriteria));
        }

        public override IList<ReleaseInfo> Fetch(AnimeEpisodeSearchCriteria searchCriteria)
        {
            if (!SupportsSearch)
            {
                return new List<ReleaseInfo>();
            }

            var generator = GetRequestGenerator();

            return FetchReleases(generator.GetSearchRequests(searchCriteria));
        }

        public override IList<ReleaseInfo> Fetch(SpecialEpisodeSearchCriteria searchCriteria)
        {
            if (!SupportsSearch)
            {
                return new List<ReleaseInfo>();
            }

            var generator = GetRequestGenerator();

            return FetchReleases(generator.GetSearchRequests(searchCriteria));
        }

        protected virtual IList<ReleaseInfo> FetchReleases(IList<IEnumerable<IndexerRequest>> pageableRequests)
        {
            var releases = new List<ReleaseInfo>();

            foreach (var pageableRequest in pageableRequests)
            {
                var pagedReleases = new List<ReleaseInfo>();

                foreach (var request in pageableRequest)
                {
                    var page = FetchPage(request);

                    pagedReleases.AddRange(page);

                    if (!IsFullPage(page) || pagedReleases.Count >= MaxNumResultsPerQuery)
                    {
                        break;
                    }
                }

                releases.AddRange(pagedReleases);
            }

            return CleanupReleases(releases);
        }

        protected virtual Boolean IsFullPage(IList<ReleaseInfo> page)
        {
            return PageSize != 0 && page.Count >= PageSize;
        }

        protected virtual IList<ReleaseInfo> FetchPage(IndexerRequest request)
        {
            var url = request.Url;

            try
            {
                _logger.Debug("Downloading Feed " + request.Url);
                request.Headers.Accept = "application/rss+xml, text/rss+xml, text/xml";
                var response = new IndexerResponse(_httpClient.Get(request));

                if (response.Headers.ContentType != null && response.Headers.ContentType.StartsWith("text/html"))
                {
                    throw new WebException("Indexer responded with html content. Site is likely blocked or unavailable.");
                }

                var xml = response.Content;
                if (!string.IsNullOrWhiteSpace(xml))
                {
                    return GetParser().ParseResponse(response).ToList();
                }
                else
                {
                    _logger.Warn("{0} returned empty response.", url);
                }
            }
            catch (WebException webException)
            {
                if (webException.Message.Contains("502") || webException.Message.Contains("503") ||
                    webException.Message.Contains("timed out"))
                {
                    _logger.Warn("{0} server is currently unavailable. {1} {2}", this, url, webException.Message);
                }
                else
                {
                    _logger.Warn("{0} {1} {2}", this, url, webException.Message);
                }
            }
            catch (ApiKeyException)
            {
                _logger.Warn("Invalid API Key for {0} {1}", this, url);
            }
            catch (Exception feedEx)
            {
                feedEx.Data.Add("FeedUrl", url);
                _logger.ErrorException("An error occurred while processing feed. " + url, feedEx);
            }

            return new List<ReleaseInfo>();
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(TestConnection());
        }

        protected virtual ValidationFailure TestConnection()
        {
            try
            {
                var releases = FetchRecent();

                if (releases.Any()) return null;
            }
            catch (ApiKeyException)
            {
                _logger.Warn("Indexer returned result for RSS URL, API Key appears to be invalid");

                return new ValidationFailure("ApiKey", "Invalid API Key");
            }
            catch (RequestLimitReachedException)
            {
                _logger.Warn("Request limit reached");
            }
            catch (Exception ex)
            {
                _logger.WarnException("Unable to connect to indexer: " + ex.Message, ex);

                return new ValidationFailure("Url", "Unable to connect to indexer, check the log for more details");
            }

            return null;
        }
    }

}
