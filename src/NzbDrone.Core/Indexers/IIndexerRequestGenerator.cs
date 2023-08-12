using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers
{
    public interface IIndexerRequestGenerator
    {
        IndexerPageableRequestChain GetRecentRequests();
        IndexerPageableRequestChain GetSearchRequests(SingleEpisodeSearchCriteria searchCriteria);
        IndexerPageableRequestChain GetSearchRequests(SeasonSearchCriteria searchCriteria);
        IndexerPageableRequestChain GetSearchRequests(DailyEpisodeSearchCriteria searchCriteria);
        IndexerPageableRequestChain GetSearchRequests(DailySeasonSearchCriteria searchCriteria);
        IndexerPageableRequestChain GetSearchRequests(AnimeEpisodeSearchCriteria searchCriteria);
        IndexerPageableRequestChain GetSearchRequests(AnimeSeasonSearchCriteria searchCriteria);
        IndexerPageableRequestChain GetSearchRequests(SpecialEpisodeSearchCriteria searchCriteria);
    }
}
