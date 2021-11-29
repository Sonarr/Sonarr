using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers.Fanzub
{
    public class FanzubRequestGenerator : IIndexerRequestGenerator
    {
        private static readonly Regex RemoveCharactersRegex = new Regex(@"[!?`]", RegexOptions.Compiled);

        public FanzubSettings Settings { get; set; }
        public int PageSize { get; set; }

        public FanzubRequestGenerator()
        {
            PageSize = 100;
        }

        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetPagedRequests(null));

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SingleEpisodeSearchCriteria searchCriteria)
        {
            return new IndexerPageableRequestChain();
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SeasonSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            if (Settings.AnimeStandardFormatSearch && searchCriteria.SeasonNumber > 0)
            {
                var searchTitles = searchCriteria.CleanSceneTitles.SelectMany(v => GetSeasonSearchStrings(v, searchCriteria.SeasonNumber)).ToList();
                pageableRequests.Add(GetPagedRequests(string.Join("|", searchTitles)));
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(DailyEpisodeSearchCriteria searchCriteria)
        {
            return new IndexerPageableRequestChain();
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(DailySeasonSearchCriteria searchCriteria)
        {
            return new IndexerPageableRequestChain();
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(AnimeEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            var searchTitles = searchCriteria.CleanSceneTitles.SelectMany(v => GetTitleSearchStrings(v, searchCriteria.AbsoluteEpisodeNumber)).ToList();

            if (Settings.AnimeStandardFormatSearch && searchCriteria.SeasonNumber > 0 && searchCriteria.EpisodeNumber > 0)
            {
                searchTitles.AddRange(searchCriteria.CleanSceneTitles.SelectMany(v => GetTitleSearchStrings(v, searchCriteria.SeasonNumber, searchCriteria.EpisodeNumber)).ToList());
            }

            pageableRequests.Add(GetPagedRequests(string.Join("|", searchTitles)));

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SpecialEpisodeSearchCriteria searchCriteria)
        {
            return new IndexerPageableRequestChain();
        }

        private IEnumerable<IndexerRequest> GetPagedRequests(string query)
        {
            var url = new StringBuilder();
            url.AppendFormat("{0}?cat=anime&max={1}", Settings.BaseUrl, PageSize);

            if (query.IsNotNullOrWhiteSpace())
            {
                url.AppendFormat("&q={0}", query);
            }

            yield return new IndexerRequest(url.ToString(), HttpAccept.Rss);
        }

        private IEnumerable<string> GetSeasonSearchStrings(string title, int seasonNumber)
        {
            var formats = new[] { "{0}%20S{1:00}", "{0}%20-%20S{1:00}" };

            return formats.Select(s => "\"" + string.Format(s, CleanTitle(title), seasonNumber) + "\"");
        }

        private IEnumerable<string> GetTitleSearchStrings(string title, int absoluteEpisodeNumber)
        {
            var formats = new[] { "{0}%20{1:00}", "{0}%20-%20{1:00}" };

            return formats.Select(s => "\"" + string.Format(s, CleanTitle(title), absoluteEpisodeNumber) + "\"");
        }

        private IEnumerable<string> GetTitleSearchStrings(string title, int seasonNumber, int episodeNumber)
        {
            var formats = new[] { "{0}%20S{1:00}E{2:00}", "{0}%20-%20S{1:00}E{2:00}" };

            return formats.Select(s => "\"" + string.Format(s, CleanTitle(title), seasonNumber, episodeNumber) + "\"");
        }

        private string CleanTitle(string title)
        {
            return RemoveCharactersRegex.Replace(title, "");
        }
    }
}
