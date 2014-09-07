using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NzbDrone.Common;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers.Animezb
{
    public class AnimezbRequestGenerator : IIndexerRequestGenerator
    {
        private static readonly Regex RemoveCharactersRegex = new Regex(@"[!?`]", RegexOptions.Compiled);
        private static readonly Regex RemoveSingleCharacterRegex = new Regex(@"\b[a-z0-9]\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex DuplicateCharacterRegex = new Regex(@"[ +]{2,}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public String BaseUrl { get; set; }
        public Int32 PageSize { get; set; }

        public AnimezbRequestGenerator()
        {
            BaseUrl = "https://animezb.com/rss?cat=anime";
            PageSize = 100;
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetRecentRequests()
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();

            pageableRequests.AddIfNotNull(GetPagedRequests(null));

            return pageableRequests;
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(SingleEpisodeSearchCriteria searchCriteria)
        {
            return new List<IEnumerable<IndexerRequest>>();
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(SeasonSearchCriteria searchCriteria)
        {
            return new List<IEnumerable<IndexerRequest>>();
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(DailyEpisodeSearchCriteria searchCriteria)
        {
            return new List<IEnumerable<IndexerRequest>>();
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(AnimeEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();

            foreach (var queryTitle in searchCriteria.QueryTitles)
            {
                var searchQuery = GetSearchQuery(queryTitle, searchCriteria.AbsoluteEpisodeNumber);

                pageableRequests.Add(GetPagedRequests(searchQuery));
            }

            return pageableRequests;
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(SpecialEpisodeSearchCriteria searchCriteria)
        {
            return new List<IEnumerable<IndexerRequest>>();
        }

        private IEnumerable<IndexerRequest> GetPagedRequests(String query)
        {
            var url = new StringBuilder();
            url.AppendFormat("{0}&max={1}", BaseUrl, PageSize);

            if (query.IsNotNullOrWhiteSpace())
            {
                url.AppendFormat("&q={0}", query);
            }

            yield return new IndexerRequest(url.ToString());
        }

        private String GetSearchQuery(String title, Int32 absoluteEpisodeNumber)
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
