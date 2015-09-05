using Newtonsoft.Json;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace NzbDrone.Core.Indexers.GetStrike
{
    public class GetStrike : HttpIndexerBase<GetStrikeSettings>
    {
        public override string Name { get { return "GetStrike"; } }
        public override DownloadProtocol Protocol { get { return DownloadProtocol.Torrent; } }
        public override bool SupportsRss { get { return false; } }
        public override bool SupportsSearch { get { return true; } }
        public override int PageSize { get { return 30; } }

        public GetStrike(IHttpClient httpClient, IIndexerStatusService indexerStatusService, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, indexerStatusService, configService, parsingService, logger)
        { }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new GetStrikeRequestGenerator() { Settings = Settings };
        }

        public override IParseIndexerResponse GetParser()
        {
            return new GetStrikeParser(Settings);
        }

        protected override IList<ReleaseInfo> FetchReleases(IList<IEnumerable<IndexerRequest>> pageableRequests, bool isRecent = false)
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
                    lastReleaseInfo = _indexerStatusService.GetLastRssSyncReleaseInfo(Definition.Id);
                }

                foreach (var pageableRequest in pageableRequests)
                {
                    var pagedReleases = new List<ReleaseInfo>();

                    foreach (var request in pageableRequest)
                    {
                        url = request.Url.ToString();

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

                    releases.AddRange(pagedReleases);
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
                _indexerStatusService.RecordFailure(Definition.Id);
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
            catch (HttpException httpException)
            {
                if ((int)httpException.Response.StatusCode == 429)
                {
                    _indexerStatusService.RecordFailure(Definition.Id, TimeSpan.FromHours(1));
                    _logger.Warn("API Request Limit reached for {0}", this);
                }
                else if ((int)httpException.Response.StatusCode == 404)
                {
                    try
                    {
                        // Try parse not found message
                        GetStrikeNotFound notFound = JsonConvert.DeserializeObject<GetStrikeNotFound>(httpException.Response.Content);
                        if (notFound.statuscode == 404)
                        {
                            _indexerStatusService.RecordSuccess(Definition.Id);
                        }
                        else
                        {
                            _indexerStatusService.RecordFailure(Definition.Id);
                            _logger.Warn("{0} {1}", this, httpException.Message);
                        }
                    } 
                    catch (JsonException)
                    {
                        _indexerStatusService.RecordFailure(Definition.Id);
                        _logger.Warn("{0} {1}", this, httpException.Message);

                    }
                }
                else
                {
                    _indexerStatusService.RecordFailure(Definition.Id);
                    _logger.Warn("{0} {1}", this, httpException.Message);
                }
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
            catch (IndexerException ex)
            {
                _indexerStatusService.RecordFailure(Definition.Id);
                var message = String.Format("{0} - {1}", ex.Message, url);
                _logger.WarnException(message, ex);
            }
            catch (Exception feedEx)
            {
                _indexerStatusService.RecordFailure(Definition.Id);
                feedEx.Data.Add("FeedUrl", url);
                _logger.ErrorException("An error occurred while processing feed. " + url, feedEx);
            }

            return CleanupReleases(releases);

        }
    }
}
