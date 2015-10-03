using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers.KickassTorrents
{
    public class KickassTorrentsRequestGenerator : IIndexerRequestGenerator
    {
        public KickassTorrentsSettings Settings { get; set; }

        public int MaxPages { get; set; }
        public int PageSize { get; set; }

        public KickassTorrentsRequestGenerator()
        {
            MaxPages = 30;
            PageSize = 25;
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetRecentRequests()
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();

            pageableRequests.AddIfNotNull(GetPagedRequests(MaxPages, "tv"));

            return pageableRequests;
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(SingleEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();

            foreach (var queryTitle in searchCriteria.QueryTitles)
            {
                pageableRequests.AddIfNotNull(GetPagedRequests(MaxPages, "usearch",
                    PrepareQuery(queryTitle),
                    "category:tv",
                    string.Format("season:{0}", searchCriteria.SeasonNumber),
                    string.Format("episode:{0}", searchCriteria.EpisodeNumber)));

                pageableRequests.AddIfNotNull(GetPagedRequests(MaxPages, "usearch",
                    PrepareQuery(queryTitle),
                    string.Format("S{0:00}E{1:00}", searchCriteria.SeasonNumber, searchCriteria.EpisodeNumber),
                    "category:tv"));
            }

            return pageableRequests;
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(SeasonSearchCriteria searchCriteria)
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();

            foreach (var queryTitle in searchCriteria.QueryTitles)
            {
                pageableRequests.AddIfNotNull(GetPagedRequests(MaxPages, "usearch",
                    PrepareQuery(queryTitle),
                    "category:tv",
                    string.Format("season:{0}", searchCriteria.SeasonNumber)));

                pageableRequests.AddIfNotNull(GetPagedRequests(MaxPages, "usearch",
                    PrepareQuery(queryTitle),
                    "category:tv",
                    string.Format("S{0:00}", searchCriteria.SeasonNumber)));
            }

            return pageableRequests;
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(DailyEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();

            foreach (var queryTitle in searchCriteria.QueryTitles)
            {
                pageableRequests.AddIfNotNull(GetPagedRequests(MaxPages, "usearch",
                    PrepareQuery(queryTitle),
                    string.Format("{0:yyyy-MM-dd}", searchCriteria.AirDate),
                    "category:tv"));
            }

            return pageableRequests;
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(AnimeEpisodeSearchCriteria searchCriteria)
        {
            return new List<IEnumerable<IndexerRequest>>();
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(SpecialEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();

            foreach (var queryTitle in searchCriteria.EpisodeQueryTitles)
            {
                pageableRequests.AddIfNotNull(GetPagedRequests(MaxPages, "usearch",
                    PrepareQuery(queryTitle),
                    "category:tv"));
            }

            return pageableRequests;
        }

        private IEnumerable<IndexerRequest> GetPagedRequests(int maxPages, string rssType, params string[] searchParameters)
        {
            string searchUrl = null;

            if (searchParameters.Any())
            {
                // Prevent adding a '/' if no search parameters are specified
                if (Settings.VerifiedOnly)
                {
                    searchUrl = string.Format("/{0} verified:1", string.Join(" ", searchParameters));
                }
                else
                {
                    searchUrl = string.Format("/{0}", string.Join(" ", searchParameters)).Trim();
                }
            }

            if (PageSize == 0)
            {
                var request = new IndexerRequest(string.Format("{0}/{1}{2}/?rss=1&field=time_add&sorder=desc", Settings.BaseUrl.TrimEnd('/'), rssType, searchUrl), HttpAccept.Rss);
                request.HttpRequest.SuppressHttpError = true;

                yield return request;
            }
            else
            {
                for (var page = 0; page < maxPages; page++)
                {
                    var request = new IndexerRequest(string.Format("{0}/{1}{2}/{3}/?rss=1&field=time_add&sorder=desc", Settings.BaseUrl.TrimEnd('/'), rssType, searchUrl, page + 1), HttpAccept.Rss);
                    request.HttpRequest.SuppressHttpError = true;

                    yield return request;
                }
            }
        }

        private string PrepareQuery(string query)
        {
            return query.Replace('+', ' ');
        }
    }
}
