using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.DecisionEngine.Specifications.Search;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.DecisionEngine
{
    public interface IMakeDownloadDecision
    {
        IEnumerable<DownloadDecision> GetRssDecision(IEnumerable<EpisodeParseResult> episodeParseResults);
        IEnumerable<DownloadDecision> GetSearchDecision(IEnumerable<EpisodeParseResult> episodeParseResult, SearchDefinitionBase searchDefinitionBase);
    }

    public class DownloadDecisionMaker : IMakeDownloadDecision
    {
        private readonly IEnumerable<IRejectWithReason> _specifications;

        public DownloadDecisionMaker(IEnumerable<IRejectWithReason> specifications)
        {
            _specifications = specifications;
        }

        public IEnumerable<DownloadDecision> GetRssDecision(IEnumerable<EpisodeParseResult> episodeParseResults)
        {
            foreach (var parseResult in episodeParseResults)
            {
                parseResult.Decision = new DownloadDecision(parseResult, GetGeneralRejectionReasons(parseResult).ToArray());
                yield return parseResult.Decision;
            }

        }

        public IEnumerable<DownloadDecision> GetSearchDecision(IEnumerable<EpisodeParseResult> episodeParseResults, SearchDefinitionBase searchDefinitionBase)
        {
            foreach (var parseResult in episodeParseResults)
            {
                var generalReasons = GetGeneralRejectionReasons(parseResult);
                var searchReasons = GetSearchRejectionReasons(parseResult, searchDefinitionBase);

                parseResult.Decision = new DownloadDecision(parseResult, generalReasons.Union(searchReasons).ToArray());

                yield return parseResult.Decision;
            }
        }


        private IEnumerable<string> GetGeneralRejectionReasons(EpisodeParseResult episodeParseResult)
        {
            return _specifications
                .OfType<IDecisionEngineSpecification>()
                .Where(spec => !spec.IsSatisfiedBy(episodeParseResult))
                .Select(spec => spec.RejectionReason);
        }

        private IEnumerable<string> GetSearchRejectionReasons(EpisodeParseResult episodeParseResult, SearchDefinitionBase searchDefinitionBase)
        {
            return _specifications
                .OfType<IDecisionEngineSearchSpecification>()
                .Where(spec => !spec.IsSatisfiedBy(episodeParseResult, searchDefinitionBase))
                .Select(spec => spec.RejectionReason);
        }
    }
}