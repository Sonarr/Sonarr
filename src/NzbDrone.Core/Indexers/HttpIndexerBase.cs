using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.TPL;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Indexers
{
    public abstract class HttpIndexerBase<TSettings> : IndexerBase<TSettings>
        where TSettings : IProviderConfig, new()
    {
        protected const Int32 MaxNumResultsPerQuery = 1000;

        private readonly IHttpClient _httpClient;

        public override bool SupportsRss { get { return true; } }
        public override bool SupportsSearch { get { return true; } }
        public bool SupportsPaging { get { return PageSize > 0; } }

        public virtual Int32 PageSize { get { return 0; } }
        public virtual TimeSpan RateLimit { get { return TimeSpan.FromSeconds(2); } }

        public abstract IIndexerRequestGenerator GetRequestGenerator();
        public abstract IParseIndexerResponse GetParser();

        public HttpIndexerBase(IHttpClient httpClient, IIndexerStatusService indexerStatusService, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(indexerStatusService, configService, parsingService, logger)
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

            return FetchReleases(generator.GetRecentRequests(), true);
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

        protected virtual IList<ReleaseInfo> FetchReleases(IList<IEnumerable<IndexerRequest>> pageableRequests, bool isRecent = false)
        {
            var releases = new List<ReleaseInfo>();
            var url = String.Empty;

            var parser = GetParser();

            try
            {
                var fullyUpdated = false;
                ReleaseInfo lastReleaseInfo = null;
                if (isRecent)
                {
                    var indexerStatus = _indexerStatusService.GetIndexerStatus(Definition.Id);
                    if (indexerStatus != null)
                    {
                        lastReleaseInfo = indexerStatus.LastRssSyncReleaseInfo;
                    }
                }

                foreach (var pageableRequest in pageableRequests)
                {
                    var pagedReleases = new List<ReleaseInfo>();

                    foreach (var request in pageableRequest)
                    {
                        url = request.Url.ToString();

                        var page = FetchPage(request, parser);

                        pagedReleases.AddRange(page);

                        if (isRecent)
                        {
                            if (lastReleaseInfo == null)
                            {
                                fullyUpdated = true;
                                break;
                            }
                            var oldestDate = page.Select(v => v.PublishDate).Min();
                            if (oldestDate < lastReleaseInfo.PublishDate || page.Any(v => v.DownloadUrl == lastReleaseInfo.DownloadUrl))
                            {
                                fullyUpdated = true;
                                break;
                            }

                            if (pagedReleases.Count >= MaxNumResultsPerQuery)
                            {
                                if (oldestDate < DateTime.UtcNow - TimeSpan.FromHours(24))
                                {
                                    fullyUpdated = false;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (pagedReleases.Count >= MaxNumResultsPerQuery)
                            {
                                break;
                            }
                        }

                        if (!IsFullPage(page))
                        {
                            break;
                        }
                    }

                    releases.AddRange(pagedReleases);
                }

                if (isRecent && !releases.Empty())
                {
                    lastReleaseInfo = releases.OrderByDescending(v => v.PublishDate).First();
                    _indexerStatusService.UpdateRecentSearchStatus(Definition.Id, lastReleaseInfo, fullyUpdated);
                }

                _indexerStatusService.ReportSuccess(Definition.Id);
            }
            catch (WebException webException)
            {
                if (webException.Message.Contains("502") || webException.Message.Contains("503") ||
                    webException.Message.Contains("timed out"))
                {
                    _indexerStatusService.ReportFailure(Definition.Id);
                    _logger.Warn("{0} server is currently unavailable. {1} {2}", this, url, webException.Message);
                }
                else
                {
                    _indexerStatusService.ReportFailure(Definition.Id);
                    _logger.Warn("{0} {1} {2}", this, url, webException.Message);
                }
            }
            catch (HttpException httpException)
            {
                if ((int)httpException.Response.StatusCode == 429)
                {
                    _indexerStatusService.ReportFailure(Definition.Id, TimeSpan.FromHours(1));
                    _logger.Warn("API Request Limit reached for {0}", this);
                }
                else
                {
                    _indexerStatusService.ReportFailure(Definition.Id);
                    _logger.Warn("{0} {1}", this, httpException.Message);
                }
            }
            catch (RequestLimitReachedException)
            {
                _indexerStatusService.ReportFailure(Definition.Id, TimeSpan.FromHours(1));
                _logger.Warn("API Request Limit reached for {0}", this);
            }
            catch (ApiKeyException)
            {
                _indexerStatusService.ReportFailure(Definition.Id);
                _logger.Warn("Invalid API Key for {0} {1}", this, url);
            }
            catch (IndexerException ex)
            {
                _indexerStatusService.ReportFailure(Definition.Id);
                var message = String.Format("{0} - {1}", ex.Message, url);
                _logger.WarnException(message, ex);
            }
            catch (Exception feedEx)
            {
                _indexerStatusService.ReportFailure(Definition.Id);
                feedEx.Data.Add("FeedUrl", url);
                _logger.ErrorException("An error occurred while processing feed. " + url, feedEx);
            }

            return CleanupReleases(releases);
        }

        protected virtual Boolean IsFullPage(IList<ReleaseInfo> page)
        {
            return PageSize != 0 && page.Count >= PageSize;
        }

        protected virtual IList<ReleaseInfo> FetchPage(IndexerRequest request, IParseIndexerResponse parser)
        {
            var response = FetchIndexerResponse(request);

            return parser.ParseResponse(response).ToList();
        }

        protected virtual IndexerResponse FetchIndexerResponse(IndexerRequest request)
        {
            _logger.Debug("Downloading Feed " + request.Url);

            if (request.HttpRequest.RateLimit < RateLimit)
            {
                request.HttpRequest.RateLimit = RateLimit;
            }

            return new IndexerResponse(request, _httpClient.Execute(request.HttpRequest));
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(TestConnection());
        }

        protected virtual ValidationFailure TestConnection()
        {
            try
            {
                var parser = GetParser();
                var generator = GetRequestGenerator();
                var releases = FetchPage(generator.GetRecentRequests().First().First(), parser);

                if (releases.Empty())
                {
                    return new ValidationFailure(string.Empty, "No results were returned from your indexer, please check your settings.");
                }
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
            catch (UnsupportedFeedException ex)
            {
                _logger.WarnException("Indexer feed is not supported: " + ex.Message, ex);

                return new ValidationFailure(string.Empty, "Indexer feed is not supported: " + ex.Message);
            }
            catch (Exception ex)
            {
                _logger.WarnException("Unable to connect to indexer: " + ex.Message, ex);

                return new ValidationFailure(string.Empty, "Unable to connect to indexer, check the log for more details");
            }

            return null;
        }
    }

}
