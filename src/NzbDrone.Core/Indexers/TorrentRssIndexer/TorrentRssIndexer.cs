using System;
using System.Collections.Generic;
using System.Net;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.TorrentRssIndexer
{
    public class TorrentRssIndexer : HttpIndexerBase<TorrentRssIndexerSettings>
    {
        public override DownloadProtocol Protocol { get { return DownloadProtocol.Torrent; } }
        public override Boolean SupportsSearch { get { return false; } }
        public override Int32 PageSize { get { return 0; } }

        public TorrentRssIndexer(IHttpClient httpClient, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, configService, parsingService, logger)
        {

        }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new TorrentRssIndexerRequestGenerator() { Settings = Settings };
        }

        public override IParseIndexerResponse GetParser()
        {
            var factory = new TorrentRssParserFactory();
            return factory.GetParser(Settings, FetchIndexerResponse);
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            try
            {
                var parser = this.GetParser();
                if (parser == null)
                {
                    throw new Exception("Could not parse feed. See log for details.");
                }
            }
            catch (Exception)
            {
                failures.Add(new ValidationFailure("", "Feed cannot be parsed or is invalid. See log for details."));
            }
            
            base.Test(failures);
        }

        protected override IList<ReleaseInfo> FetchReleases(IList<IEnumerable<IndexerRequest>> pageableRequests)
        {
            var releases = new List<ReleaseInfo>();
            var url = String.Empty;

            var parser = GetParser();

            try
            {
                foreach (var pageableRequest in pageableRequests)
                {
                    var pagedReleases = new List<ReleaseInfo>();

                    foreach (var request in pageableRequest)
                    {
                        url = request.Url.ToString();

                        var page = FetchPage(request, parser);

                        pagedReleases.AddRange(page);

                        if (!IsFullPage(page) || pagedReleases.Count >= MaxNumResultsPerQuery)
                        {
                            break;
                        }
                    }

                    releases.AddRange(pagedReleases);
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
            catch (HttpException httpException)
            {
                if ((int)httpException.Response.StatusCode == 429)
                {
                    _logger.Warn("API Request Limit reached for {0}", this);
                }

                _logger.Warn("{0} {1}", this, httpException.Message);
            }
            catch (RequestLimitReachedException)
            {
                // TODO: Backoff for x period.
                _logger.Warn("API Request Limit reached for {0}", this);
            }
            catch (ApiKeyException)
            {
                _logger.Warn("Invalid API Key for {0} {1}", this, url);
            }
            catch (IndexerException ex)
            {
                var message = String.Format("{0} - {1}", ex.Message, url);
                _logger.WarnException(message, ex);
            }
            catch (Exception feedEx)
            {
                feedEx.Data.Add("FeedUrl", url);
                _logger.ErrorException("An error occurred while processing feed. " + url, feedEx);
            }

            return CleanupReleases(releases);
        }
    }
}
