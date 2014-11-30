using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class UserRatingsSpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public UserRatingsSpecification(Logger logger)
        {
            _logger = logger;
        }

        public String RejectionReason
        {
            get { return "Release has too low user ratings"; }
        }

        public RejectionType Type { get { return RejectionType.Permanent; } }

        public Decision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            var userRatings = subject.Release.UserRatings;

            if (userRatings == null)
            {
                _logger.Debug("Indexer doesn't supply user ratings, skipping check.");
                return Decision.Accept();
            }

            if (userRatings.IsSpamConfirmed)
            {
                _logger.Debug("Release confirmed as spam.");
                return Decision.Reject("Release confirmed as spam.");
            }

            if (userRatings.IsPasswordedConfirmed)
            {
                _logger.Debug("Release confirmed as passworded.");
                return Decision.Reject("Release confirmed as passworded.");
            }

            if (userRatings.UpVotes.HasValue && userRatings.DownVotes.HasValue)
            {
                var numberOfVotes = userRatings.UpVotes + userRatings.DownVotes;
                var downVotePercentage = userRatings.DownVotes / (Double)numberOfVotes;

                if (numberOfVotes > 5 && downVotePercentage >= 2 / 3.0)
                {
                    _logger.Debug("Release received {0} downvotes with only {1} upvotes. ({2:00}%)", userRatings.DownVotes, userRatings.UpVotes, downVotePercentage * 100);
                    return Decision.Reject("Release received too much negative votes. ({2:00%})", downVotePercentage * 100);
                }
            }

            return Decision.Accept();
        }
    }
}
