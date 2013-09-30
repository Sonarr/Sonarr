using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.Newznab
{
    public class NewznabParser : RssParserBase
    {
        protected override string GetNzbInfoUrl(XElement item)
        {
            return item.Comments().Replace("#comments", "");
        }

        protected override long GetSize(XElement item)
        {
            var attributes = item.Elements("attr").ToList();
            var sizeElement = attributes.SingleOrDefault(e => e.Attribute("name").Value.Equals("size", StringComparison.CurrentCultureIgnoreCase));

            if (sizeElement != null)
            {
                return Convert.ToInt64(sizeElement.Attribute("value").Value);
            }

            return ParseSize(item.Description());
        }

        protected override ReleaseInfo PostProcessor(XElement item, ReleaseInfo currentResult)
        {
            if (currentResult != null)
            {
                var attributes = item.Elements("attr").ToList();

                var rageIdElement = attributes.SingleOrDefault(e => e.Attribute("name").Value.Equals("rageid", StringComparison.CurrentCultureIgnoreCase));

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

        protected override void PreProcess(string source, string url)
        {
            NewznabPreProcessor.Process(source, url);
        }
    }
}