using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.DecisionEngine
{
    public interface IMakeDownloadDecision
    {
        List<DownloadDecision> GetRssDecision(List<ReportInfo> reports);
        List<DownloadDecision> GetSearchDecision(List<ReportInfo> reports, SearchCriteriaBase searchCriteriaBase);
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

        public List<DownloadDecision> GetRssDecision(List<ReportInfo> reports)
        {
            return GetDecisions(reports).ToList();
        }

        public List<DownloadDecision> GetSearchDecision(List<ReportInfo> reports, SearchCriteriaBase searchCriteriaBase)
        {
            return GetDecisions(reports, searchCriteriaBase).ToList();
        }

        private IEnumerable<DownloadDecision> GetDecisions(List<ReportInfo> reports, SearchCriteriaBase searchCriteria = null)
        {
            if (reports.Any())
            {
                _logger.ProgressInfo("Processing {0} reports", reports.Count);
            }

            else
            {
                _logger.ProgressInfo("No reports found");
            }

            var reportNumber = 1;

            foreach (var report in reports)
            {
                DownloadDecision decision = null;
                _logger.ProgressTrace("Processing report {0}/{1}", reportNumber, reports.Count);

                try
                {
                    var parsedEpisodeInfo = Parser.Parser.ParseTitle(report.Title);

                    if (parsedEpisodeInfo != null && !string.IsNullOrWhiteSpace(parsedEpisodeInfo.SeriesTitle))
                    {
                        var remoteEpisode = _parsingService.Map(parsedEpisodeInfo, report.TvRageId);
                        remoteEpisode.Report = report;

                        if (remoteEpisode.Series != null)
                        {
                            decision = GetDecisionForReport(remoteEpisode, searchCriteria);
                        }
                        else
                        {
                            decision = new DownloadDecision(remoteEpisode, "Unknown Series");
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.ErrorException("Couldn't process report.", e);
                }

                reportNumber++;

                if (decision != null)
                {
                    yield return decision;
                }
            }

        }

        private DownloadDecision GetDecisionForReport(RemoteEpisode remoteEpisode, SearchCriteriaBase searchCriteria = null)
        {
            var reasons = _specifications.Select(c => EvaluateSpec(c, remoteEpisode, searchCriteria))
                .Where(c => !string.IsNullOrWhiteSpace(c));

            return new DownloadDecision(remoteEpisode, reasons.ToArray());
        }

        private string EvaluateSpec(IRejectWithReason spec, RemoteEpisode remoteEpisode, SearchCriteriaBase searchCriteriaBase = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(spec.RejectionReason))
                {
                    throw new InvalidOperationException("[Need Rejection Text]");
                }

                var generalSpecification = spec as IDecisionEngineSpecification;
                if (generalSpecification != null && !generalSpecification.IsSatisfiedBy(remoteEpisode, searchCriteriaBase))
                {
                    return spec.RejectionReason;
                }
            }
            catch (Exception e)
            {
                e.Data.Add("report", remoteEpisode.Report.ToJson());
                e.Data.Add("parsed", remoteEpisode.ParsedEpisodeInfo.ToJson());
                _logger.ErrorException("Couldn't evaluate decision on " + remoteEpisode.Report.Title, e);
                return string.Format("{0}: {1}", spec.GetType().Name, e.Message);
            }

            return null;
        }
    }
}