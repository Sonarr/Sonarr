using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications;

public class ExternalIdsSpecification : IDownloadDecisionEngineSpecification
{
    private readonly Logger _logger;

    public SpecificationPriority Priority => SpecificationPriority.Default;
    public RejectionType Type => RejectionType.Permanent;

    public ExternalIdsSpecification(Logger logger)
    {
        _logger = logger;
    }

    public DownloadSpecDecision IsSatisfiedBy(RemoteEpisode subject, ReleaseDecisionInformation information)
    {
        if (subject.Release.TvdbId != 0 && subject.Release.TvdbId != subject.Series.TvdbId)
        {
            _logger.Debug("Wrong series. TVDb Id {0} wanted, but found {1}.", subject.Series.TvdbId, subject.Release.TvdbId);
            return DownloadSpecDecision.Reject(DownloadRejectionReason.WrongSeries, "Wrong series. TVDb Id {0} wanted, but found {1}.", subject.Series.TvdbId, subject.Release.TvdbId);
        }

        if (subject.Release.ImdbId.IsNotNullOrWhiteSpace() && subject.Release.ImdbId != "0" && subject.Release.ImdbId != subject.Series.ImdbId)
        {
            _logger.Debug("Wrong series. IMDb ID {0} wanted, but found {1}.", subject.Series.ImdbId, subject.Release.ImdbId);
            return DownloadSpecDecision.Reject(DownloadRejectionReason.WrongSeries, "Wrong series. IMDb ID {0} wanted, but found {1}.", subject.Series.ImdbId, subject.Release.ImdbId);
        }

        return DownloadSpecDecision.Accept();
    }
}
