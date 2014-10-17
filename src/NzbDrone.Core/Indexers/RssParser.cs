using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers
{
    public class RssParser : IParseIndexerResponse
    {
        protected readonly Logger _logger;

        // Use the 'guid' element content as InfoUrl.
        public Boolean UseGuidInfoUrl { get; set; }

        // Use the enclosure as download url and/or length.
        public Boolean UseEnclosureUrl { get; set; }
        public Boolean UseEnclosureLength { get; set; }

        // Parse "Size: 1.3 GB" or "1.3 GB" parts in the description element and use that as Size.
        public Boolean ParseSizeInDescription { get; set; }

        public RssParser()
        {
            _logger = NzbDroneLogger.GetLogger(this);
        }

        public virtual IList<ReleaseInfo> ParseResponse(IndexerResponse indexerResponse)
        {
            var releases = new List<ReleaseInfo>();

            if (!PreProcess(indexerResponse))
            {
                return releases;
            }

            using (var xmlTextReader = XmlReader.Create(new StringReader(indexerResponse.Content), new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore, IgnoreComments = true }))
            {
                var document = XDocument.Load(xmlTextReader);
                var items = GetItems(document);

                foreach (var item in items)
                {
                    try
                    {
                        var reportInfo = ProcessItem(item);

                        releases.AddIfNotNull(reportInfo);
                    }
                    catch (Exception itemEx)
                    {
                        itemEx.Data.Add("Item", item.Title());
                        _logger.ErrorException("An error occurred while processing feed item from " + indexerResponse.Request.Url, itemEx);
                    }
                }
            }

            return releases;
        }

        protected virtual ReleaseInfo CreateNewReleaseInfo()
        {
            return new ReleaseInfo();
        }

        protected virtual Boolean PreProcess(IndexerResponse indexerResponse)
        {
            if (indexerResponse.HttpResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new IndexerException(indexerResponse, "Indexer API call resulted in an unexpected StatusCode [{0}]", indexerResponse.HttpResponse.StatusCode);
            }

            if (indexerResponse.HttpResponse.Headers.ContentType != null && indexerResponse.HttpResponse.Headers.ContentType.Contains("text/html") &&
                indexerResponse.HttpRequest.Headers.Accept != null && !indexerResponse.HttpRequest.Headers.Accept.Contains("text/html"))
            {
                throw new IndexerException(indexerResponse, "Indexer responded with html content. Site is likely blocked or unavailable.");
            }

            return true;
        }

        protected ReleaseInfo ProcessItem(XElement item)
        {
            var releaseInfo = CreateNewReleaseInfo();

            releaseInfo = ProcessItem(item, releaseInfo);

            _logger.Trace("Parsed: {0}", releaseInfo.Title);

            return PostProcess(item, releaseInfo);
        }

        protected virtual ReleaseInfo ProcessItem(XElement item, ReleaseInfo releaseInfo)
        {
            releaseInfo.Guid = GetGuid(item);
            releaseInfo.Title = GetTitle(item);
            releaseInfo.PublishDate = GetPublishDate(item);
            releaseInfo.DownloadUrl = GetDownloadUrl(item);
            releaseInfo.InfoUrl = GetInfoUrl(item);
            releaseInfo.CommentUrl = GetCommentUrl(item);

            try
            {
                releaseInfo.Size = GetSize(item);
            }
            catch (Exception)
            {
                throw new SizeParsingException("Unable to parse size from: {0}", releaseInfo.Title);
            }

            return releaseInfo;
        }

        protected virtual ReleaseInfo PostProcess(XElement item, ReleaseInfo releaseInfo)
        {
            return releaseInfo;
        }

        protected virtual String GetGuid(XElement item)
        {
            return item.TryGetValue("guid", Guid.NewGuid().ToString());
        }

        protected virtual String GetTitle(XElement item)
        {
            return item.TryGetValue("title", "Unknown");
        }

        protected virtual DateTime GetPublishDate(XElement item)
        {
            var dateString = item.TryGetValue("pubDate");

            return XElementExtensions.ParseDate(dateString);
        }

        protected virtual string GetDownloadUrl(XElement item)
        {
            if (UseEnclosureUrl)
            {
                return item.Element("enclosure").Attribute("url").Value;
            }
            else
            {
                return item.Element("link").Value;
            }
        }

        protected virtual string GetInfoUrl(XElement item)
        {
            if (UseGuidInfoUrl)
            {
                return (String)item.Element("guid");
            }

            return String.Empty;
        }

        protected virtual string GetCommentUrl(XElement item)
        {
            return (String)item.Element("comments");
        }

        protected virtual long GetSize(XElement item)
        {
            if (UseEnclosureLength)
            {
                return GetEnclosureLength(item);
            }
            else if (ParseSizeInDescription)
            {
                return ParseSize(item.Element("description").Value, true);
            }

            return 0;
        }

        protected virtual long GetEnclosureLength(XElement item)
        {
            var enclosure = item.Element("enclosure");

            if (enclosure != null)
            {
                return (long)enclosure.Attribute("length");
            }

            return 0;
        }

        private static readonly Regex ParseSizeRegex = new Regex(@"(?<value>\d+\.\d{1,2}|\d+\,\d+\.\d{1,2}|\d+)\W?(?<unit>[KMG]i?B)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static Int64 ParseSize(String sizeString, Boolean defaultToBinaryPrefix)
        {
            var match = ParseSizeRegex.Matches(sizeString);

            if (match.Count != 0)
            {
                var value = Decimal.Parse(Regex.Replace(match[0].Groups["value"].Value, "\\,", ""), CultureInfo.InvariantCulture);

                var unit = match[0].Groups["unit"].Value.ToLower();

                switch (unit)
                {
                    case "kb":
                        return ConvertToBytes(Convert.ToDouble(value), 1, defaultToBinaryPrefix);
                    case "mb":
                        return ConvertToBytes(Convert.ToDouble(value), 2, defaultToBinaryPrefix);
                    case "gb":
                        return ConvertToBytes(Convert.ToDouble(value), 3, defaultToBinaryPrefix);
                    case "kib":
                        return ConvertToBytes(Convert.ToDouble(value), 1, true);
                    case "mib":
                        return ConvertToBytes(Convert.ToDouble(value), 2, true);
                    case "gib":
                        return ConvertToBytes(Convert.ToDouble(value), 3, true);
                    default:
                        return (Int64)value;
                }
            }
            return 0;
        }

        private static Int64 ConvertToBytes(Double value, Int32 power, Boolean binaryPrefix)
        {
            var prefix = binaryPrefix ? 1024 : 1000;
            var multiplier = Math.Pow(prefix, power);
            var result = value * multiplier;

            return Convert.ToInt64(result);
        }

        private IEnumerable<XElement> GetItems(XDocument document)
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
    }
}
