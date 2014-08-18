using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FluentValidation.Results;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Indexers.Animezb
{
    public class Animezb : IndexerBase<NullConfig>
    {
        private static readonly Regex RemoveCharactersRegex = new Regex(@"[!?`]", RegexOptions.Compiled);
        private static readonly Regex RemoveSingleCharacterRegex = new Regex(@"\b[a-z0-9]\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex DuplicateCharacterRegex = new Regex(@"[ +]{2,}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
                return new AnimezbParser();
            }
        }

        public override IEnumerable<string> RecentFeed
        {
            get
            {
                yield return "https://animezb.com/rss?cat=anime&max=100";
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
                        return titles.SelectMany(title =>
                        RecentFeed.Select(url => 
                                String.Format("{0}&q={1}", url, GetSearchQuery(title, absoluteEpisodeNumber))));

        }

        public override IEnumerable<string> GetSearchUrls(string query, int offset)
        {
            return new List<string>();
        }

        public override ValidationResult Test()
        {
            return new ValidationResult();
        }

        private String GetSearchQuery(string title, int absoluteEpisodeNumber)
        {
            var match = RemoveSingleCharacterRegex.Match(title);

            if (match.Success)
            {
                title = RemoveSingleCharacterRegex.Replace(title, "");

                //Since we removed a character we need to not wrap it in quotes and hope animedb doesn't give us a million results
                return CleanTitle(String.Format("{0}+{1:00}", title, absoluteEpisodeNumber));
            }

            //Wrap the query in quotes and search!
            return CleanTitle(String.Format("\"{0}+{1:00}\"", title, absoluteEpisodeNumber));
        }

        private String CleanTitle(String title)
        {
            title = RemoveCharactersRegex.Replace(title, "");
            return DuplicateCharacterRegex.Replace(title, "+");
        }
    }
}
