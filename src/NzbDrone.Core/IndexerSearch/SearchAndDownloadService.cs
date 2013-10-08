using System;

namespace NzbDrone.Core.IndexerSearch
{

    interface ISearchAndDownload
    {
        void SearchSingle(int seriesId, int seasonNumber, int episodeNumber);
        void SearchDaily(int seriesId, DateTime airDate);
        void SearchSeason(int seriesId, int seasonNumber);
    }

   /* public class SearchAndDownloadService : ISearchAndDownload
    {
        private readonly ISearchForNzb _searchService;
        private readonly IMakeDownloadDecision _downloadDecisionMaker;

        public SearchAndDownloadService(ISearchForNzb searchService, IMakeDownloadDecision downloadDecisionMaker)
        {
            _searchService = searchService;
            _downloadDecisionMaker = downloadDecisionMaker;
        }

        public void FetchSearchSingle(int seriesId, int seasonNumber, int episodeNumber)
        {
            var result = _searchService.SearchSingle(seriesId, seasonNumber, episodeNumber);
        }

        public void SearchDaily(int seriesId, DateTime airDate)
        {
            throw new NotImplementedException();
        }

        public void SearchSeason(int seriesId, int seasonNumber)
        {
            throw new NotImplementedException();
        }
    }*/
}