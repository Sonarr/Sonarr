using System.Text.RegularExpressions;
using System.Xml.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.ImportLists.Rss.Plex
{
    public class PlexRssImportParser : RssImportBaseParser
    {
        private readonly Logger _logger;
        private static readonly Regex ImdbIdRegex = new (@"(tt\d{7,8})", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public PlexRssImportParser(Logger logger)
            : base(logger)
        {
            _logger = logger;
        }

        protected override ImportListItemInfo ProcessItem(XElement item)
        {
            var category = item.TryGetValue("category");

            if (category != "show")
            {
                return null;
            }

            var info = new ImportListItemInfo
            {
                Title = item.TryGetValue("title", "Unknown")
            };

            var guid = item.TryGetValue("guid", string.Empty);

            if (guid.IsNotNullOrWhiteSpace())
            {
                if (guid.StartsWith("imdb://"))
                {
                    info.ImdbId = ParseImdbId(guid.Replace("imdb://", ""));
                }

                if (int.TryParse(guid.Replace("tvdb://", ""), out var tvdbId))
                {
                    info.TvdbId = tvdbId;
                }

                if (int.TryParse(guid.Replace("tmdb://", ""), out var tmdbId))
                {
                    info.TmdbId = tmdbId;
                }
            }

            if (info.ImdbId.IsNullOrWhiteSpace() && info.TvdbId == 0 && info.TmdbId == 0)
            {
                _logger.Warn("Each item in the RSS feed must have a guid element with a IMDB ID, TVDB ID or TMDB ID: '{0}'", info.Title);

                return null;
            }

            return info;
        }

        private static string ParseImdbId(string value)
        {
            if (value.IsNullOrWhiteSpace())
            {
                return null;
            }

            var match = ImdbIdRegex.Match(value);

            return match.Success ? match.Groups[1].Value : null;
        }
    }
}
