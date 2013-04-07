using System.ServiceModel.Syndication;
using NzbDrone.Core.Model;

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

        protected override IndexerParseResult PostProcessor(SyndicationItem item, IndexerParseResult currentResult)
        {
            if (currentResult != null)
            {
                currentResult.Size = 0;
            }

            return currentResult;
        }
    }
}