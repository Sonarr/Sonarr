using System;
using System.Collections.Generic;
using System.Linq;
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

        public DownloadDecisionMaker(IEnumerable<IRejectWithReason> specifications, IParsingService parsingService)
        {
            _specifications = specifications;
            _parsingService = parsingService;
        }

        public List<DownloadDecision> GetRssDecision(IEnumerable<ReportInfo> reports)
        {
            return GetDecisions(reports, GetGeneralRejectionReasons).ToList();
        }

        public List<DownloadDecision> GetSearchDecision(IEnumerable<ReportInfo> reports, SearchDefinitionBase searchDefinitionBase)
        {
            return GetDecisions(reports, remoteEpisode =>
                {
                    var generalReasons = GetGeneralRejectionReasons(remoteEpisode);
                    var searchReasons = GetSearchRejectionReasons(remoteEpisode, searchDefinitionBase);
                    return generalReasons.Union(searchReasons);
                }).ToList();
        }


        private IEnumerable<string> GetGeneralRejectionReasons(RemoteEpisode report)
        {
            return _specifications
                .OfType<IDecisionEngineSpecification>()
                .Where(spec => !spec.IsSatisfiedBy(report))
                .Select(spec => spec.RejectionReason);
        }

        private IEnumerable<DownloadDecision> GetDecisions(IEnumerable<ReportInfo> reports, Func<RemoteEpisode, IEnumerable<string>> decisionCallback)
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
                        yield return new DownloadDecision(remoteEpisode, decisionCallback(remoteEpisode).ToArray());
                    }
                    else
                    {
                        yield return new DownloadDecision(remoteEpisode, "Unknown Series");
                    }
                }
            }
        }

        private IEnumerable<string> GetSearchRejectionReasons(RemoteEpisode report, SearchDefinitionBase searchDefinitionBase)
        {
            return _specifications
                .OfType<IDecisionEngineSearchSpecification>()
                .Where(spec => !spec.IsSatisfiedBy(report, searchDefinitionBase))
                .Select(spec => spec.RejectionReason);
        }
    }
}