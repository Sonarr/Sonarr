using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using Ninject;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Search;
using PetaPoco;

namespace NzbDrone.Core.Providers
{
    public class SearchResultProvider
    {
        private readonly IDatabase _database;
        private readonly SeriesProvider _seriesProvider;
        private readonly DownloadProvider _downloadProvider;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public SearchResultProvider(IDatabase database, SeriesProvider seriesProvider,
                                        DownloadProvider downloadProvider)
        {
            _database = database;
            _seriesProvider = seriesProvider;
            _downloadProvider = downloadProvider;
        }

        public SearchResultProvider()
        {
            
        }

        public virtual void Add(SearchResult searchResult)
        {
            logger.Trace("Adding new search result");
            searchResult.SuccessfulDownload = searchResult.SearchResultItems.Any(s => s.Success);
            var id = Convert.ToInt32(_database.Insert(searchResult));

            searchResult.SearchResultItems.ForEach(s => s.SearchResultId = id);
            logger.Trace("Adding search result items");
            _database.InsertMany(searchResult.SearchResultItems);
        }

        public virtual void Delete(int id)
        {
            logger.Trace("Deleting search result items attached to: {0}", id);
            _database.Execute("DELETE FROM SearchResultItems WHERE SearchResultId = @0", id);

            logger.Trace("Deleting search result: {0}", id);
            _database.Delete<SearchResult>(id);
        }

        public virtual List<SearchResult> AllSearchResults()
        {
            var sql = @"SELECT SearchResults.Id, SearchResults.SeriesId, SearchResults.SeasonNumber,
                        SearchResults.EpisodeId, SearchResults.SearchTime,
                        Series.Title as SeriesTitle, Series.IsDaily,
                        Episodes.EpisodeNumber, Episodes.SeasonNumber, Episodes.Title as EpisodeTitle,
                        Episodes.AirDate,
                        Count(SearchResultItems.Id) as TotalItems,
                        SUM(CASE WHEN SearchResultItems.Success = 1 THEN 1 ELSE 0 END) as SuccessfulCount
                        FROM SearchResults
                        INNER JOIN Series
                        ON Series.SeriesId = SearchResults.SeriesId
                        LEFT JOIN Episodes
                        ON Episodes.EpisodeId = SearchResults.EpisodeId
                        INNER JOIN SearchResultItems
                        ON SearchResultItems.SearchResultId = SearchResults.Id
                        GROUP BY SearchResults.Id, SearchResults.SeriesId, SearchResults.SeasonNumber,
                        SearchResults.EpisodeId, SearchResults.SearchTime,
                        Series.Title, Series.IsDaily,
                        Episodes.EpisodeNumber, Episodes.SeasonNumber, Episodes.Title,
                        Episodes.AirDate";

            return _database.Fetch<SearchResult>(sql);
        }

        public virtual SearchResult GetSearchResult(int id)
        {
            var sql = @"SELECT SearchResults.Id, SearchResults.SeriesId, SearchResults.SeasonNumber,
                        SearchResults.EpisodeId, SearchResults.SearchTime,
                        Series.Title as SeriesTitle, Series.IsDaily,
                        Episodes.EpisodeNumber, Episodes.SeasonNumber, Episodes.Title as EpisodeTitle,
                        Episodes.AirDate
                        FROM SearchResults
                        INNER JOIN Series
                        ON Series.SeriesId = SearchResults.SeriesId
                        LEFT JOIN Episodes
                        ON Episodes.EpisodeId = SearchResults.EpisodeId
                        WHERE SearchResults.Id = @0";

            var result = _database.Single<SearchResult>(sql, id);
            result.SearchResultItems = _database.Fetch<SearchResultItem>("WHERE SearchResultId = @0", id);

            return result;
        }

        public virtual void ForceDownload(int itemId)
        {
            var item = _database.Single<SearchResultItem>(itemId);
            var searchResult = _database.Single<SearchResult>(item.SearchResultId);
            var series = _seriesProvider.GetSeries(searchResult.SeriesId);

            var parseResult = Parser.ParseTitle(item.ReportTitle);
            parseResult.NzbUrl = item.NzbUrl;
            parseResult.Series = series;
            parseResult.Indexer = item.Indexer;

            _downloadProvider.DownloadReport(parseResult);
        }
    }
}
