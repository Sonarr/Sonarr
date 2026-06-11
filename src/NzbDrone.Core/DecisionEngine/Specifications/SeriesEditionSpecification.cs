using NLog;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class SeriesEditionSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public SeriesEditionSpecification(Logger logger)
        {
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public DownloadSpecDecision IsSatisfiedBy(RemoteEpisode subject, ReleaseDecisionInformation information)
        {
            if (subject.Series == null || subject.Release == null)
            {
                return DownloadSpecDecision.Accept();
            }

            if (SeriesEditions.HasRequiredMarker(subject.Series, subject.Release.Title))
            {
                return DownloadSpecDecision.Accept();
            }

            _logger.Debug("Release {0} does not contain a required marker for series edition {1}", subject.Release.Title, subject.Series.SeriesEdition);

            return DownloadSpecDecision.Reject(DownloadRejectionReason.WrongSeriesEdition, "Release does not contain a required series edition marker");
        }
    }
}
