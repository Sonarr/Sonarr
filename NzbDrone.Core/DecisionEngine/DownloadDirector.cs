using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.DecisionEngine
{
    public interface IDownloadDirector
    {
        DownloadDecision GetDownloadDecision(EpisodeParseResult episodeParseResult);
    }

    public class DownloadDirector : IDownloadDirector
    {
        private readonly IEnumerable<IFetchableSpecification> _specifications;

        public DownloadDirector(IEnumerable<IFetchableSpecification> specifications)
        {
            _specifications = specifications;
        }

        public DownloadDecision GetDownloadDecision(EpisodeParseResult episodeParseResult)
        {
            var rejections = _specifications
                .Where(spec => !spec.IsSatisfiedBy(episodeParseResult))
                .Select(spec => spec.RejectionReason).ToArray();

            episodeParseResult.Decision = new DownloadDecision(rejections);

            return episodeParseResult.Decision;
        }
    }
}