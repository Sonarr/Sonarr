using System.ServiceModel.Syndication;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.Wombles
{
    public class WomblesParser : BasicRssParser
    {
        protected override string GetNzbUrl(SyndicationItem item)
        {
            return item.Links[0].Uri.ToString();
        }

        protected override string GetNzbInfoUrl(SyndicationItem item)
        {
            return null;
        }

        protected override ReportInfo PostProcessor(SyndicationItem item, ReportInfo currentResult)
        {
            if (currentResult != null)
            {
                currentResult.Size = 0;
            }

            return currentResult;
        }
    }
}