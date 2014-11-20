using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers.KickassTorrents
{
    public class KickassTorrentsRequestGenerator : IIndexerRequestGenerator
    {
        public KickassTorrentsSettings Settings { get; set; }

        public Int32 MaxPages { get; set; }
        public Int32 PageSize { get; set; }

        public KickassTorrentsRequestGenerator()
        {
            MaxPages = 30;
            PageSize = 25;
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetRecentRequests()
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();

            pageableRequests.AddIfNotNull(GetPagedRequests(1, "tv"));

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
                    String.Format("season:{0}", searchCriteria.SeasonNumber),
                    String.Format("episode:{0}", searchCriteria.EpisodeNumber)));

                pageableRequests.AddIfNotNull(GetPagedRequests(MaxPages, "usearch",
                    PrepareQuery(queryTitle),
                    String.Format("S{0:00}E{1:00}", searchCriteria.SeasonNumber, searchCriteria.EpisodeNumber),
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
                    String.Format("season:{0}", searchCriteria.SeasonNumber)));

                pageableRequests.AddIfNotNull(GetPagedRequests(MaxPages, "usearch",
                    PrepareQuery(queryTitle),
                    "category:tv",
                    String.Format("S{0:00}", searchCriteria.SeasonNumber)));
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
                    String.Format("{0:yyyy-MM-dd}", searchCriteria.AirDate),
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

        private IEnumerable<IndexerRequest> GetPagedRequests(Int32 maxPages, String rssType, params String[] searchParameters)
        {
            String searchUrl = null;

            if (searchParameters.Any())
            {
                // Prevent adding a '/' if no search parameters are specified
                if (Settings.VerifiedOnly)
                {
                    searchUrl = String.Format("/{0} verified:1", String.Join(" ", searchParameters));
                }
                else
                {
                    searchUrl = String.Format("/{0}", String.Join(" ", searchParameters)).Trim();
                }
            }

            if (PageSize == 0)
            {
                var request = new IndexerRequest(String.Format("{0}/{1}{2}/?rss=1&field=time_add&sorder=desc", Settings.BaseUrl.TrimEnd('/'), rssType, searchUrl), HttpAccept.Rss);
                request.HttpRequest.SuppressHttpError = true;

                yield return request;
            }
            else
            {
                for (var page = 0; page < maxPages; page++)
                {
                    var request = new IndexerRequest(String.Format("{0}/{1}{2}/{3}/?rss=1&field=time_add&sorder=desc", Settings.BaseUrl.TrimEnd('/'), rssType, searchUrl, page + 1), HttpAccept.Rss);
                    request.HttpRequest.SuppressHttpError = true;

                    yield return request;
                }
            }
        }

        private String PrepareQuery(String query)
        {
            return query.Replace('+', ' ');
        }
    }
}
