using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers
{
    public interface IParseFeed
    {
        IEnumerable<ReportInfo> Process(Stream source);
    }

    public class BasicRssParser : IParseFeed
    {
        private readonly Logger _logger;

        public BasicRssParser()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        public IEnumerable<ReportInfo> Process(Stream source)
        {
            var reader = new SyndicationFeedXmlReader(source);
            var feed = SyndicationFeed.Load(reader).Items;

            var result = new List<ReportInfo>();

            foreach (var syndicationItem in feed)
            {
                try
                {
                    var parsedEpisode = ParseFeed(syndicationItem);
                    if (parsedEpisode != null)
                    {
                        parsedEpisode.NzbUrl = GetNzbUrl(syndicationItem);
                        parsedEpisode.NzbInfoUrl = GetNzbInfoUrl(syndicationItem);
                        result.Add(parsedEpisode);
                    }
                }
                catch (Exception itemEx)
                {
                    itemEx.Data.Add("Item", syndicationItem.Title);
                    _logger.ErrorException("An error occurred while processing feed item", itemEx);
                }
            }

            return result;
        }


        protected virtual string GetTitle(SyndicationItem syndicationItem)
        {
            return syndicationItem.Title.Text;
        }

        protected virtual string GetNzbUrl(SyndicationItem item)
        {
            return item.Links[0].Uri.ToString();
        }

        protected virtual string GetNzbInfoUrl(SyndicationItem item)
        {
            return String.Empty;
        }

        protected virtual ReportInfo PostProcessor(SyndicationItem item, ReportInfo currentResult)
        {
            return currentResult;
        }

        private ReportInfo ParseFeed(SyndicationItem item)
        {
            var title = GetTitle(item);

            var reportInfo = new ReportInfo();

            reportInfo.Title = title;
            reportInfo.Age = DateTime.Now.Date.Subtract(item.PublishDate.Date).Days;
            reportInfo.ReleaseGroup = ParseReleaseGroup(title);

            _logger.Trace("Parsed: {0} from: {1}", reportInfo, item.Title.Text);

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
                                                                        RegexOptions.IgnoreCase | RegexOptions.Compiled),
                                                                
                                                                new Regex(@"(?:\[.+\]\W+\[.+\]\W+\[.+\]\W+\"")(?<nzbTitle>.+)(?:\"".+)",
                                                                        RegexOptions.IgnoreCase | RegexOptions.Compiled),
                                                                    
                                                                new Regex(@"(?:\[)(?<nzbTitle>.+)(?:\]\-.+)",
                                                                        RegexOptions.IgnoreCase | RegexOptions.Compiled),
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

        private static readonly Regex ReportSizeRegex = new Regex(@"(?<value>\d+\.\d{1,2}|\d+\,\d+\.\d{1,2})\W?(?<unit>GB|MB|GiB|MiB)",
                                                                  RegexOptions.IgnoreCase | RegexOptions.Compiled);


        public static long GetReportSize(string sizeString)
        {
            var match = ReportSizeRegex.Matches(sizeString);

            if (match.Count != 0)
            {
                var cultureInfo = new CultureInfo("en-US");
                var value = Decimal.Parse(Regex.Replace(match[0].Groups["value"].Value, "\\,", ""), cultureInfo);

                var unit = match[0].Groups["unit"].Value;

                if (unit.Equals("MB", StringComparison.InvariantCultureIgnoreCase) || unit.Equals("MiB", StringComparison.InvariantCultureIgnoreCase))
                    return Convert.ToInt64(value * 1048576L);

                if (unit.Equals("GB", StringComparison.InvariantCultureIgnoreCase) || unit.Equals("GiB", StringComparison.InvariantCultureIgnoreCase))
                    return Convert.ToInt64(value * 1073741824L);
            }
            return 0;
        }
    }
}