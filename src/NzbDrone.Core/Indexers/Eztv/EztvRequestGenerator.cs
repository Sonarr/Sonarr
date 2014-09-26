using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Common;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers.Eztv
{
    public class EztvRequestGenerator : IIndexerRequestGenerator
    {
        public EztvSettings Settings { get; set; }

        public EztvRequestGenerator()
        {

        }

        public virtual IList<IEnumerable<IndexerRequest>> GetRecentRequests()
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();

            pageableRequests.AddIfNotNull(GetPagedRequests("/feed/"));

            return pageableRequests;
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(SingleEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();
            
            foreach (var queryTitle in searchCriteria.QueryTitles)
            {
                pageableRequests.AddIfNotNull(GetPagedRequests(String.Format("/search/index.php?show_name={0}&season={1}&episode={2}&mode=rss",
                    queryTitle,
                    searchCriteria.SeasonNumber,
                    searchCriteria.EpisodeNumber)));
            }

            return pageableRequests;
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(SeasonSearchCriteria searchCriteria)
        {
            var pageableRequests = new List<IEnumerable<IndexerRequest>>();
            
            foreach (var queryTitle in searchCriteria.QueryTitles)
            {
                pageableRequests.AddIfNotNull(GetPagedRequests(String.Format("/search/index.php?show_name={0}&season={1}&mode=rss",
                    queryTitle,
                    searchCriteria.SeasonNumber)));
            }

            return pageableRequests;
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(DailyEpisodeSearchCriteria searchCriteria)
        {
            //EZTV doesn't support searching based on actual episode airdate. they only support release date.
            return new List<IEnumerable<IndexerRequest>>();
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(AnimeEpisodeSearchCriteria searchCriteria)
        {
            return new List<IEnumerable<IndexerRequest>>();
        }

        public virtual IList<IEnumerable<IndexerRequest>> GetSearchRequests(SpecialEpisodeSearchCriteria searchCriteria)
        {
            return new List<IEnumerable<IndexerRequest>>();
        }

        private IEnumerable<IndexerRequest> GetPagedRequests(String query)
        {
            yield return new IndexerRequest(Settings.BaseUrl.TrimEnd('/') + query, HttpAccept.Rss);
        }
    }
}
