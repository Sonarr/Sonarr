using System.Xml.Linq;
using NLog;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.ImportLists.Rss.Plex
{
    public class PlexRssImportParser : RssImportBaseParser
    {
        private readonly Logger _logger;

        public PlexRssImportParser(Logger logger)
        : base(logger)
        {
            _logger = logger;
        }

        protected override ImportListItemInfo ProcessItem(XElement item)
        {
            var info = new ImportListItemInfo();
            var guid = item.TryGetValue("guid", string.Empty);
            var category = item.TryGetValue("category");

            if (category != "show")
            {
                return null;
            }

            info.Title = item.TryGetValue("title", "Unknown");

            if (int.TryParse(guid.Replace("tvdb://", ""), out var tvdbId))
            {
                info.TvdbId = tvdbId;
            }

            if (info.TvdbId == 0)
            {
                throw new UnsupportedFeedException("Each item in the RSS feed must have a guid element with a TVDB ID");
            }

            return info;
        }
    }
}
