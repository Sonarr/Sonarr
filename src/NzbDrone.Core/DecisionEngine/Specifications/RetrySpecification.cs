using System;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.History;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class RetrySpecification : IDecisionEngineSpecification
    {
        private readonly IHistoryService _historyService;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public RetrySpecification(IHistoryService historyService, IConfigService configService, Logger logger)
        {
            _historyService = historyService;
            _configService = configService;
            _logger = logger;
        }

        public string RejectionReason
        {
            get
            {
                return "Release has been retried too many times";
            }
        }

        public RejectionType Type { get { return RejectionType.Permanent; } }

        public bool IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            if (!_configService.EnableFailedDownloadHandling)
            {
                _logger.Debug("Failed Download Handling is not enabled");
                return true;
            }

            var history = _historyService.FindBySourceTitle(subject.Release.Title);

            if (history.Count(h => h.EventType == HistoryEventType.Grabbed &&
                                   HasSamePublishedDate(h, subject.Release.PublishDate)) >
                                   _configService.BlacklistRetryLimit)
            {
                _logger.Debug("Release has been attempted more times than allowed, rejecting");
                return false;
            }

            return true;
        }

        private bool HasSamePublishedDate(History.History item, DateTime publishedDate)
        {
            DateTime itemsPublishedDate;

            if (!DateTime.TryParse(item.Data.GetValueOrDefault("PublishedDate", null), out itemsPublishedDate)) return true;

            return itemsPublishedDate.AddDays(-2) <= publishedDate && itemsPublishedDate.AddDays(2) >= publishedDate;
        }
    }
}
