using System;
using System.Linq;
using System.Xml.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.Newznab
{
    public class NewznabRssParser : RssParser
    {
        public const String ns = "{http://www.newznab.com/DTD/2010/feeds/attributes/}";

        protected override bool PreProcess(IndexerResponse indexerResponse)
        {
            var xdoc = LoadXmlDocument(indexerResponse);
            var error = xdoc.Descendants("error").FirstOrDefault();

            if (error == null) return true;

            var code = Convert.ToInt32(error.Attribute("code").Value);
            var errorMessage = error.Attribute("description").Value;

            if (code >= 100 && code <= 199) throw new ApiKeyException("Invalid API key");

            if (!indexerResponse.Request.Url.ToString().Contains("apikey=") && errorMessage == "Missing parameter")
            {
                throw new ApiKeyException("Indexer requires an API key");
            }

            if (errorMessage == "Request limit reached")
            {
                throw new RequestLimitReachedException("API limit reached");
            }

            throw new NewznabException("Newznab error detected: {0}", errorMessage);
        }

        protected override ReleaseInfo ProcessItem(XElement item, ReleaseInfo releaseInfo)
        {
            releaseInfo = base.ProcessItem(item, releaseInfo);

            releaseInfo.TvRageId = GetTvRageId(item);

            return releaseInfo;
        }

        protected override String GetInfoUrl(XElement item)
        {
            return ParseUrl(item.TryGetValue("comments").TrimEnd("#comments"));
        }

        protected override String GetCommentUrl(XElement item)
        {
            return ParseUrl(item.TryGetValue("comments"));
        }

        protected override Int64 GetSize(XElement item)
        {
            Int64 size;

            var sizeString = TryGetNewznabAttribute(item, "size");
            if (!sizeString.IsNullOrWhiteSpace() && Int64.TryParse(sizeString, out size))
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

        protected override string GetDownloadUrl(XElement item)
        {
            var url = base.GetDownloadUrl(item);

            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                url = ParseUrl((string)item.Element("enclosure").Attribute("url"));
            }

            return url;
        }

        protected virtual Int32 GetTvRageId(XElement item)
        {
            var tvRageIdString = TryGetNewznabAttribute(item, "rageid");
            Int32 tvRageId;

            if (!tvRageIdString.IsNullOrWhiteSpace() && Int32.TryParse(tvRageIdString, out tvRageId))
            {
                return tvRageId;
            }

            return 0;
        }

        protected String TryGetNewznabAttribute(XElement item, String key, String defaultValue = "")
        {
            var attr = item.Elements(ns + "attr").SingleOrDefault(e => e.Attribute("name").Value.Equals(key, StringComparison.CurrentCultureIgnoreCase));

            if (attr != null)
            {
                return attr.Attribute("value").Value;
            }

            return defaultValue;
        }
    }
}
