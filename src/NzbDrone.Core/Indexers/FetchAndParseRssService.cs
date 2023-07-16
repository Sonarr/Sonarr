using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Core.Parser.Model;
namespace NzbDrone.Core.Indexers
{
    public interface IFetchAndParseRss
    {
        Task<List<ReleaseInfo>> Fetch();
    }

    public class FetchAndParseRssService : IFetchAndParseRss
    {
        private readonly IIndexerFactory _indexerFactory;
        private readonly Logger _logger;

        public FetchAndParseRssService(IIndexerFactory indexerFactory, Logger logger)
        {
            _indexerFactory = indexerFactory;
            _logger = logger;
        }

        public async Task<List<ReleaseInfo>> Fetch()
        {
            var indexers = _indexerFactory.RssEnabled();

            if (!indexers.Any())
            {
                _logger.Warn("No available indexers. check your configuration.");

                return new List<ReleaseInfo>();
            }

            _logger.Debug("Available indexers {0}", indexers.Count);

            var tasks = indexers.Select(FetchIndexer);

            var batch = await Task.WhenAll(tasks);

            var result = batch.SelectMany(x => x).ToList();

            _logger.Debug("Found {0} reports", result.Count);

            return result;
        }

        private async Task<IList<ReleaseInfo>> FetchIndexer(IIndexer indexer)
        {
            try
            {
                return await indexer.FetchRecent();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error during RSS Sync");
            }

            return Array.Empty<ReleaseInfo>();
        }
    }
}
