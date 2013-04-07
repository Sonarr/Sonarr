using System.ServiceModel.Syndication;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.Indexers.FileSharingTalk
{
    public class FileSharingTalkParser : BasicRssParser
    {
        protected override string GetNzbInfoUrl(SyndicationItem item)
        {
            return item.Id;
        }

        protected override IndexerParseResult PostProcessor(SyndicationItem item, IndexerParseResult currentResult)
        {
            if (currentResult != null)
            {
                currentResult.Size = 0;
                currentResult.Age = 0;
            }

            return currentResult;
        }
    }
}