using System;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.NzbClub
{
    public class NzbClubParser : BasicRssParser
    {

        private static readonly Regex SizeRegex = new Regex(@"(?:Size:)\s(?<size>\d+.\d+\s[g|m]i?[b])", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly Logger logger;

        public NzbClubParser()
        {
            logger = LogManager.GetCurrentClassLogger();
        }


        protected override ReportInfo PostProcessor(SyndicationItem item, ReportInfo currentResult)
        {
            if (currentResult != null)
            {
                var match = SizeRegex.Match(item.Summary.Text);

                if (match.Success && match.Groups["size"].Success)
                {
                    currentResult.Size = GetReportSize(match.Groups["size"].Value);
                }
                else
                {
                   logger.Warn("Couldn't parse size from {0}", item.Summary.Text);
                }
            }

            return currentResult;
        }

        protected override string GetTitle(SyndicationItem syndicationItem)
        {
            var title = ParseHeader(syndicationItem.Title.Text);

            if (String.IsNullOrWhiteSpace(title))
                return syndicationItem.Title.Text;

            return title;
        }

        protected override string GetNzbInfoUrl(SyndicationItem item)
        {
            return item.Links[1].Uri.ToString();
        }
    }
}