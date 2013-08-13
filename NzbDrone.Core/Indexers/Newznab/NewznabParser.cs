using System;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;
using NLog;
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
                var attributes = item.Elements(NewznabNamespace + "attr");
                var sizeElement = attributes.SingleOrDefault(e => e.Attribute("name").Value == "size");

                if (sizeElement == null)
                {
                    var message = String.Format("Unable to parse size from: {0} [{1}]", currentResult.Title, currentResult.Indexer);
                    throw new SizeParsingException(message);
                }

                currentResult.Size = Convert.ToInt64(sizeElement.Attribute("value").Value);
            }

            return currentResult;
        }
    }
}