using System;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.Indexers.NzbIndex
{
    public class NzbIndexParser : BasicRssParser
    {

        protected override string GetNzbUrl(SyndicationItem item)
        {
            return item.Links[1].Uri.ToString();
        }

        protected override string GetNzbInfoUrl(SyndicationItem item)
        {
            return item.Links[0].Uri.ToString();
        }

        protected override EpisodeParseResult PostProcessor(SyndicationItem item, EpisodeParseResult currentResult)
        {
            if (currentResult != null)
            {
                var sizeString = Regex.Match(item.Summary.Text, @"<b>\d+\.\d{1,2}\s\w{2}</b><br\s/>", RegexOptions.IgnoreCase | RegexOptions.Compiled).Value;
                currentResult.Size = Parser.GetReportSize(sizeString);
            }

            return currentResult;
        }

        protected override string GetTitle(SyndicationItem item)
        {
            var title = Parser.ParseHeader(item.Title.Text);

            if (String.IsNullOrWhiteSpace(title))
                return item.Title.Text;

            return title;
        }
    }
}