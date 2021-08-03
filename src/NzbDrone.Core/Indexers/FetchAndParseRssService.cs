using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Common.TPL;
using NzbDrone.Core.Parser.Model;
namespace NzbDrone.Core.Indexers
{
    public interface IFetchAndParseRss
    {
        List<ReleaseInfo> Fetch();
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

        public List<ReleaseInfo> Fetch()
        {
            var result = new List<ReleaseInfo>();

            var indexers = _indexerFactory.RssEnabled();

            if (!indexers.Any())
            {
                _logger.Warn("No available indexers. check your configuration.");
                return result;
            }

            _logger.Debug("Available indexers {0}", indexers.Count);

            var taskList = new List<Task>();
            var taskFactory = new TaskFactory(TaskCreationOptions.LongRunning, TaskContinuationOptions.None);

            foreach (var indexer in indexers)
            {
                var indexerLocal = indexer;

                var task = taskFactory.StartNew(() =>
                     {
                         try
                         {
                             var indexerReports = indexerLocal.FetchRecent();

                             lock (result)
                             {
                                 _logger.Debug("Found {0} from {1}", indexerReports.Count, indexer.Name);

                                 result.AddRange(indexerReports);
                             }
                         }
                         catch (Exception e)
                         {
                             _logger.Error(e, "Error during RSS Sync");
                         }
                     }).LogExceptions();

                taskList.Add(task);
            }

            Task.WaitAll(taskList.ToArray());

            _logger.Debug("Found {0} reports", result.Count);

            return result;
        }
    }
}
