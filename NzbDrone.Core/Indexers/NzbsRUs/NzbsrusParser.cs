using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using NzbDrone.Core.Model;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.NzbsRUs
{
    public class NzbsrusParser : BasicRssParser
    {
        protected override ReportInfo PostProcessor(SyndicationItem item, ReportInfo currentResult)
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