using System;
using System.Linq;
using System.Xml.Linq;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.Newznab
{
    public class NewznabParser : BasicRssParser
    {
        private static readonly XNamespace NewznabNamespace = "http://www.newznab.com/DTD/2010/feeds/attributes/";

        protected override string GetNzbInfoUrl(XElement item)
        {
            return item.Comments().Replace("#comments", "");
        }

        protected override ReportInfo PostProcessor(XElement item, ReportInfo currentResult)
        {
            if (currentResult != null)
            {
                var attributes = item.Elements(NewznabNamespace + "attr").ToList();
                var sizeElement = attributes.SingleOrDefault(e => e.Attribute("name").Value.Equals("size", StringComparison.CurrentCultureIgnoreCase));
                var rageIdElement = attributes.SingleOrDefault(e => e.Attribute("name").Value.Equals("rageid", StringComparison.CurrentCultureIgnoreCase));

                if (sizeElement == null)
                {
                    throw new SizeParsingException("Unable to parse size from: {0} [{1}]", currentResult.Title, currentResult.Indexer);
                }

                currentResult.Size = Convert.ToInt64(sizeElement.Attribute("value").Value);

                if (rageIdElement != null)
                {
                    int tvRageId;

                    if (Int32.TryParse(rageIdElement.Attribute("value").Value, out tvRageId))
                    {
                        currentResult.TvRageId = tvRageId;
                    }
                }
            }

            return currentResult;
        }
    }
}