using System;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.Newznab
{
    public class NewznabParser : BasicRssParser
    {
        private static XNamespace NEWZNAB = "http://www.newznab.com/DTD/2010/feeds/attributes/";
        
        private readonly Newznab _newznabIndexer;

        public NewznabParser(Newznab newznabIndexer)
        {
            _newznabIndexer = newznabIndexer;
        }

        protected override string GetNzbInfoUrl(XElement item)
        {
            return item.Comments().Replace("#comments", "");
        }

        protected override ReportInfo PostProcessor(XElement item, ReportInfo currentResult)
        {
            if (currentResult != null)
            {
                var attributes = item.Elements(NEWZNAB + "attr");
                var sizeElement = attributes.Single(e => e.Attribute("name").Value == "size");

                currentResult.Size = Convert.ToInt64(sizeElement.Attribute("value").Value);
            }

            return currentResult;
        }
    }
}