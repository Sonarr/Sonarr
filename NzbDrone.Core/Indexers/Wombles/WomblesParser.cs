using System.Xml.Linq;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.Wombles
{
    public class WomblesParser : BasicRssParser
    {
        protected override string GetNzbInfoUrl(XElement item)
        {
            return null;
        }

        protected override ReportInfo PostProcessor(XElement item, ReportInfo currentResult)
        {
            if (currentResult != null)
            {
                currentResult.Size = 0;
            }

            return currentResult;
        }
    }
}