using System;
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

            var searchTitles = searchCriteria.QueryTitles.SelectMany(v => GetTitleSearchStrings(v, searchCriteria.AbsoluteEpisodeNumber)).ToList();

            pageableRequests.Add(GetPagedRequests(string.Join("|", searchTitles)));

            return pageableRequests;
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(SpecialEpisodeSearchCriteria searchCriteria)
        {
            return new List<IEnumerable<IndexerRequest>>();
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

        private IEnumerable<string> GetTitleSearchStrings(string title, int absoluteEpisodeNumber)
        {
            var formats = new[] { "{0}%20{1:00}", "{0}%20-%20{1:00}" };

            return formats.Select(s => "\"" + string.Format(s, CleanTitle(title), absoluteEpisodeNumber) + "\"");
        }

        private string CleanTitle(string title)
        {
            return RemoveCharactersRegex.Replace(title, "");
        }
    }
}
