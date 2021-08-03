using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Http.CloudFlare;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.ThingiProvider;

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

            return FetchReleases(g => g.GetRecentRequests(), true);
        }

        public override IList<ReleaseInfo> Fetch(SingleEpisodeSearchCriteria searchCriteria)
        {
            if (!SupportsSearch)
            {
                return new List<ReleaseInfo>();
            }

            return FetchReleases(g => g.GetSearchRequests(searchCriteria));
        }

        public override IList<ReleaseInfo> Fetch(SeasonSearchCriteria searchCriteria)
        {
            if (!SupportsSearch)
            {
                return new List<ReleaseInfo>();
            }

            return FetchReleases(g => g.GetSearchRequests(searchCriteria));
        }

        public override IList<ReleaseInfo> Fetch(DailyEpisodeSearchCriteria searchCriteria)
        {
            if (!SupportsSearch)
            {
                return new List<ReleaseInfo>();
            }

            return FetchReleases(g => g.GetSearchRequests(searchCriteria));
        }

        public override IList<ReleaseInfo> Fetch(DailySeasonSearchCriteria searchCriteria)
        {
            if (!SupportsSearch)
            {
                return new List<ReleaseInfo>();
            }

            return FetchReleases(g => g.GetSearchRequests(searchCriteria));
        }

        public override IList<ReleaseInfo> Fetch(AnimeEpisodeSearchCriteria searchCriteria)
        {
            if (!SupportsSearch)
            {
                return new List<ReleaseInfo>();
            }

            return FetchReleases(g => g.GetSearchRequests(searchCriteria));
        }

        public override IList<ReleaseInfo> Fetch(SpecialEpisodeSearchCriteria searchCriteria)
        {
            if (!SupportsSearch)
            {
                return new List<ReleaseInfo>();
            }

            return FetchReleases(g => g.GetSearchRequests(searchCriteria));
        }

        protected virtual IList<ReleaseInfo> FetchReleases(Func<IIndexerRequestGenerator, IndexerPageableRequestChain> pageableRequestChainSelector, bool isRecent = false)
        {
            var releases = new List<ReleaseInfo>();
            var url = string.Empty;

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

                for (int i = 0; i < pageableRequestChain.Tiers; i++)
                {
                    var pageableRequests = pageableRequestChain.GetTier(i);

                    foreach (var pageableRequest in pageableRequests)
                    {
                        var pagedReleases = new List<ReleaseInfo>();

                        foreach (var request in pageableRequest)
                        {
                            url = request.Url.FullUri;

                            var page = FetchPage(request, parser);

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
                if (webException.Status == WebExceptionStatus.NameResolutionFailure ||
                    webException.Status == WebExceptionStatus.ConnectFailure)
                {
                    _indexerStatusService.RecordConnectionFailure(Definition.Id);
                }
                else
                {
                    _indexerStatusService.RecordFailure(Definition.Id);
                }

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
            catch (TooManyRequestsException ex)
            {
                if (ex.RetryAfter != TimeSpan.Zero)
                {
                    _indexerStatusService.RecordFailure(Definition.Id, ex.RetryAfter);
                }
                else
                {
                    _indexerStatusService.RecordFailure(Definition.Id, TimeSpan.FromHours(1));
                }

                _logger.Warn("API Request Limit reached for {0}", this);
            }
            catch (HttpException ex)
            {
                _indexerStatusService.RecordFailure(Definition.Id);
                _logger.Warn("{0} {1}", this, ex.Message);
            }
            catch (RequestLimitReachedException)
            {
                _indexerStatusService.RecordFailure(Definition.Id, TimeSpan.FromHours(1));
                _logger.Warn("API Request Limit reached for {0}", this);
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
            if (release.DownloadUrl.IsNullOrWhiteSpace())
            {
                _logger.Trace("Invalid Release: '{0}' from indexer: {1}. No Download URL provided.", release.Title, release.Indexer);
                return false;
            }

            return true;
        }

        protected virtual bool IsFullPage(IList<ReleaseInfo> page)
        {
            return PageSize != 0 && page.Count >= PageSize;
        }

        protected virtual IList<ReleaseInfo> FetchPage(IndexerRequest request, IParseIndexerResponse parser)
        {
            var response = FetchIndexerResponse(request);

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

        protected virtual IndexerResponse FetchIndexerResponse(IndexerRequest request)
        {
            _logger.Debug("Downloading Feed " + request.HttpRequest.ToString(false));

            if (request.HttpRequest.RateLimit < RateLimit)
            {
                request.HttpRequest.RateLimit = RateLimit;
            }

            request.HttpRequest.RateLimitKey = Definition.Id.ToString();

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
                var firstRequest = generator.GetRecentRequests().GetAllTiers().FirstOrDefault()?.FirstOrDefault();

                if (firstRequest == null)
                {
                    return new ValidationFailure(string.Empty, "No rss feed query available. This may be an issue with the indexer or your indexer category settings.");
                }

                var releases = FetchPage(firstRequest, parser);

                if (releases.Empty())
                {
                    return new ValidationFailure(string.Empty, "Query successful, but no results in the configured categories were returned from your indexer. This may be an issue with the indexer or your indexer category settings.");
                }
            }
            catch (ApiKeyException ex)
            {
                _logger.Warn("Indexer returned result for RSS URL, API Key appears to be invalid: " + ex.Message);

                return new ValidationFailure("ApiKey", "Invalid API Key");
            }
            catch (RequestLimitReachedException ex)
            {
                _logger.Warn("Request limit reached: " + ex.Message);
            }
            catch (CloudFlareCaptchaException ex)
            {
                if (ex.IsExpired)
                {
                    return new ValidationFailure("CaptchaToken", "CloudFlare CAPTCHA token expired, please Refresh.");
                }
                else
                {
                    return new ValidationFailure("CaptchaToken", "Site protected by CloudFlare CAPTCHA. Valid CAPTCHA token required.");
                }
            }
            catch (UnsupportedFeedException ex)
            {
                _logger.Warn(ex, "Indexer feed is not supported");

                return new ValidationFailure(string.Empty, "Indexer feed is not supported: " + ex.Message);
            }
            catch (IndexerException ex)
            {
                _logger.Warn(ex, "Unable to connect to indexer");

                return new ValidationFailure(string.Empty, "Unable to connect to indexer. " + ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Unable to connect to indexer");

                return new ValidationFailure(string.Empty, "Unable to connect to indexer, check the log for more details");
            }

            return null;
        }
    }
}
