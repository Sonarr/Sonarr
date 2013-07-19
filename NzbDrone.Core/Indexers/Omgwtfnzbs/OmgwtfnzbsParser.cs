using System;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.Omgwtfnzbs
{
    public class OmgwtfnzbsParser : BasicRssParser
    {
        protected override string GetNzbUrl(SyndicationItem item)
        {
            return item.Links[0].Uri.ToString();
        }

        protected override string GetNzbInfoUrl(SyndicationItem item)
        {
            //Todo: Me thinks I need to parse details to get this...
            var match = Regex.Match(item.Summary.Text, @"(?:\<b\>View NZB\:\<\/b\>\s\<a\shref\=\"")(?<URL>.+)(?:\""\starget)",
                                    RegexOptions.IgnoreCase | RegexOptions.Compiled);

            if (match.Success)
            {
                return match.Groups["URL"].Value;
            }

            return String.Empty;
        }

        protected override ReportInfo PostProcessor(SyndicationItem item, ReportInfo currentResult)
        {
            if (currentResult != null)
            {
                var sizeString = Regex.Match(item.Summary.Text, @"Size:\<\/b\>\s\d+\.\d{1,2}\s\w{2}\<br \/\>", RegexOptions.IgnoreCase | RegexOptions.Compiled).Value;
                currentResult.Size = GetReportSize(sizeString);
            }

            return currentResult;
        }
    }
}