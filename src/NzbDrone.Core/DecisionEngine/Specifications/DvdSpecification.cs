using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class DvdSpecification : IDecisionEngineSpecification
    {
        private static readonly string[] _dvdContainerTypes = new[] { "vob", "iso" };

        private readonly Logger _logger;

        public DvdSpecification(Logger logger)
        {
            _logger = logger;
        }

        public RejectionType Type { get { return RejectionType.Permanent; } }

        public virtual Decision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            if (subject.Release == null || subject.Release.Container.IsNullOrWhiteSpace())
            {
                return Decision.Accept();
            }

            if (_dvdContainerTypes.Contains(subject.Release.Container.ToLower()))
            {
                _logger.Debug("Release contains raw DVD, rejecting.");
                return Decision.Reject("Raw DVD release");
            }

            return Decision.Accept();
        }
    }
}
