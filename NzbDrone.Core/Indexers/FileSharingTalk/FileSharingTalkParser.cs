using System.ServiceModel.Syndication;
using NzbDrone.Core.Model;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.FileSharingTalk
{
    public class FileSharingTalkParser : BasicRssParser
    {
        protected override string GetNzbInfoUrl(SyndicationItem item)
        {
            return item.Id;
        }

        protected override ReportInfo PostProcessor(SyndicationItem item, ReportInfo currentResult)
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