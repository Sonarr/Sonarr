using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.Indexers.TitansOfTv
{
    public class TitansOfTvRequestGenerator : IIndexerRequestGenerator
    {
        public TitansOfTvSettings Settings { get; set; }

        private const string BASE_RSS_URL = "https://titansof.tv/rss/feed/?apikey={0}";

        public IList<IEnumerable<IndexerRequest>> GetRecentRequests()
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();
            var url = string.Format(BASE_RSS_URL, Settings.ApiKey);
            var innerList = new List<IndexerRequest>();

            innerList.Add(new IndexerRequest(url, HttpAccept.Rss));
            pageableRequests.Add(innerList);

            return pageableRequests;
        }

        public IList<IEnumerable<IndexerRequest>> GetSearchRequests(IndexerSearch.Definitions.SingleEpisodeSearchCriteria searchCriteria)
        {
            return new List<IEnumerable<IndexerRequest>>();
        }

        public IList<IEnumerable<IndexerRequest>> GetSearchRequests(IndexerSearch.Definitions.SeasonSearchCriteria searchCriteria)
        {
            return new List<IEnumerable<IndexerRequest>>();
        }

        public IList<IEnumerable<IndexerRequest>> GetSearchRequests(IndexerSearch.Definitions.DailyEpisodeSearchCriteria searchCriteria)
        {
            return new List<IEnumerable<IndexerRequest>>();
        }

        public IList<IEnumerable<IndexerRequest>> GetSearchRequests(IndexerSearch.Definitions.AnimeEpisodeSearchCriteria searchCriteria)
        {
            return new List<IEnumerable<IndexerRequest>>();
        }

        public IList<IEnumerable<IndexerRequest>> GetSearchRequests(IndexerSearch.Definitions.SpecialEpisodeSearchCriteria searchCriteria)
        {
            return new List<IEnumerable<IndexerRequest>>();
        }
    }
}
