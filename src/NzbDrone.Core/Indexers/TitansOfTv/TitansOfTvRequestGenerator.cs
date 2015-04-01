using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Indexers.TitansOfTv
{
    public class TitansOfTvRequestGenerator : IIndexerRequestGenerator
    {
        public TitansOfTvSettings Settings { get; set; }

        public IList<IEnumerable<IndexerRequest>> GetRecentRequests()
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();

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
