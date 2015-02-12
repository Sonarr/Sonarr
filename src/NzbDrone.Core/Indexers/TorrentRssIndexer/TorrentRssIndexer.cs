using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

using FluentValidation.Results;

using NLog;

using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.TorrentRssIndexer
{
    using System.Net;

    public class TorrentRssIndexer : HttpIndexerBase<TorrentRssIndexerSettings>
    {
        public override DownloadProtocol Protocol { get { return DownloadProtocol.Torrent; } }
        public override Boolean SupportsSearch { get { return false; } }
        public override Int32 PageSize { get { return 0; } }
        protected TorrentRssIndexerParserSettings ParserSettings { get; set; }

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
            if (ParserSettings == null)
            {
                throw new InvalidOperationException("You have to initialize ParserSettings first.");
            }

            if (ParserSettings.UseEZTVFormat)
            {
                return new EzrssTorrentRssParser();
            }
            else
            {
                return new TorrentRssParser() { UseGuidInfoUrl = false, ParseSeedersInDescription = ParserSettings.ParseSeedersInDescription, ParseSizeInDescription = ParserSettings.ParseSizeInDescription, SizeElementName = ParserSettings.SizeElementName };
            }
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            var requestGenerator = GetRequestGenerator();
            var request = requestGenerator.GetRecentRequests().First().First();
            ParserSettings = GetParserSettings(request);

            if (ParserSettings == null)
            {
                failures.Add(new ValidationFailure("", "Feed cannot be parsed"));
            }

            base.Test(failures);
        }

        private TorrentRssIndexerParserSettings GetParserSettings(IndexerRequest request)
        {
            IndexerResponse response;
            var settings = new TorrentRssIndexerParserSettings();

            try
            {
                response = FetchIndexerResponse(request);
            }
            catch (Exception ex)
            {
                _logger.WarnException("Unable to connect to indexer: " + ex.Message, ex);

                return null;
            }

            var isEZTVFeed = IsEZTVFeed(response);
            _logger.Debug("Feed is EZTV Compatible: {0}", isEZTVFeed);

            TorrentRssParser parser;
            if (isEZTVFeed)
            {
                // Test EZTV
                parser = new EzrssTorrentRssParser();
                var eztvTestResult = TestTorrentParser(response, parser);
                _logger.Debug("EZTV Parse result: {0}", eztvTestResult);

                if (eztvTestResult)
                {
                    _logger.Debug("Feed is a parseable EZTV Feed");
                    settings.UseEZTVFormat = true;
                    return settings;
                }
            }

            parser = new TorrentRssParser { ParseSeedersInDescription = true, ParseSizeInDescription = true };
            if (TestTorrentParser(response, parser))
            {
                _logger.Debug("Feed is a normal RSS Feed with Seeders and Size in Description");
                settings.UseEZTVFormat = false;
                settings.ParseSeedersInDescription = true;
                settings.ParseSizeInDescription = true;
                return settings;
            }

            parser = new TorrentRssParser { ParseSeedersInDescription = true, SizeElementName = "Size" };
            if (TestTorrentParser(response, parser))
            {
                _logger.Debug("Feed is an RSS Feed with Seeders in Description and Size field is \"Size\"");
                settings.UseEZTVFormat = false;
                settings.ParseSeedersInDescription = true;
                settings.SizeElementName = "Size";
                return settings;
            }

            return null;
        }

        protected override IList<ReleaseInfo> FetchReleases(IList<IEnumerable<IndexerRequest>> pageableRequests)
        {
            var releases = new List<ReleaseInfo>();
            var url = String.Empty;

            ParserSettings = GetParserSettings(pageableRequests.First().First());
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
        
        private Boolean IsEZTVFeed(IndexerResponse response)
        {
            using (var xmlTextReader = XmlReader.Create(new StringReader(response.Content), new XmlReaderSettings { DtdProcessing = DtdProcessing.Parse, ValidationType = ValidationType.None, IgnoreComments = true, XmlResolver = null }))
            {
                var document = XDocument.Load(xmlTextReader);

                // Check Namespace
                if (document.Root == null)
                {
                    throw new InvalidDataException("Could not parse IndexerResponse into XML.");
                }

                var ns = document.Root.GetNamespaceOfPrefix("torrent");
                if (ns == "http://xmlns.ezrss.it/0.1/")
                {
                    _logger.Trace("Identified feed as EZTV compatible by EZTV Namespace");
                    return true;
                }

                // Check DTD in DocType
                if (document.DocumentType != null && document.DocumentType.SystemId == "http://xmlns.ezrss.it/0.1/dtd/")
                {
                    _logger.Trace("Identified feed as EZTV compatible by EZTV DTD");
                    return true;
                }

                return false;
            }
        }

        private Boolean TestTorrentParser(IndexerResponse response, TorrentRssParser parser)
        {
            return ExecuteWithExceptionHandling(
                () =>
                {
                    var releases = parser.ParseResponse(response).ToList();

                    if (releases.Empty())
                    {
                        _logger.Trace("Empty releases");
                        return false;
                    }

                    var firstRelease = releases.First();
                    var torrentInfo = firstRelease as TorrentInfo;
                    if (torrentInfo == null)
                    {
                        _logger.Trace("Not TorrentInfo");
                        return false;
                    }

                    if (torrentInfo.Size == 0 || string.IsNullOrEmpty(torrentInfo.Title) || string.IsNullOrEmpty(torrentInfo.DownloadUrl))
                    {
                        _logger.Trace("Failed Parsing. Content: \n{0}", torrentInfo.ToString("L"));
                        _logger.Trace(response.Content);
                        return false;
                    }

                    return true;
                });
        }

        private Boolean ExecuteWithExceptionHandling(Func<Boolean> functionToExecute)
        {
            try
            {
                return functionToExecute();
            }
            catch (ApiKeyException)
            {
                _logger.Warn("Indexer returned result for RSS URL, API Key appears to be invalid");

                return false;
            }
            catch (RequestLimitReachedException)
            {
                _logger.Warn("Request limit reached");
            }
            catch (Exception ex)
            {
                _logger.WarnException("Unable to connect to indexer: " + ex.Message, ex);

                return false;
            }

            return false;
        }
    }
}
