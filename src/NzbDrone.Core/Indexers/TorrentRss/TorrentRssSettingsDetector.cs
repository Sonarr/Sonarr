using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.TorrentRss
{
    public interface ITorrentRssSettingsDetector
    {
        TorrentRssIndexerParserSettings Detect(TorrentRssIndexerSettings settings);
    }

    public class TorrentRssSettingsDetector : ITorrentRssSettingsDetector
    {
        private const long ValidSizeThreshold = 2 * 1024 * 1024;

        protected readonly Logger _logger;

        private readonly IHttpClient _httpClient;

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
        public TorrentRssIndexerParserSettings Detect(TorrentRssIndexerSettings indexerSettings)
        {
            _logger.Debug("Evaluating TorrentRss feed '{0}'", indexerSettings.BaseUrl);

            try
            {
                var requestGenerator = new TorrentRssIndexerRequestGenerator { Settings = indexerSettings };
                var request = requestGenerator.GetRecentRequests().GetAllTiers().First().First();

                HttpResponse httpResponse = null;
                try
                {
                    httpResponse = _httpClient.Execute(request.HttpRequest);
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, string.Format("Unable to connect to indexer {0}: {1}", request.Url, ex.Message));
                    return null;
                }

                var indexerResponse = new IndexerResponse(request, httpResponse);
                return GetParserSettings(indexerResponse, indexerSettings);
            }
            catch (Exception ex)
            {
                ex.WithData("FeedUrl", indexerSettings.BaseUrl);
                throw;
            }
    }

        private TorrentRssIndexerParserSettings GetParserSettings(IndexerResponse response, TorrentRssIndexerSettings indexerSettings)
        {
            var settings = GetEzrssParserSettings(response, indexerSettings);
            if (settings != null)
            {
                return settings;
            }

            settings = GetGenericTorrentRssParserSettings(response, indexerSettings);
            if (settings != null)
            {
                return settings;
            }

            return null;
        }

        private TorrentRssIndexerParserSettings GetEzrssParserSettings(IndexerResponse response, TorrentRssIndexerSettings indexerSettings)
        {
            if (!IsEZTVFeed(response))
            {
                return null;
            }

            _logger.Trace("Feed has Ezrss schema");

            var parser = new EzrssTorrentRssParser();
            var releases = ParseResponse(parser, response);

            try
            {
                ValidateReleases(releases, indexerSettings);
                ValidateReleaseSize(releases, indexerSettings);

                _logger.Debug("Feed was parseable by Ezrss Parser");
                return new TorrentRssIndexerParserSettings
                {
                    UseEZTVFormat = true
                };
            }
            catch (Exception ex)
            {
                _logger.Trace(ex, "Feed wasn't parsable by Ezrss Parser");
                return null;
            }
        }

        private TorrentRssIndexerParserSettings GetGenericTorrentRssParserSettings(IndexerResponse response, TorrentRssIndexerSettings indexerSettings)
        {
            var parser = new TorrentRssParser
            {
                UseEnclosureUrl = true,
                UseEnclosureLength = true,
                ParseSeedersInDescription = true
            };

            var item = parser.GetItems(response).FirstOrDefault();
            if (item == null)
            {
                throw new UnsupportedFeedException("Empty feed, cannot check if feed is parsable.");
            }

            var settings = new TorrentRssIndexerParserSettings()
            {
                UseEnclosureUrl = true,
                UseEnclosureLength = true,
                ParseSeedersInDescription = true
            };

            if (item.Element("enclosure") == null)
            {
                parser.UseEnclosureUrl = settings.UseEnclosureUrl = false;
            }

            var releases = ParseResponse(parser, response);
            ValidateReleases(releases, indexerSettings);

            if (!releases.Any(v => v.Seeders.HasValue))
            {
                _logger.Trace("Feed doesn't have Seeders in Description, disabling option.");
                parser.ParseSeedersInDescription = settings.ParseSeedersInDescription = false;
            }

            if (!releases.Any(r => r.Size < ValidSizeThreshold))
            {
                _logger.Trace("Feed has valid size in enclosure.");
                return settings;
            }

            parser.UseEnclosureLength = settings.UseEnclosureLength = false;

            foreach (var sizeElementName in new[] { "size", "Size" })
            {
                parser.SizeElementName = settings.SizeElementName = sizeElementName;

                releases = ParseResponse(parser, response);
                ValidateReleases(releases, indexerSettings);

                if (!releases.Any(r => r.Size < ValidSizeThreshold))
                {
                    _logger.Trace("Feed has valid size in Size element.");
                    return settings;
                }
            }

            parser.SizeElementName = settings.SizeElementName = null;
            parser.ParseSizeInDescription = settings.ParseSizeInDescription = true;

            releases = ParseResponse(parser, response);
            ValidateReleases(releases, indexerSettings);

            if (releases.Count(r => r.Size >= ValidSizeThreshold) > releases.Count() / 2)
            {
                if (releases.Any(r => r.Size < ValidSizeThreshold))
                {
                    _logger.Debug("Feed {0} contains very small releases.", response.Request.Url);
                }

                _logger.Trace("Feed has valid size in description.");
                return settings;
            }

            parser.ParseSizeInDescription = settings.ParseSizeInDescription = false;

            _logger.Debug("Feed doesn't have release size.");

            releases = ParseResponse(parser, response);
            ValidateReleases(releases, indexerSettings);
            ValidateReleaseSize(releases, indexerSettings);

            return settings;
        }

        private bool IsEZTVFeed(IndexerResponse response)
        {
            var content = XmlCleaner.ReplaceEntities(response.Content);
            content = XmlCleaner.ReplaceUnicode(content);

            using (var xmlTextReader = XmlReader.Create(new StringReader(content), new XmlReaderSettings { DtdProcessing = DtdProcessing.Parse, ValidationType = ValidationType.None, IgnoreComments = true, XmlResolver = null }))
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

                // Check namespaces
                if (document.Descendants().Any(v => v.GetDefaultNamespace().NamespaceName == "http://xmlns.ezrss.it/0.1/"))
                {
                    _logger.Trace("Identified feed as EZTV compatible by EZTV Namespace");
                    return true;
                }

                return false;
            }
        }

        private TorrentInfo[] ParseResponse(IParseIndexerResponse parser, IndexerResponse response)
        {
            try
            {
                var releases = parser.ParseResponse(response).Cast<TorrentInfo>().ToArray();
                return releases;
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "Unable to parse indexer feed: " + ex.Message);
                throw new UnsupportedFeedException("Unable to parse indexer: " + ex.Message);
            }
        }

        private void ValidateReleases(TorrentInfo[] releases, TorrentRssIndexerSettings indexerSettings)
        {
            if (releases == null || releases.Empty())
            {
                throw new UnsupportedFeedException("Empty feed, cannot check if feed is parsable.");
            }

            var torrentInfo = releases.First();

            _logger.Trace("TorrentInfo: \n{0}", torrentInfo.ToString("L"));

            if (releases.Any(r => r.Title.IsNullOrWhiteSpace()))
            {
                throw new UnsupportedFeedException("Feed contains releases without title.");
            }

            if (releases.Any(r => !IsValidDownloadUrl(r.DownloadUrl)))
            {
                throw new UnsupportedFeedException("Failed to find a valid download url in the feed.");
            }

            var total = releases.Where(v => v.Guid != null).Select(v => v.Guid).ToArray();
            var distinct = total.Distinct().ToArray();

            if (distinct.Length != total.Length)
            {
                throw new UnsupportedFeedException("Feed contains releases with same guid, rejecting malformed rss feed.");
            }
        }

        private void ValidateReleaseSize(TorrentInfo[] releases, TorrentRssIndexerSettings indexerSettings)
        {
            if (!indexerSettings.AllowZeroSize && releases.Any(r => r.Size == 0))
            {
                throw new UnsupportedFeedException("Feed doesn't contain the release content size.");
            }

            if (releases.Any(r => r.Size != 0 && r.Size < ValidSizeThreshold))
            {
                throw new UnsupportedFeedException("Size of one more releases lower than {0}, feed must contain release content size.", ValidSizeThreshold.SizeSuffix());
            }
        }

        private static bool IsValidDownloadUrl(string url)
        {
            if (url.IsNullOrWhiteSpace())
            {
                return false;
            }

            if (url.StartsWith("magnet:") ||
                url.StartsWith("http:") ||
                url.StartsWith("https:"))
            {
                return true;
            }

            return false;
        }
    }
}
