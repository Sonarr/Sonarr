using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.Newznab
{
    public class NewznabParser : RssParserBase
    {

        private static readonly string[] IgnoredErrors =
        {
            "Request limit reached",
        };

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

        public override IEnumerable<ReleaseInfo> Process(string xml, string url)
        {
            try
            {
                return base.Process(xml, url);
            }
            catch (NewznabException e)
            {
                if (!IgnoredErrors.Any(ignoredError => e.Message.Contains(ignoredError)))
                {
                    throw;
                }
                _logger.Error(e.Message);
                return new List<ReleaseInfo>();
            }
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