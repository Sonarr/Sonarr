using System.Linq;
using NLog;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class CustomStartDateSpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public CustomStartDateSpecification(Logger logger)
        {
            _logger = logger;
        }

        public string RejectionReason
        {
            get
            {
                return "Aired before configured cut-off";
            }
        }


        public virtual bool IsSatisfiedBy(IndexerParseResult subject)
        {
            if (!subject.Series.CustomStartDate.HasValue)
            {
                _logger.Debug("{0} does not restrict downloads before date.", subject.Series.Title);
                return true;
            }

            if (subject.Episodes.Any(episode => episode.AirDate >= subject.Series.CustomStartDate.Value))
            {
                _logger.Debug("One or more episodes aired after cutoff, downloading.");
                return true;
            }

            _logger.Debug("Episodes aired before cutoff date: {0}", subject.Series.CustomStartDate);
            return false;
        }
    }
}
