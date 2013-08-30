using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using NLog;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers
{
    public interface IParseFeed
    {
        IEnumerable<ReportInfo> Process(string xml, string url);
    }

    public class BasicRssParser : IParseFeed
    {
        private readonly Logger _logger;

        public BasicRssParser()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        public IEnumerable<ReportInfo> Process(string xml, string url)
        {
            using (var xmlTextReader = new XmlTextReader(new StringReader(xml)) { DtdProcessing = DtdProcessing.Ignore })
            {
                var document = XDocument.Load(xmlTextReader);
                var items = document.Descendants("item");

                var result = new List<ReportInfo>();

                foreach (var item in items)
                {
                    try
                    {
                        var reportInfo = ParseFeedItem(item);
                        if (reportInfo != null)
                        {
                            reportInfo.NzbUrl = GetNzbUrl(item);
                            reportInfo.NzbInfoUrl = GetNzbInfoUrl(item);

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


        protected virtual string GetTitle(XElement item)
        {
            return item.Title();
        }

        protected virtual string GetNzbUrl(XElement item)
        {
            return item.Links().First();
        }

        protected virtual string GetNzbInfoUrl(XElement item)
        {
            return String.Empty;
        }

        protected virtual ReportInfo PostProcessor(XElement item, ReportInfo currentResult)
        {
            return currentResult;
        }

        private ReportInfo ParseFeedItem(XElement item)
        {
            var title = GetTitle(item);

            var reportInfo = new ReportInfo();

            reportInfo.Title = title;
            reportInfo.Age = DateTime.Now.Date.Subtract(item.PublishDate().Date).Days;
            reportInfo.ReleaseGroup = ParseReleaseGroup(title);

            _logger.Trace("Parsed: {0} from: {1}", reportInfo, item.Title());

            return PostProcessor(item, reportInfo);
        }

        public static string ParseReleaseGroup(string title)
        {
            title = title.Trim();
            var index = title.LastIndexOf('-');

            if (index < 0)
                index = title.LastIndexOf(' ');

            if (index < 0)
                return String.Empty;

            var group = title.Substring(index + 1);

            if (@group.Length == title.Length)
                return String.Empty;

            return @group;
        }

        private static readonly Regex[] HeaderRegex = new[]
                                                          {
                                                                new Regex(@"(?:\[.+\]\-\[.+\]\-\[.+\]\-\[)(?<nzbTitle>.+)(?:\]\-.+)",
                                                                        RegexOptions.IgnoreCase),
                                                                
                                                                new Regex(@"(?:\[.+\]\W+\[.+\]\W+\[.+\]\W+\"")(?<nzbTitle>.+)(?:\"".+)",
                                                                        RegexOptions.IgnoreCase),
                                                                    
                                                                new Regex(@"(?:\[)(?<nzbTitle>.+)(?:\]\-.+)",
                                                                        RegexOptions.IgnoreCase),
                                                          };

        public static string ParseHeader(string header)
        {
            foreach (var regex in HeaderRegex)
            {
                var match = regex.Matches(header);

                if (match.Count != 0)
                    return match[0].Groups["nzbTitle"].Value.Trim();
            }

            return header;
        }

        private static readonly Regex ReportSizeRegex = new Regex(@"(?<value>\d+\.\d{1,2}|\d+\,\d+\.\d{1,2}|\d+)\W?(?<unit>GB|MB|GiB|MiB)",
                                                                  RegexOptions.IgnoreCase | RegexOptions.Compiled);


        public static long GetReportSize(string sizeString)
        {
            var match = ReportSizeRegex.Matches(sizeString);

            if (match.Count != 0)
            {
                var cultureInfo = new CultureInfo("en-US");
                var value = Decimal.Parse(Regex.Replace(match[0].Groups["value"].Value, "\\,", ""), cultureInfo);

                var unit = match[0].Groups["unit"].Value;

                if (unit.Equals("MB", StringComparison.InvariantCultureIgnoreCase) ||
                    unit.Equals("MiB", StringComparison.InvariantCultureIgnoreCase))
                {
                    return ConvertToBytes(Convert.ToDouble(value), 2);
                }

                if (unit.Equals("GB", StringComparison.InvariantCultureIgnoreCase) ||
                        unit.Equals("GiB", StringComparison.InvariantCultureIgnoreCase))
                {
                    return ConvertToBytes(Convert.ToDouble(value), 3);
                }
            }
            return 0;
        }

        private static long ConvertToBytes(double value, int power)
        {
            var multiplier = Math.Pow(1024, power);
            var result = value * multiplier;

            return Convert.ToInt64(result);
        }
    }
}