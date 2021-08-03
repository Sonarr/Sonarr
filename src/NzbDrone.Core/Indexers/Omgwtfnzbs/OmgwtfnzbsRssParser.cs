using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Indexers.Exceptions;

namespace NzbDrone.Core.Indexers.Omgwtfnzbs
{
    public class OmgwtfnzbsRssParser : RssParser
    {
        public OmgwtfnzbsRssParser()
        {
            UseEnclosureUrl = true;
            UseEnclosureLength = true;
        }

        protected override bool PreProcess(IndexerResponse indexerResponse)
        {
            var xdoc = LoadXmlDocument(indexerResponse);
            var notice = xdoc.Descendants("notice").FirstOrDefault();

            if (notice == null)
            {
                return true;
            }

            if (!notice.Value.ContainsIgnoreCase("api"))
            {
                return true;
            }

            throw new ApiKeyException(notice.Value);
        }

        protected override string GetInfoUrl(XElement item)
        {
            //Todo: Me thinks I need to parse details to get this...
            var match = Regex.Match(item.Description(),
                @"(?:\<b\>View NZB\:\<\/b\>\s\<a\shref\=\"")(?<URL>.+)(?:\""\starget)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

            if (match.Success)
            {
                return match.Groups["URL"].Value;
            }

            return string.Empty;
        }
    }
}
