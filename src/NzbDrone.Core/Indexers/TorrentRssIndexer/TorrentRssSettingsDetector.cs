using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.TorrentRssIndexer
{
    public class TorrentRssSettingsDetector : ITorrentRssSettingsDetector
    {
        protected readonly Logger _logger;

        protected TorrentRssIndexerSettings _settings;

        private readonly IHttpClient _httpClient;

        private const int ValidEntryPercentage = 100;

        private const int ValidSizeThresholdMegabytes = 2;

        public TorrentRssSettingsDetector(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// Detect settings for Parser, based on URL
        /// </summary>
        /// <param name="settings">Indexer Settings to use for Parser</param>
        /// <returns>Parsed Settings or <c>null</c></returns>
        public TorrentRssIndexerParserSettings Detect(TorrentRssIndexerSettings settings)
        {
            _settings = settings;
            
            var requestGenerator = new TorrentRssIndexerRequestGenerator { Settings = _settings };
            var request = requestGenerator.GetRecentRequests().First().First();
            if (request == null)
            {
                throw new NullReferenceException("request cannot be null.");
            }

            HttpResponse httpResponse = null;
            try
            {
                httpResponse = _httpClient.Execute(request.HttpRequest);
            }
            catch (Exception ex)
            {
                _logger.WarnException(string.Format("Unable to connect to indexer {0}: {1}", request.Url, ex.Message), ex);
                return null;
            }

            var indexerResponse = new IndexerResponse(request, httpResponse);
            return GetParserSettings(indexerResponse);
        }

        private TorrentRssIndexerParserSettings GetParserSettings(IndexerResponse response)
        {
            var settings = new TorrentRssIndexerParserSettings();

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

            parser = new TorrentRssParser
            {
                ParseSeedersInDescription = true,
                ParseSizeInDescription = true
            };

            if (TestTorrentParser(response, parser))
            {
                _logger.Debug("Feed is a normal RSS Feed with Seeders and Size in Description");
                settings.UseEZTVFormat = false;
                settings.ParseSeedersInDescription = true;
                settings.ParseSizeInDescription = true;
                return settings;
            }

            parser = new TorrentRssParser
            {
                ParseSeedersInDescription = true,
                SizeElementName = "Size"
            };

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

                    if (!ValidateTorrents(releases, r => Parser.Parser.ParseTitle(r.Title) != null, ValidEntryPercentage))
                    {
                        _logger.Trace("Percentage of Titles that could parsed is lower than threshold of {0}", ValidEntryPercentage);
                        return false;
                    }

                    if (!ValidateTorrents(releases, r => r.Size > ValidSizeThresholdMegabytes * 1024 * 1024, ValidEntryPercentage))
                    {
                        _logger.Trace("Percentage of entries that have a size bigger than ValidSizeThresholdMegabytes ({0} MB) is  lower than threshold of {1}", ValidSizeThresholdMegabytes, ValidEntryPercentage);
                        return false;
                    }

                    if (!ValidateTorrents(releases, r => Uri.IsWellFormedUriString(r.DownloadUrl, UriKind.Absolute), ValidEntryPercentage))
                    {
                        _logger.Trace("Percentage of entries that have a valid download url is smaller than threshold of {0}", ValidEntryPercentage);
                        return false;
                    }

                    return true;
                });
        }

        private Boolean ValidateTorrents(IEnumerable<ReleaseInfo> releases, Func<ReleaseInfo, Boolean> entryValidationFunction, int threshold)
        {
            var validEntries = releases.Count(entryValidationFunction);
            var validEntriesPercentage = validEntries * 1.0 / releases.Count() * 100.0;
            return validEntriesPercentage >= threshold;
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
