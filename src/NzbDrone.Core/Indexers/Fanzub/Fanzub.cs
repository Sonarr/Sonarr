using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FluentValidation.Results;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Indexers.Fanzub
{
    public class Fanzub : IndexerBase<NullConfig>
    {
        private static readonly Regex RemoveCharactersRegex = new Regex(@"[!?`]", RegexOptions.Compiled);

        public override DownloadProtocol Protocol
        {
            get
            {
                return DownloadProtocol.Usenet;
            }
        }

        public override bool SupportsSearch
        {
            get
            {
                return true;
            }
        }

        public override IParseFeed Parser
        {
            get
            {
                return new FanzubParser();
            }
        }

        public override IEnumerable<string> RecentFeed
        {
            get
            {
                yield return "https://fanzub.com/rss/?cat=anime&max=100";
            }
        }

        public override IEnumerable<string> GetEpisodeSearchUrls(List<String> titles, int tvRageId, int seasonNumber, int episodeNumber)
        {
            return new List<string>();
        }

        public override IEnumerable<string> GetSeasonSearchUrls(List<String> titles, int tvRageId, int seasonNumber, int offset)
        {
            return new List<string>();
        }

        public override IEnumerable<string> GetDailyEpisodeSearchUrls(List<String> titles, int tvRageId, DateTime date)
        {
            return new List<string>();
        }

        public override IEnumerable<string> GetAnimeEpisodeSearchUrls(List<String> titles, int tvRageId, int absoluteEpisodeNumber)
        {
            return RecentFeed.Select(url => String.Format("{0}&q={1}",
                                            url,
                                            String.Join("|", titles.SelectMany(title => GetTitleSearchStrings(title, absoluteEpisodeNumber)))));
        }

        public override IEnumerable<string> GetSearchUrls(string query, int offset)
        {
            return new List<string>();
        }

        public override ValidationResult Test()
        {
            return new ValidationResult();
        }

        private IEnumerable<String> GetTitleSearchStrings(string title, int absoluteEpisodeNumber)
        {
            var formats = new[] { "{0}%20{1:00}", "{0}%20-%20{1:00}" };

            return formats.Select(s => "\"" + String.Format(s, CleanTitle(title), absoluteEpisodeNumber) + "\"" );
        }

        private String CleanTitle(String title)
        {
            return RemoveCharactersRegex.Replace(title, "");
        }
    }
}
