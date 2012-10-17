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
    public class SearchHistoryProvider
    {
        private readonly IDatabase _database;
        private readonly SeriesProvider _seriesProvider;
        private readonly DownloadProvider _downloadProvider;
        private readonly EpisodeProvider _episodeProvider;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public SearchHistoryProvider(IDatabase database, SeriesProvider seriesProvider,
                                        DownloadProvider downloadProvider, EpisodeProvider episodeProvider)
        {
            _database = database;
            _seriesProvider = seriesProvider;
            _downloadProvider = downloadProvider;
            _episodeProvider = episodeProvider;
        }

        public SearchHistoryProvider()
        {
            
        }

        public virtual int Add(SearchHistory searchHistory)
        {
            logger.Trace("Adding new search result");
            searchHistory.SuccessfulDownload = searchHistory.SearchHistoryItems.Any(s => s.Success);
            var id = Convert.ToInt32(_database.Insert(searchHistory));

            searchHistory.SearchHistoryItems.ForEach(s => s.SearchHistoryId = id);
            logger.Trace("Adding search result items");
            _database.InsertMany(searchHistory.SearchHistoryItems);

            return id;
        }

        public virtual void Delete(int id)
        {
            logger.Trace("Deleting search result items attached to: {0}", id);
            _database.Execute("DELETE FROM SearchHistoryItems WHERE SearchHistoryId = @0", id);

            logger.Trace("Deleting search result: {0}", id);
            _database.Delete<SearchHistory>(id);
        }

        public virtual List<SearchHistory> AllSearchHistory()
        {
            var sql = @"SELECT SearchHistory.Id, SearchHistory.SeriesId, SearchHistory.SeasonNumber,
                        SearchHistory.EpisodeId, SearchHistory.SearchTime,
                        Series.Title as SeriesTitle, Series.IsDaily,
                        Episodes.EpisodeNumber, Episodes.SeasonNumber, Episodes.Title as EpisodeTitle,
                        Episodes.AirDate,
                        Count(SearchHistoryItems.Id) as TotalItems,
                        SUM(CASE WHEN SearchHistoryItems.Success = 1 THEN 1 ELSE 0 END) as SuccessfulCount
                        FROM SearchHistory
                        INNER JOIN Series
                        ON Series.SeriesId = SearchHistory.SeriesId
                        LEFT JOIN Episodes
                        ON Episodes.EpisodeId = SearchHistory.EpisodeId
                        INNER JOIN SearchHistoryItems
                        ON SearchHistoryItems.SearchHistoryId = SearchHistory.Id
                        GROUP BY SearchHistory.Id, SearchHistory.SeriesId, SearchHistory.SeasonNumber,
                        SearchHistory.EpisodeId, SearchHistory.SearchTime,
                        Series.Title, Series.IsDaily,
                        Episodes.EpisodeNumber, Episodes.SeasonNumber, Episodes.Title,
                        Episodes.AirDate";

            return _database.Fetch<SearchHistory>(sql);
        }

        public virtual SearchHistory GetSearchHistory(int id)
        {
            var sql = @"SELECT SearchHistory.Id, SearchHistory.SeriesId, SearchHistory.SeasonNumber,
                        SearchHistory.EpisodeId, SearchHistory.SearchTime,
                        Series.Title as SeriesTitle, Series.IsDaily,
                        Episodes.EpisodeNumber, Episodes.SeasonNumber, Episodes.Title as EpisodeTitle,
                        Episodes.AirDate
                        FROM SearchHistory
                        INNER JOIN Series
                        ON Series.SeriesId = SearchHistory.SeriesId
                        LEFT JOIN Episodes
                        ON Episodes.EpisodeId = SearchHistory.EpisodeId
                        WHERE SearchHistory.Id = @0";

            var result = _database.Single<SearchHistory>(sql, id);
            result.SearchHistoryItems = _database.Fetch<SearchHistoryItem>("WHERE SearchHistoryId = @0", id);

            return result;
        }

        public virtual void ForceDownload(int itemId)
        {
            var item = _database.Single<SearchHistoryItem>(itemId);
            logger.Info("Starting Force Download of: {0}", item.ReportTitle);
            var searchResult = _database.Single<SearchHistory>(item.SearchHistoryId);
            var series = _seriesProvider.GetSeries(searchResult.SeriesId);
            
            var parseResult = Parser.ParseTitle(item.ReportTitle);
            parseResult.NzbUrl = item.NzbUrl;
            parseResult.Series = series;
            parseResult.Indexer = item.Indexer;
            parseResult.Episodes = _episodeProvider.GetEpisodesByParseResult(parseResult);
            parseResult.SceneSource = true;

            logger.Info("Forcing Download of: {0}", item.ReportTitle);
            _downloadProvider.DownloadReport(parseResult);
        }

        public virtual void Cleanup()
        {
            var ids = _database.Fetch<int>("SELECT Id FROM SearchHistory WHERE SearchTime < @0", DateTime.Now.AddDays(-7));

            if (ids.Any())
            {
                logger.Trace("Deleting old search items");
                _database.Execute("DELETE FROM SearchHistoryItems WHERE SearchHistoryId IN (@0)", ids);

                logger.Trace("Deleting old search results");
                _database.Execute("DELETE FROM SearchHistory WHERE Id IN (@0)", ids);
            }
        }
    }
}
