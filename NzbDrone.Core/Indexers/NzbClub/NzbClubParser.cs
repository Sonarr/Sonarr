using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using NLog;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.NzbClub
{
    public class NzbClubParser : BasicRssParser
    {

        private static readonly Regex SizeRegex = new Regex(@"(?:Size:)\s(?<size>\d+.\d+\s[g|m]i?[b])", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly Logger logger;

        public NzbClubParser()
        {
            logger =  NzbDroneLogger.GetLogger();
        }


        protected override ReportInfo PostProcessor(XElement item, ReportInfo currentResult)
        {
            if (currentResult != null)
            {
                var match = SizeRegex.Match(item.Description());

                if (match.Success && match.Groups["size"].Success)
                {
                    currentResult.Size = GetReportSize(match.Groups["size"].Value);
                }
                else
                {
                   logger.Warn("Couldn't parse size from {0}", item.Description());
                }
            }

            return currentResult;
        }

        protected override string GetTitle(XElement item)
        {
            var title = ParseHeader(item.Title());

            if (String.IsNullOrWhiteSpace(title))
                return item.Title();

            return title;
        }

        protected override string GetNzbInfoUrl(XElement item)
        {
            return item.Links().First();
        }

        protected override string GetNzbUrl(XElement item)
        {
            var enclosure = item.Element("enclosure");

            return enclosure.Attribute("url").Value;
        }
    }
}