using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using MonoTorrent;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.Newznab
{
    public class NewznabRssParser : RssParser
    {
        public const string ns = "{http://www.newznab.com/DTD/2010/feeds/attributes/}";

        public NewznabRssParser()
        {
            PreferredEnclosureMimeTypes = UsenetEnclosureMimeTypes;
            UseEnclosureUrl = true;
        }

        public static void CheckError(XDocument xdoc, IndexerResponse indexerResponse)
        {
            var error = xdoc.Descendants("error").FirstOrDefault();

            if (error == null)
            {
                return;
            }

            var code = Convert.ToInt32(error.Attribute("code").Value);
            var errorMessage = error.Attribute("description").Value;

            if (code >= 100 && code <= 199)
            {
                throw new ApiKeyException(errorMessage);
            }

            if (!indexerResponse.Request.Url.FullUri.Contains("apikey=") && (errorMessage == "Missing parameter" || errorMessage.Contains("apikey")))
            {
                throw new ApiKeyException("Indexer requires an API key");
            }

            if (errorMessage == "Request limit reached")
            {
                throw new RequestLimitReachedException("API limit reached");
            }

            throw new NewznabException(indexerResponse, errorMessage);
        }

        protected override bool PreProcess(IndexerResponse indexerResponse)
        {
            var xdoc = LoadXmlDocument(indexerResponse);

            CheckError(xdoc, indexerResponse);

            return true;
        }

        protected override bool PostProcess(IndexerResponse indexerResponse, List<XElement> items, List<ReleaseInfo> releases)
        {
            var enclosureTypes = items.SelectMany(GetEnclosures).Select(v => v.Type).Distinct().ToArray();
            if (enclosureTypes.Any() && enclosureTypes.Intersect(PreferredEnclosureMimeTypes).Empty())
            {
                if (enclosureTypes.Intersect(TorrentEnclosureMimeTypes).Any())
                {
                    _logger.Warn("{0} does not contain {1}, found {2}, did you intend to add a Torznab indexer?", indexerResponse.Request.Url, NzbEnclosureMimeType, enclosureTypes[0]);
                }
                else
                {
                    _logger.Warn("{1} does not contain {1}, found {2}.", indexerResponse.Request.Url, NzbEnclosureMimeType, enclosureTypes[0]);
                }
            }

            return true;
        }

        protected override ReleaseInfo ProcessItem(XElement item, ReleaseInfo releaseInfo)
        {
            releaseInfo = base.ProcessItem(item, releaseInfo);

            releaseInfo.TvdbId = GetTvdbId(item);
            releaseInfo.TvRageId = GetTvRageId(item);

            return releaseInfo;
        }

        protected override string GetInfoUrl(XElement item)
        {
            return ParseUrl(item.TryGetValue("comments").TrimEnd("#comments"));
        }

        protected override string GetCommentUrl(XElement item)
        {
            return ParseUrl(item.TryGetValue("comments"));
        }

        protected override long GetSize(XElement item)
        {
            long size;

            var sizeString = TryGetNewznabAttribute(item, "size");
            if (!sizeString.IsNullOrWhiteSpace() && long.TryParse(sizeString, out size))
            {
                return size;
            }

            size = GetEnclosureLength(item);

            return size;
        }

        protected override DateTime GetPublishDate(XElement item)
        {
            var dateString = TryGetNewznabAttribute(item, "usenetdate");
            if (!dateString.IsNullOrWhiteSpace())
            {
                return XElementExtensions.ParseDate(dateString);
            }

            return base.GetPublishDate(item);
        }

        protected virtual int GetTvdbId(XElement item)
        {
            var tvdbIdString = TryGetNewznabAttribute(item, "tvdbid");
            int tvdbId;

            if (!tvdbIdString.IsNullOrWhiteSpace() && int.TryParse(tvdbIdString, out tvdbId))
            {
                return tvdbId;
            }

            return 0;
        }

        protected virtual int GetTvRageId(XElement item)
        {
            var tvRageIdString = TryGetNewznabAttribute(item, "rageid");
            int tvRageId;

            if (!tvRageIdString.IsNullOrWhiteSpace() && int.TryParse(tvRageIdString, out tvRageId))
            {
                return tvRageId;
            }

            return 0;
        }

        protected string TryGetNewznabAttribute(XElement item, string key, string defaultValue = "")
        {
            var attrElement = item.Elements(ns + "attr").FirstOrDefault(e => e.Attribute("name").Value.Equals(key, StringComparison.OrdinalIgnoreCase));
            if (attrElement != null)
            {
                var attrValue = attrElement.Attribute("value");
                if (attrValue != null)
                {
                    return attrValue.Value;
                }
            }

            return defaultValue;
        }
    }
}
