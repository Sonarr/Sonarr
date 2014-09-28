using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NzbDrone.Common;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers.BroadcastheNet
{
    public class BroadcastheNetRequestGenerator : IIndexerRequestGenerator
    {
        public Int32 MaxPages { get; set; }
        public Int32 PageSize { get; set; }
        public BroadcastheNetSettings Settings { get; set; }

        public BroadcastheNetRequestGenerator()
        {
            MaxPages = 10;
            PageSize = 100;
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetRecentRequests()
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();

            pageableRequests.AddIfNotNull(GetPagedRequests(1, null));

            return pageableRequests;
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(SingleEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequest = new List<IEnumerable<IndexerRequest>>();

            var parameters = new BroadcastheNetTorrentQuery();
            if (AddSeriesSearchParameters(parameters, searchCriteria))
            {
                parameters.Category = "Episode";
                parameters.Name = String.Format("S{0:00}E{1:00}", searchCriteria.SeasonNumber, searchCriteria.EpisodeNumber);

                pageableRequest.AddIfNotNull(GetPagedRequests(MaxPages, parameters));
            }

            return pageableRequest;
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(SeasonSearchCriteria searchCriteria)
        {
            var pageableRequest = new List<IEnumerable<IndexerRequest>>();

            var parameters = new BroadcastheNetTorrentQuery();
            if (AddSeriesSearchParameters(parameters, searchCriteria))
            {
                parameters.Category = "Episode";
                parameters.Name = String.Format("S{0:00}E%", searchCriteria.SeasonNumber);

                pageableRequest.AddIfNotNull(GetPagedRequests(MaxPages, parameters));

                parameters = parameters.Clone();

                parameters.Category = "Season";
                parameters.Name = String.Format("Season {0}", searchCriteria.SeasonNumber);

                pageableRequest.AddIfNotNull(GetPagedRequests(MaxPages, parameters));
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
                parameters.Name = String.Format("{0:yyyy}.{0:MM}.{0:dd}", searchCriteria.AirDate);

                pageableRequest.AddIfNotNull(GetPagedRequests(MaxPages, parameters));
            }

            return pageableRequest;
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(AnimeEpisodeSearchCriteria searchCriteria)
        {
            return new List<IEnumerable<IndexerRequest>>();
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(SpecialEpisodeSearchCriteria searchCriteria)
        {
            return new List<IEnumerable<IndexerRequest>>();
        }

        private bool AddSeriesSearchParameters(BroadcastheNetTorrentQuery parameters, SearchCriteriaBase searchCriteria)
        {
            if (searchCriteria.Series.TvRageId != 0)
            {
                parameters.Tvrage = String.Format("{0}", searchCriteria.Series.TvRageId);
                return true;
            }
            else if (searchCriteria.Series.TvdbId != 0)
            {
                parameters.Tvdb = String.Format("{0}", searchCriteria.Series.TvdbId);
                return true;
            }
            else
            {
                // BTN is very neatly managed, so it's unlikely they map tvrage/tvdb wrongly.
                return false;
            }
        }

        private IEnumerable<IndexerRequest> GetPagedRequests(Int32 maxPages, BroadcastheNetTorrentQuery parameters)
        {
            if (parameters == null)
            {
                parameters = new BroadcastheNetTorrentQuery();
            }

            var builder = new JsonRpcRequestBuilder(Settings.BaseUrl, "getTorrents", new Object[] { Settings.ApiKey, parameters, PageSize, 0 });
            builder.SupressHttpError = true;

            for (var page = 0; page < maxPages;page++)
            {
                builder.Parameters[3] = page * PageSize;

                yield return new IndexerRequest(builder.Build(""));
            }
        }
    }
}
