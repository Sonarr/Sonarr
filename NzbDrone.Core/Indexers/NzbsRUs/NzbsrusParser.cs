using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.Indexers.NzbsRUs
{
    public class NzbsrusParser : BasicRssParser
    {
        protected override IndexerParseResult PostProcessor(SyndicationItem item, IndexerParseResult currentResult)
        {
            if (currentResult != null)
            {
                var sizeString = Regex.Match(item.Summary.Text, @"\d+\.\d{1,2} \w{3}", RegexOptions.IgnoreCase).Value;
                currentResult.Size = GetReportSize(sizeString);
            }

            return currentResult;
        }
    }
}