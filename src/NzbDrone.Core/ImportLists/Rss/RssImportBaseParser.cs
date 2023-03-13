using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.ImportLists.Exceptions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.ImportLists.Rss
{
    public class RssImportBaseParser : IParseImportListResponse
    {
        private readonly Logger _logger;

        public RssImportBaseParser(Logger logger)
        {
            _logger = logger;
        }

        public virtual IList<ImportListItemInfo> ParseResponse(ImportListResponse importResponse)
        {
            var series = new List<ImportListItemInfo>();

            if (!PreProcess(importResponse))
            {
                return series;
            }

            var document = LoadXmlDocument(importResponse);
            var items = GetItems(document).ToList();

            foreach (var item in items)
            {
                try
                {
                    var itemInfo = ProcessItem(item);

                    series.AddIfNotNull(itemInfo);
                }
                catch (UnsupportedFeedException itemEx)
                {
                    itemEx.WithData("FeedUrl", importResponse.Request.Url);
                    itemEx.WithData("ItemTitle", item.Title());
                    throw;
                }
                catch (Exception itemEx)
                {
                    itemEx.WithData("FeedUrl", importResponse.Request.Url);
                    itemEx.WithData("ItemTitle", item.Title());
                    _logger.Error(itemEx, "An error occurred while processing feed item from {0}", importResponse.Request.Url);
                }
            }

            return series;
        }

        protected virtual bool PreProcess(ImportListResponse importListResponse)
        {
            if (importListResponse.HttpResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new ImportListException(importListResponse, "Request resulted in an unexpected StatusCode [{0}]", importListResponse.HttpResponse.StatusCode);
            }

            if (importListResponse.HttpResponse.Headers.ContentType != null && importListResponse.HttpResponse.Headers.ContentType.Contains("text/xml") &&
                importListResponse.HttpRequest.Headers.Accept != null && !importListResponse.HttpRequest.Headers.Accept.Contains("text/xml"))
            {
                throw new ImportListException(importListResponse, "Request responded with html content. Site is likely blocked or unavailable.");
            }

            return true;
        }

        protected virtual XDocument LoadXmlDocument(ImportListResponse importListResponse)
        {
            try
            {
                var content = XmlCleaner.ReplaceEntities(importListResponse.Content);
                content = XmlCleaner.ReplaceUnicode(content);

                using (var xmlTextReader = XmlReader.Create(new StringReader(content), new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore, IgnoreComments = true }))
                {
                    return XDocument.Load(xmlTextReader);
                }
            }
            catch (XmlException ex)
            {
                var contentSample = importListResponse.Content.Substring(0, Math.Min(importListResponse.Content.Length, 512));
                _logger.Debug("Truncated response content (originally {0} characters): {1}", importListResponse.Content.Length, contentSample);

                ex.WithData(importListResponse.HttpResponse);

                throw;
            }
        }

        protected IEnumerable<XElement> GetItems(XDocument document)
        {
            var root = document.Root;

            if (root == null)
            {
                return Enumerable.Empty<XElement>();
            }

            var channel = root.Element("channel");

            if (channel == null)
            {
                return Enumerable.Empty<XElement>();
            }

            return channel.Elements("item");
        }

        protected virtual ImportListItemInfo ProcessItem(XElement item)
        {
            var info = new ImportListItemInfo();
            var guid = item.TryGetValue("guid");

            if (guid != null)
            {
                if (int.TryParse(guid, out var tvdbId))
                {
                    info.TvdbId = tvdbId;
                }
            }

            info.Title = item.TryGetValue("title", "Unknown");

            if (info.TvdbId == 0)
            {
                throw new UnsupportedFeedException("Each item in the RSS feed must have a guid element with a TVDB ID");
            }

            return info;
        }
    }
}
