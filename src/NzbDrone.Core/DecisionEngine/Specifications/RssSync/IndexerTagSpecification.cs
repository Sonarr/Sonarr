using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications.RssSync
{
    public class IndexerTagSpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;
        private readonly IIndexerRepository _indexerRepository;

        public IndexerTagSpecification(Logger logger, IIndexerRepository indexerRepository)
        {
            _logger = logger;
            _indexerRepository = indexerRepository;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual Decision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            // If indexer has tags, check that at least one of them is present on the series
            var indexerTags = _indexerRepository.Get(subject.Release.IndexerId).Tags;

            if (indexerTags.Any() && indexerTags.Intersect(subject.Series.Tags).Empty())
            {
                _logger.Debug("Indexer {0} has tags. None of these are present on series {1}. Rejecting", subject.Release.Indexer, subject.Series);

                return Decision.Reject("Series tags do not match any of the indexer tags");
            }

            return Decision.Accept();
        }
    }
}
