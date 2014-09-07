using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using NLog;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.Indexers.Newznab;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers
{
    public abstract class RssParserBase : IParseFeed
    {
        protected readonly Logger _logger;

        protected virtual ReleaseInfo CreateNewReleaseInfo()
        {
            return new ReleaseInfo();
        }

        protected RssParserBase()
        {
            _logger = NzbDroneLogger.GetLogger(this);
        }

        public virtual IEnumerable<ReleaseInfo> Process(string xml, string url)
        {
            PreProcess(xml, url);

            using (var xmlTextReader = XmlReader.Create(new StringReader(xml), new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore, IgnoreComments = true }))
            {

                var document = XDocument.Load(xmlTextReader);
                var items = document.Descendants("item");

                var result = new List<ReleaseInfo>();

                foreach (var item in items)
                {
                    try
                    {
                        var reportInfo = ParseFeedItem(item.StripNameSpace(), url);

                        if (reportInfo != null)
                        {
                            result.Add(reportInfo);
                        }
                    }
                    catch (Exception itemEx)
                    {
                        itemEx.Data.Add("Item", item.Title());
                        _logger.ErrorException("An error occurred while processing feed item from " + url, itemEx);
                    }
                }

                return result;
            }
        }

        private ReleaseInfo ParseFeedItem(XElement item, string url)
        {
            var reportInfo = CreateNewReleaseInfo();

            reportInfo.Guid = GetGuid(item);
            reportInfo.Title = GetTitle(item);
            reportInfo.PublishDate = GetPublishDate(item);
            reportInfo.DownloadUrl = GetNzbUrl(item);
            reportInfo.InfoUrl = GetNzbInfoUrl(item);

            try
            {
                reportInfo.Size = GetSize(item);
            }
            catch (Exception)
            {
                throw new SizeParsingException("Unable to parse size from: {0} [{1}]", reportInfo.Title, url);
            }

            _logger.Trace("Parsed: {0}", reportInfo.Title);

            return PostProcessor(item, reportInfo);
        }

        protected virtual String GetGuid(XElement item)
        {
            return item.TryGetValue("guid", Guid.NewGuid().ToString());
        }

        protected virtual string GetTitle(XElement item)
        {
            return item.Title();
        }

        protected virtual DateTime GetPublishDate(XElement item)
        {
            return item.PublishDate();
        }

        protected virtual string GetNzbUrl(XElement item)
        {
            return item.Links().First();
        }

        protected virtual string GetNzbInfoUrl(XElement item)
        {
            return String.Empty;
        }

        protected abstract long GetSize(XElement item);

        protected virtual void PreProcess(string source, string url)
        {
        }

        protected virtual ReleaseInfo PostProcessor(XElement item, ReleaseInfo currentResult)
        {
            return currentResult;
        }

        private static readonly Regex ReportSizeRegex = new Regex(@"(?<value>\d+\.\d{1,2}|\d+\,\d+\.\d{1,2}|\d+)\W?(?<unit>GB|MB|GiB|MiB)",
                                                                  RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static Int64 ParseSize(String sizeString, Boolean defaultToBinaryPrefix)
        {
            var match = ReportSizeRegex.Matches(sizeString);

            if (match.Count != 0)
            {
                var cultureInfo = new CultureInfo("en-US");
                var value = Decimal.Parse(Regex.Replace(match[0].Groups["value"].Value, "\\,", ""), cultureInfo);

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
    }
}
