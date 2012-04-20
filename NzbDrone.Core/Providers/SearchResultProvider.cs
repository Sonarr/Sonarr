using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using Ninject;
using NzbDrone.Core.Repository.Search;
using PetaPoco;

namespace NzbDrone.Core.Providers
{
    public class SearchResultProvider
    {
        private readonly IDatabase _database;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public SearchResultProvider(IDatabase database)
        {
            _database = database;
        }

        public SearchResultProvider()
        {
            
        }

        public virtual void Add(SearchResult searchResult)
        {
            logger.Trace("Adding new search result");
            var id = Convert.ToInt32(_database.Insert(searchResult));

            searchResult.SearchResultItems.ForEach(s => s.Id = id);
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
            return _database.Fetch<SearchResult>();
        }

        public virtual SearchResult GetSearchResult(int id)
        {
            var result = _database.Single<SearchResult>(id);
            result.SearchResultItems = _database.Fetch<SearchResultItem>("WHERE SearchResultId = @0", id);

            return result;
        }
    }
}
