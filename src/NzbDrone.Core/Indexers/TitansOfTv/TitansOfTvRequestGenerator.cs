using System;
using System.Collections.Generic;
using System.Globalization;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers.TitansOfTv
{
    public class TitansOfTvRequestGenerator : IIndexerRequestGenerator
    {
        public int MaxPages { get; set; }
        public int PageSize { get; set; }
        public TitansOfTvSettings Settings { get; set; }
        
        public TitansOfTvRequestGenerator()
        {
            MaxPages = 30;
            PageSize = 100;
        }

        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetPagedRequests(MaxPages));

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SingleEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetPagedRequests(MaxPages,
                series_id: searchCriteria.Series.TvdbId,
                episode: string.Format("S{0:00}E{1:00}", searchCriteria.SeasonNumber, searchCriteria.EpisodeNumber)));

            pageableRequests.Add(GetPagedRequests(MaxPages,
                series_id: searchCriteria.Series.TvdbId,
                season: string.Format("Season {0:00}", searchCriteria.SeasonNumber)));

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SeasonSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetPagedRequests(MaxPages,
                series_id: searchCriteria.Series.TvdbId,
                season: string.Format("Season {0:00}", searchCriteria.SeasonNumber)));

            pageableRequests.AddTier();

            // TODO: Search for all episodes?!?

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(DailyEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetPagedRequests(MaxPages,
                series_id: searchCriteria.Series.TvdbId,
                air_date: searchCriteria.AirDate));

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(AnimeEpisodeSearchCriteria searchCriteria)
        {
            return new IndexerPageableRequestChain();
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SpecialEpisodeSearchCriteria searchCriteria)
        {
            return new IndexerPageableRequestChain();
        }

        private IEnumerable<IndexerRequest> GetPagedRequests(int maxPages, int? series_id = null, string episode = null, string season = null, DateTime? air_date = null)
        {
            var pageSize = PageSize;

            if (pageSize == 0)
            {
                maxPages = 1;
                pageSize = 100;
            }

            for (var page = 0; page < maxPages; page++)
            {
                var request = new IndexerRequest(string.Format("{0}/torrents?offset={1}&limit={2}", Settings.BaseUrl.TrimEnd('/'), page * pageSize, pageSize), HttpAccept.Json);
                request.HttpRequest.Headers.Add("X-Authorization", Settings.ApiKey);

                if (series_id.HasValue)
                {
                    request.HttpRequest.AddQueryParam("series_id", series_id.Value.ToString(CultureInfo.InvariantCulture));
                }
                
                if (season != null)
                {
                    request.HttpRequest.AddQueryParam("season", season);
                }
                
                if (episode != null)
                {
                    request.HttpRequest.AddQueryParam("episode", episode);
                }
                
                if (air_date.HasValue)
                {
                    request.HttpRequest.AddQueryParam("air_date", air_date.Value.ToString("yyyy-MM-dd"));
                }

                yield return request;
            }
        }
    }
}
