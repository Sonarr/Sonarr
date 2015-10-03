using System;
using System.Linq;
using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers.BroadcastheNet
{
    public class BroadcastheNetRequestGenerator : IIndexerRequestGenerator
    {
        public int MaxPages { get; set; }
        public int PageSize { get; set; }
        public BroadcastheNetSettings Settings { get; set; }

        public BroadcastheNetRequestGenerator()
        {
            MaxPages = 10;
            PageSize = 100;
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetRecentRequests()
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();

            pageableRequests.AddIfNotNull(GetPagedRequests(MaxPages, null));

            return pageableRequests;
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(SingleEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequest = new List<IEnumerable<IndexerRequest>>();

            var parameters = new BroadcastheNetTorrentQuery();
            if (AddSeriesSearchParameters(parameters, searchCriteria))
            {
                foreach (var episode in searchCriteria.Episodes)
                {
                    parameters = parameters.Clone();

                    parameters.Category = "Episode";
                    parameters.Name = string.Format("S{0:00}E{1:00}", episode.SeasonNumber, episode.EpisodeNumber);

                    pageableRequest.AddIfNotNull(GetPagedRequests(MaxPages, parameters));
                }

                foreach (var seasonNumber in searchCriteria.Episodes.Select(v => v.SeasonNumber).Distinct())
                {
                    parameters = parameters.Clone();

                    parameters.Category = "Season";
                    parameters.Name = string.Format("Season {0}", seasonNumber);

                    pageableRequest.AddIfNotNull(GetPagedRequests(MaxPages, parameters));
                }
            }

            return pageableRequest;
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(SeasonSearchCriteria searchCriteria)
        {
            var pageableRequest = new List<IEnumerable<IndexerRequest>>();

            var parameters = new BroadcastheNetTorrentQuery();
            if (AddSeriesSearchParameters(parameters, searchCriteria))
            {
                foreach (var seasonNumber in searchCriteria.Episodes.Select(v => v.SeasonNumber).Distinct())
                {
                    parameters.Category = "Episode";
                    parameters.Name = string.Format("S{0:00}E%", seasonNumber);

                    pageableRequest.AddIfNotNull(GetPagedRequests(MaxPages, parameters));

                    parameters = parameters.Clone();

                    parameters.Category = "Season";
                    parameters.Name = string.Format("Season {0}", seasonNumber);

                    pageableRequest.AddIfNotNull(GetPagedRequests(MaxPages, parameters));
                }
            }

            return pageableRequest;
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(DailyEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequest = new List<IEnumerable<IndexerRequest>>();
            
            var parameters = new BroadcastheNetTorrentQuery();
            if (AddSeriesSearchParameters(parameters, searchCriteria))
            {
                parameters.Category = "Episode";
                parameters.Name = string.Format("{0:yyyy}.{0:MM}.{0:dd}", searchCriteria.AirDate);

                pageableRequest.AddIfNotNull(GetPagedRequests(MaxPages, parameters));
            }

            return pageableRequest;
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(AnimeEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequest = new List<IEnumerable<IndexerRequest>>();

            var parameters = new BroadcastheNetTorrentQuery();
            if (AddSeriesSearchParameters(parameters, searchCriteria))
            {
                foreach (var episode in searchCriteria.Episodes)
                {
                    parameters = parameters.Clone();

                    parameters.Category = "Episode";
                    parameters.Name = string.Format("S{0:00}E{1:00}", episode.SeasonNumber, episode.EpisodeNumber);

                    pageableRequest.AddIfNotNull(GetPagedRequests(MaxPages, parameters));
                }

                foreach (var seasonNumber in searchCriteria.Episodes.Select(v => v.SeasonNumber).Distinct())
                {
                    parameters = parameters.Clone();

                    parameters.Category = "Season";
                    parameters.Name = string.Format("Season {0}", seasonNumber);

                    pageableRequest.AddIfNotNull(GetPagedRequests(MaxPages, parameters));
                }
            }

            return pageableRequest;
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(SpecialEpisodeSearchCriteria searchCriteria)
        {
            return new List<IEnumerable<IndexerRequest>>();
        }

        private bool AddSeriesSearchParameters(BroadcastheNetTorrentQuery parameters, SearchCriteriaBase searchCriteria)
        {
            if (searchCriteria.Series.TvdbId != 0)
            {
                parameters.Tvdb = string.Format("{0}", searchCriteria.Series.TvdbId);
                return true;
            }
            if (searchCriteria.Series.TvRageId != 0)
            {
                parameters.Tvrage = string.Format("{0}", searchCriteria.Series.TvRageId);
                return true;
            }
            // BTN is very neatly managed, so it's unlikely they map tvrage/tvdb wrongly.
            return false;
        }

        private IEnumerable<IndexerRequest> GetPagedRequests(int maxPages, BroadcastheNetTorrentQuery parameters)
        {
            if (parameters == null)
            {
                parameters = new BroadcastheNetTorrentQuery();
            }

            var builder = new JsonRpcRequestBuilder(Settings.BaseUrl, "getTorrents", new object[] { Settings.ApiKey, parameters, PageSize, 0 });
            builder.SupressHttpError = true;

            for (var page = 0; page < maxPages;page++)
            {
                builder.Parameters[3] = page * PageSize;

                yield return new IndexerRequest(builder.Build(""));
            }
        }
    }
}
