using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers.BroadcastheNet
{
    public class BroadcastheNetRequestGenerator : IIndexerRequestGenerator
    {
        public int MaxPages { get; set; }
        public int PageSize { get; set; }
        public BroadcastheNetSettings Settings { get; set; }

        public int? LastRecentTorrentID { get; set; }

        public BroadcastheNetRequestGenerator()
        {
            MaxPages = 10;
            PageSize = 100;
        }

        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();

            if (LastRecentTorrentID.HasValue)
            {
                pageableRequests.Add(GetPagedRequests(MaxPages, new BroadcastheNetTorrentQuery()
                {
                    Id = ">=" + (LastRecentTorrentID.Value - 100)
                }));
            }

            pageableRequests.AddTier(GetPagedRequests(MaxPages, new BroadcastheNetTorrentQuery()
            {
                Age = "<=86400"
            }));

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SingleEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            var parameters = new BroadcastheNetTorrentQuery();
            if (AddSeriesSearchParameters(parameters, searchCriteria))
            {
                foreach (var episode in searchCriteria.Episodes)
                {
                    parameters = parameters.Clone();

                    parameters.Category = "Episode";
                    parameters.Name = string.Format("S{0:00}%E{1:00}%", episode.SeasonNumber, episode.EpisodeNumber);

                    pageableRequests.Add(GetPagedRequests(MaxPages, parameters));
                }
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SeasonSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            var parameters = new BroadcastheNetTorrentQuery();
            if (AddSeriesSearchParameters(parameters, searchCriteria))
            {
                foreach (var seasonNumber in searchCriteria.Episodes.Select(v => v.SeasonNumber).Distinct())
                {
                    parameters.Category = "Season";
                    parameters.Name = string.Format("Season {0}%", seasonNumber);

                    pageableRequests.Add(GetPagedRequests(MaxPages, parameters));

                    parameters = parameters.Clone();

                    parameters.Category = "Episode";
                    parameters.Name = string.Format("S{0:00}E%", seasonNumber);

                    pageableRequests.Add(GetPagedRequests(MaxPages, parameters));
                }
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(DailyEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            var parameters = new BroadcastheNetTorrentQuery();
            if (AddSeriesSearchParameters(parameters, searchCriteria))
            {
                parameters.Category = "Episode";
                parameters.Name = string.Format("{0:yyyy}.{0:MM}.{0:dd}", searchCriteria.AirDate);

                pageableRequests.Add(GetPagedRequests(MaxPages, parameters));

                pageableRequests.AddTier();

                foreach (var episode in searchCriteria.Episodes)
                {
                    parameters = parameters.Clone();

                    parameters.Category = "Episode";
                    parameters.Name = string.Format("S{0:00}E{1:00}", episode.SeasonNumber, episode.EpisodeNumber);

                    pageableRequests.Add(GetPagedRequests(MaxPages, parameters));
                }
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(DailySeasonSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            var parameters = new BroadcastheNetTorrentQuery();
            if (AddSeriesSearchParameters(parameters, searchCriteria))
            {
                parameters.Category = "Episode";
                parameters.Name = string.Format("{0}%", searchCriteria.Year);

                pageableRequests.Add(GetPagedRequests(MaxPages, parameters));

                pageableRequests.AddTier();

                foreach (var episode in searchCriteria.Episodes)
                {
                    parameters = parameters.Clone();

                    parameters.Category = "Episode";
                    parameters.Name = string.Format("S{0:00}E{1:00}", episode.SeasonNumber, episode.EpisodeNumber);

                    pageableRequests.Add(GetPagedRequests(MaxPages, parameters));
                }
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(AnimeEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            var parameters = new BroadcastheNetTorrentQuery();
            if (AddSeriesSearchParameters(parameters, searchCriteria))
            {
                foreach (var episode in searchCriteria.Episodes)
                {
                    parameters = parameters.Clone();

                    parameters.Category = "Episode";
                    parameters.Name = string.Format("S{0:00}E{1:00}", episode.SeasonNumber, episode.EpisodeNumber);

                    pageableRequests.Add(GetPagedRequests(MaxPages, parameters));
                }

                foreach (var seasonNumber in searchCriteria.Episodes.Select(v => v.SeasonNumber).Distinct())
                {
                    parameters = parameters.Clone();

                    parameters.Category = "Season";
                    parameters.Name = string.Format("Season {0}%", seasonNumber);

                    pageableRequests.Add(GetPagedRequests(MaxPages, parameters));
                }
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SpecialEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            var parameters = new BroadcastheNetTorrentQuery();
            if (AddSeriesSearchParameters(parameters, searchCriteria))
            {
                var episodeQueryTitle = searchCriteria.Episodes.Where(e => !string.IsNullOrWhiteSpace(e.Title))
                                               .Select(e => SearchCriteriaBase.GetCleanSceneTitle(e.Title))
                                               .ToArray();

                foreach (var queryTitle in episodeQueryTitle)
                {
                    parameters = parameters.Clone();

                    parameters.Category = "Episode";
                    parameters.Name = "%" + queryTitle + "%";

                    pageableRequests.Add(GetPagedRequests(MaxPages, parameters));
                }
            }

            return pageableRequests;
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
            var builder = new JsonRpcRequestBuilder(Settings.BaseUrl)
                .Call("getTorrents", Settings.ApiKey, parameters, PageSize, 0);
            builder.SuppressHttpError = true;

            for (var page = 0; page < maxPages; page++)
            {
                builder.JsonParameters[3] = page * PageSize;

                yield return new IndexerRequest(builder.Build());
            }
        }
    }
}
