using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers
{
    public interface IFetchAndParseRss
    {
        List<ReportInfo> Fetch();
    }

    public class FetchAndParseRssService : IFetchAndParseRss
    {
        private readonly IIndexerService _indexerService;
        private readonly IFetchFeedFromIndexers _feedFetcher;
        private readonly Logger _logger;

        public FetchAndParseRssService(IIndexerService indexerService, IFetchFeedFromIndexers feedFetcher, Logger logger)
        {
            _indexerService = indexerService;
            _feedFetcher = feedFetcher;
            _logger = logger;
        }

        public List<ReportInfo> Fetch()
        {
            var result = new List<ReportInfo>();

            var indexers = _indexerService.GetAvailableIndexers();

            if (!indexers.Any())
            {
                _logger.Warn("No available indexers. check your configuration.");
                return result;
            }

            _logger.Debug("Available indexers {0}", indexers.Count);

            Parallel.ForEach(indexers, indexer =>
                {
                    var indexerFeed = _feedFetcher.FetchRss(indexer);

                    lock (result)
                    {
                        result.AddRange(indexerFeed);
                    }
                });

            _logger.Debug("Found {0} reports", result.Count);

            return result;
        }
    }
}