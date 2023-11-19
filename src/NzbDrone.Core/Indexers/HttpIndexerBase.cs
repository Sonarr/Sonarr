using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Http.CloudFlare;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers
{
    public abstract class HttpIndexerBase<TSettings> : IndexerBase<TSettings>
        where TSettings : IIndexerSettings, new()
    {
        protected const int MaxNumResultsPerQuery = 1000;

        protected readonly IHttpClient _httpClient;

        public override bool SupportsRss => true;
        public override bool SupportsSearch => true;
        public bool SupportsPaging => PageSize > 0;

        public virtual int PageSize => 0;
        public virtual TimeSpan RateLimit => TimeSpan.FromSeconds(2);

        public abstract IIndexerRequestGenerator GetRequestGenerator();
        public abstract IParseIndexerResponse GetParser();

        public HttpIndexerBase(IHttpClient httpClient, IIndexerStatusService indexerStatusService, IConfigService configService, IParsingService parsingService, Logger logger, ILocalizationService localizationService)
            : base(indexerStatusService, configService, parsingService, logger, localizationService)
        {
            _httpClient = httpClient;
        }

        public override Task<IList<ReleaseInfo>> FetchRecent()
        {
            if (!SupportsRss)
            {
                return Task.FromResult<IList<ReleaseInfo>>(Array.Empty<ReleaseInfo>());
            }

            return FetchReleases(g => g.GetRecentRequests(), true);
        }

        public override Task<IList<ReleaseInfo>> Fetch(SingleEpisodeSearchCriteria searchCriteria)
        {
            if (!SupportsSearch)
            {
                return Task.FromResult<IList<ReleaseInfo>>(Array.Empty<ReleaseInfo>());
            }

            return FetchReleases(g => g.GetSearchRequests(searchCriteria));
        }

        public override Task<IList<ReleaseInfo>> Fetch(SeasonSearchCriteria searchCriteria)
        {
            if (!SupportsSearch)
            {
                return Task.FromResult<IList<ReleaseInfo>>(Array.Empty<ReleaseInfo>());
            }

            return FetchReleases(g => g.GetSearchRequests(searchCriteria));
        }

        public override Task<IList<ReleaseInfo>> Fetch(DailyEpisodeSearchCriteria searchCriteria)
        {
            if (!SupportsSearch)
            {
                return Task.FromResult<IList<ReleaseInfo>>(Array.Empty<ReleaseInfo>());
            }

            return FetchReleases(g => g.GetSearchRequests(searchCriteria));
        }

        public override Task<IList<ReleaseInfo>> Fetch(DailySeasonSearchCriteria searchCriteria)
        {
            if (!SupportsSearch)
            {
                return Task.FromResult<IList<ReleaseInfo>>(Array.Empty<ReleaseInfo>());
            }

            return FetchReleases(g => g.GetSearchRequests(searchCriteria));
        }

        public override Task<IList<ReleaseInfo>> Fetch(AnimeEpisodeSearchCriteria searchCriteria)
        {
            if (!SupportsSearch)
            {
                return Task.FromResult<IList<ReleaseInfo>>(Array.Empty<ReleaseInfo>());
            }

            return FetchReleases(g => g.GetSearchRequests(searchCriteria));
        }

        public override Task<IList<ReleaseInfo>> Fetch(AnimeSeasonSearchCriteria searchCriteria)
        {
            if (!SupportsSearch)
            {
                return Task.FromResult<IList<ReleaseInfo>>(Array.Empty<ReleaseInfo>());
            }

            return FetchReleases(g => g.GetSearchRequests(searchCriteria));
        }

        public override Task<IList<ReleaseInfo>> Fetch(SpecialEpisodeSearchCriteria searchCriteria)
        {
            if (!SupportsSearch)
            {
                return Task.FromResult<IList<ReleaseInfo>>(Array.Empty<ReleaseInfo>());
            }

            return FetchReleases(g => g.GetSearchRequests(searchCriteria));
        }

        public override HttpRequest GetDownloadRequest(string link)
        {
            return new HttpRequest(link);
        }

        protected virtual async Task<IList<ReleaseInfo>> FetchReleases(Func<IIndexerRequestGenerator, IndexerPageableRequestChain> pageableRequestChainSelector, bool isRecent = false)
        {
            var releases = new List<ReleaseInfo>();
            var url = string.Empty;
            var minimumBackoff = TimeSpan.FromHours(1);

            try
            {
                var generator = GetRequestGenerator();
                var parser = GetParser();

                var pageableRequestChain = pageableRequestChainSelector(generator);

                var fullyUpdated = false;
                ReleaseInfo lastReleaseInfo = null;
                if (isRecent)
                {
                    lastReleaseInfo = _indexerStatusService.GetLastRssSyncReleaseInfo(Definition.Id);
                }

                for (var i = 0; i < pageableRequestChain.Tiers; i++)
                {
                    var pageableRequests = pageableRequestChain.GetTier(i);

                    foreach (var pageableRequest in pageableRequests)
                    {
                        var pagedReleases = new List<ReleaseInfo>();

                        foreach (var request in pageableRequest)
                        {
                            url = request.Url.FullUri;

                            var page = await FetchPage(request, parser);

                            pagedReleases.AddRange(page);

                            if (isRecent && page.Any())
                            {
                                if (lastReleaseInfo == null)
                                {
                                    fullyUpdated = true;
                                    break;
                                }

                                var oldestReleaseDate = page.Select(v => v.PublishDate).Min();
                                if (oldestReleaseDate < lastReleaseInfo.PublishDate || page.Any(v => v.DownloadUrl == lastReleaseInfo.DownloadUrl))
                                {
                                    fullyUpdated = true;
                                    break;
                                }

                                if (pagedReleases.Count >= MaxNumResultsPerQuery &&
                                    oldestReleaseDate < DateTime.UtcNow - TimeSpan.FromHours(24))
                                {
                                    fullyUpdated = false;
                                    break;
                                }
                            }
                            else if (pagedReleases.Count >= MaxNumResultsPerQuery)
                            {
                                break;
                            }

                            if (!IsFullPage(page))
                            {
                                break;
                            }
                        }

                        releases.AddRange(pagedReleases.Where(IsValidRelease));
                    }

                    if (releases.Any())
                    {
                        break;
                    }
                }

                if (isRecent && !releases.Empty())
                {
                    var ordered = releases.OrderByDescending(v => v.PublishDate).ToList();

                    if (!fullyUpdated && lastReleaseInfo != null)
                    {
                        var gapStart = lastReleaseInfo.PublishDate;
                        var gapEnd = ordered.Last().PublishDate;
                        _logger.Warn("Indexer {0} rss sync didn't cover the period between {1} and {2} UTC. Search may be required.", Definition.Name, gapStart, gapEnd);
                    }

                    lastReleaseInfo = ordered.First();
                    _indexerStatusService.UpdateRssSyncStatus(Definition.Id, lastReleaseInfo);
                }

                _indexerStatusService.RecordSuccess(Definition.Id);
            }
            catch (WebException webException)
            {
                if (webException.Status is WebExceptionStatus.NameResolutionFailure or WebExceptionStatus.ConnectFailure)
                {
                    _indexerStatusService.RecordConnectionFailure(Definition.Id);
                }
                else
                {
                    _indexerStatusService.RecordFailure(Definition.Id);
                }

                if (webException.Message.Contains("502") || webException.Message.Contains("503") ||
                    webException.Message.Contains("504") || webException.Message.Contains("timed out"))
                {
                    _logger.Warn("{0} server is currently unavailable. {1} {2}", this, url, webException.Message);
                }
                else
                {
                    _logger.Warn("{0} {1} {2}", this, url, webException.Message);
                }
            }
            catch (TooManyRequestsException ex)
            {
                var retryTime = ex.RetryAfter != TimeSpan.Zero ? ex.RetryAfter : minimumBackoff;
                _indexerStatusService.RecordFailure(Definition.Id, retryTime);

                _logger.Warn("API Request Limit reached for {0}. Disabled for {1}", this, retryTime);
            }
            catch (HttpException ex)
            {
                _indexerStatusService.RecordFailure(Definition.Id);
                if (ex.Response.HasHttpServerError)
                {
                    _logger.Warn("Unable to connect to {0} at [{1}]. Indexer's server is unavailable. Try again later. {2}", this, url, ex.Message);
                }
                else
                {
                    _logger.Warn("{0} {1}", this, ex.Message);
                }
            }
            catch (RequestLimitReachedException ex)
            {
                var retryTime = ex.RetryAfter != TimeSpan.Zero ? ex.RetryAfter : minimumBackoff;
                _indexerStatusService.RecordFailure(Definition.Id, retryTime);

                _logger.Warn("API Request Limit reached for {0}. Disabled for {1}", this, retryTime);
            }
            catch (ApiKeyException)
            {
                _indexerStatusService.RecordFailure(Definition.Id);
                _logger.Warn("Invalid API Key for {0} {1}", this, url);
            }
            catch (CloudFlareCaptchaException ex)
            {
                _indexerStatusService.RecordFailure(Definition.Id);
                ex.WithData("FeedUrl", url);
                if (ex.IsExpired)
                {
                    _logger.Error(ex, "Expired CAPTCHA token for {0}, please refresh in indexer settings.", this);
                }
                else
                {
                    _logger.Error(ex, "CAPTCHA token required for {0}, check indexer settings.", this);
                }
            }
            catch (TaskCanceledException ex)
            {
                _indexerStatusService.RecordFailure(Definition.Id);
                _logger.Warn(ex, "Unable to connect to indexer, possibly due to a timeout. {0}", url);
            }
            catch (IndexerException ex)
            {
                _indexerStatusService.RecordFailure(Definition.Id);
                _logger.Warn(ex, "{0}", url);
            }
            catch (Exception ex)
            {
                _indexerStatusService.RecordFailure(Definition.Id);
                ex.WithData("FeedUrl", url);
                _logger.Error(ex, "An error occurred while processing feed. {0}", url);
            }

            return CleanupReleases(releases);
        }

        protected virtual bool IsValidRelease(ReleaseInfo release)
        {
            if (release.Title.IsNullOrWhiteSpace())
            {
                _logger.Trace("Invalid Release: '{0}' from indexer: {1}. No title provided.", release.InfoUrl, Definition.Name);

                return false;
            }

            if (release.DownloadUrl.IsNullOrWhiteSpace())
            {
                _logger.Trace("Invalid Release: '{0}' from indexer: {1}. No Download URL provided.", release.Title, Definition.Name);

                return false;
            }

            return true;
        }

        protected virtual bool IsFullPage(IList<ReleaseInfo> page)
        {
            return PageSize != 0 && page.Count >= PageSize;
        }

        protected virtual async Task<IList<ReleaseInfo>> FetchPage(IndexerRequest request, IParseIndexerResponse parser)
        {
            var response = await FetchIndexerResponse(request);

            try
            {
                return parser.ParseResponse(response).ToList();
            }
            catch (Exception ex)
            {
                ex.WithData(response.HttpResponse, 128 * 1024);
                _logger.Trace("Unexpected Response content ({0} bytes): {1}", response.HttpResponse.ResponseData.Length, response.HttpResponse.Content);
                throw;
            }
        }

        protected virtual async Task<IndexerResponse> FetchIndexerResponse(IndexerRequest request)
        {
            _logger.Debug("Downloading Feed " + request.HttpRequest.ToString(false));

            if (request.HttpRequest.RateLimit < RateLimit)
            {
                request.HttpRequest.RateLimit = RateLimit;
            }

            request.HttpRequest.RateLimitKey = Definition.Id.ToString();

            var response = await _httpClient.ExecuteAsync(request.HttpRequest);

            return new IndexerResponse(request, response);
        }

        protected override async Task Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(await TestConnection());
        }

        protected virtual async Task<ValidationFailure> TestConnection()
        {
            try
            {
                var parser = GetParser();
                var generator = GetRequestGenerator();
                var firstRequest = generator.GetRecentRequests().GetAllTiers().FirstOrDefault()?.FirstOrDefault();

                if (firstRequest == null)
                {
                    return new ValidationFailure(string.Empty, _localizationService.GetLocalizedString("IndexerValidationJackettNoRssFeedQueryAvailable"));
                }

                var releases = await FetchPage(firstRequest, parser);

                if (releases.Empty())
                {
                    return new ValidationFailure(string.Empty, _localizationService.GetLocalizedString("IndexerValidationJackettNoResultsInConfiguredCategories"));
                }
            }
            catch (ApiKeyException ex)
            {
                _logger.Warn("Indexer returned result for RSS URL, API Key appears to be invalid: " + ex.Message);

                return new ValidationFailure("ApiKey", _localizationService.GetLocalizedString("IndexerValidationInvalidApiKey"));
            }
            catch (RequestLimitReachedException ex)
            {
                _logger.Warn("Request limit reached: " + ex.Message);

                return new ValidationFailure(string.Empty, _localizationService.GetLocalizedString("IndexerValidationRequestLimitReached", new Dictionary<string, object> { { "exceptionMessage", ex.Message } }));
            }
            catch (CloudFlareCaptchaException ex)
            {
                if (ex.IsExpired)
                {
                    return new ValidationFailure("CaptchaToken", _localizationService.GetLocalizedString("IndexerValidationCloudFlareCaptchaExpired"));
                }
                else
                {
                    return new ValidationFailure("CaptchaToken", _localizationService.GetLocalizedString("IndexerValidationCloudFlareCaptchaRequired"));
                }
            }
            catch (UnsupportedFeedException ex)
            {
                _logger.Warn(ex, "Indexer feed is not supported");

                return new ValidationFailure(string.Empty, _localizationService.GetLocalizedString("IndexerValidationFeedNotSupported", new Dictionary<string, object> { { "exceptionMessage", ex.Message } }));
            }
            catch (IndexerException ex)
            {
                _logger.Warn(ex, "Unable to connect to indexer");

                return new ValidationFailure(string.Empty, _localizationService.GetLocalizedString("IndexerValidationUnableToConnect", new Dictionary<string, object> { { "exceptionMessage", ex.Message } }));
            }
            catch (HttpException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.BadRequest &&
                    ex.Response.Content.Contains("not support the requested query"))
                {
                    _logger.Warn(ex, "Indexer does not support the query");
                    return new ValidationFailure(string.Empty, _localizationService.GetLocalizedString("IndexerValidationQuerySeasonEpisodesNotSupported"));
                }

                _logger.Warn(ex, "Unable to connect to indexer");
                if (ex.Response.HasHttpServerError)
                {
                    return new ValidationFailure(string.Empty, _localizationService.GetLocalizedString("IndexerValidationUnableToConnectServerUnavailable", new Dictionary<string, object> { { "exceptionMessage", ex.Message } }));
                }

                if (ex.Response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized)
                {
                    return new ValidationFailure(string.Empty, _localizationService.GetLocalizedString("IndexerValidationUnableToConnectInvalidCredentials", new Dictionary<string, object> { { "exceptionMessage", ex.Message } }));
                }

                return new ValidationFailure(string.Empty, _localizationService.GetLocalizedString("IndexerValidationUnableToConnect", new Dictionary<string, object> { { "exceptionMessage", ex.Message } }));
            }
            catch (HttpRequestException ex)
            {
                _logger.Warn(ex, "Unable to connect to indexer");

                return new ValidationFailure(string.Empty, _localizationService.GetLocalizedString("IndexerValidationUnableToConnectHttpError", new Dictionary<string, object> { { "exceptionMessage", ex.Message } }));
            }
            catch (TaskCanceledException ex)
            {
                _logger.Warn(ex, "Unable to connect to indexer");

                return new ValidationFailure(string.Empty, _localizationService.GetLocalizedString("IndexerValidationUnableToConnectTimeout", new Dictionary<string, object> { { "exceptionMessage", ex.Message } }));
            }
            catch (WebException webException)
            {
                _logger.Warn("Unable to connect to indexer.");

                if (webException.Status is WebExceptionStatus.NameResolutionFailure or WebExceptionStatus.ConnectFailure)
                {
                    return new ValidationFailure(string.Empty, _localizationService.GetLocalizedString("IndexerValidationUnableToConnectResolutionFailure", new Dictionary<string, object> { { "exceptionMessage", webException.Message } }));
                }

                if (webException.Message.Contains("502") || webException.Message.Contains("503") ||
                    webException.Message.Contains("504") || webException.Message.Contains("timed out"))
                {
                    return new ValidationFailure(string.Empty, _localizationService.GetLocalizedString("IndexerValidationUnableToConnectServerUnavailable", new Dictionary<string, object> { { "exceptionMessage", webException.Message } }));
                }
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Unable to connect to indexer");

                return new ValidationFailure(string.Empty, _localizationService.GetLocalizedString("IndexerValidationUnableToConnect", new Dictionary<string, object> { { "exceptionMessage", ex.Message } }));
            }

            return null;
        }
    }
}
