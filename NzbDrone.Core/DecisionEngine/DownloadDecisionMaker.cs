using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.DecisionEngine.Specifications.Search;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine
{
    public interface IMakeDownloadDecision
    {
        List<DownloadDecision> GetRssDecision(IEnumerable<ReportInfo> reports);
        List<DownloadDecision> GetSearchDecision(IEnumerable<ReportInfo> reports, SearchDefinitionBase searchDefinitionBase);
    }

    public class DownloadDecisionMaker : IMakeDownloadDecision
    {
        private readonly IEnumerable<IRejectWithReason> _specifications;
        private readonly IParsingService _parsingService;
        private readonly Logger _logger;

        public DownloadDecisionMaker(IEnumerable<IRejectWithReason> specifications, IParsingService parsingService, Logger logger)
        {
            _specifications = specifications;
            _parsingService = parsingService;
            _logger = logger;
        }

        public List<DownloadDecision> GetRssDecision(IEnumerable<ReportInfo> reports)
        {
            return GetDecisions(reports).ToList();
        }

        public List<DownloadDecision> GetSearchDecision(IEnumerable<ReportInfo> reports, SearchDefinitionBase searchDefinitionBase)
        {
            return GetDecisions(reports).ToList();
        }

        private IEnumerable<DownloadDecision> GetDecisions(IEnumerable<ReportInfo> reports, SearchDefinitionBase searchDefinition = null)
        {
            foreach (var report in reports)
            {
                var parsedEpisodeInfo = Parser.Parser.ParseTitle(report.Title);

                if (parsedEpisodeInfo != null)
                {
                    var remoteEpisode = _parsingService.Map(parsedEpisodeInfo);
                    remoteEpisode.Report = report;

                    if (remoteEpisode.Series != null)
                    {
                        yield return GetDecisionForReport(remoteEpisode, searchDefinition);
                    }
                    else
                    {
                        yield return new DownloadDecision(remoteEpisode, "Unknown Series");
                    }
                }
            }
        }

        private DownloadDecision GetDecisionForReport(RemoteEpisode remoteEpisode, SearchDefinitionBase searchDefinition = null)
        {
            var reasons = _specifications.Select(c => EvaluateSpec(c, remoteEpisode, searchDefinition))
                .Where(c => !string.IsNullOrWhiteSpace(c));

            return new DownloadDecision(remoteEpisode, reasons.ToArray());
        }

        private string EvaluateSpec(IRejectWithReason spec, RemoteEpisode remoteEpisode, SearchDefinitionBase searchDefinitionBase = null)
        {
            try
            {
                var searchSpecification = spec as IDecisionEngineSearchSpecification;
                if (searchSpecification != null && searchDefinitionBase != null)
                {
                    if (!searchSpecification.IsSatisfiedBy(remoteEpisode, searchDefinitionBase))
                    {
                        return spec.RejectionReason;
                    }
                }

                var generalSpecification = spec as IDecisionEngineSpecification;
                if (generalSpecification != null && !generalSpecification.IsSatisfiedBy(remoteEpisode))
                {
                    return spec.RejectionReason;
                }
            }
            catch (Exception e)
            {
                _logger.ErrorException("Couldn't evaluate decision", e);
                return string.Format("{0}: {1}", spec.GetType().Name, e.Message);
            }

            return null;
        }
    }
}