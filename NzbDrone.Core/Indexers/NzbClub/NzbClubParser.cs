using System;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using NzbDrone.Core.Model;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.NzbClub
{
    public class NzbClubParser : BasicRssParser
    {
        protected override ReportInfo PostProcessor(SyndicationItem item, ReportInfo currentResult)
        {
            if (currentResult != null)
            {
                var sizeString = Regex.Match(item.Summary.Text, @"Size:\s\d+\.\d{1,2}\s\w{2}\s", RegexOptions.IgnoreCase | RegexOptions.Compiled).Value;
                currentResult.Size = GetReportSize(sizeString);
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