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

        protected override IndexerParseResult PostProcessor(SyndicationItem item, IndexerParseResult currentResult)
        {
            if (currentResult != null)
            {
                var sizeString = Regex.Match(item.Summary.Text, @"<b>\d+\.\d{1,2}\s\w{2}</b><br\s/>", RegexOptions.IgnoreCase | RegexOptions.Compiled).Value;
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
    }
}