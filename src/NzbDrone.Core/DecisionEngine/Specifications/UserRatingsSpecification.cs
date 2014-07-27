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

        public Boolean IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            var userRatings = subject.Release.UserRatings;

            if (userRatings == null)
            {
                _logger.Debug("Indexer doesn't supply user ratings, skipping check.");
                return true;
            }

            if (userRatings.IsSpamConfirmed)
            {
                _logger.Debug("Release confirmed as spam.");
                return false;
            }

            if (userRatings.IsPasswordedConfirmed)
            {
                _logger.Debug("Release confirmed as passworded.");
                return false;
            }

            if (userRatings.UpVotes.HasValue && userRatings.DownVotes.HasValue)
            {
                var numberOfVotes = userRatings.UpVotes + userRatings.DownVotes;
                var downVotePercentage = userRatings.DownVotes / (Double)numberOfVotes;

                if (numberOfVotes > 5 && downVotePercentage >= 2 / 3.0)
                {
                    _logger.Debug("Release received {0} downvotes with only {1} upvotes. ({2:00}%)", userRatings.DownVotes, userRatings.UpVotes, downVotePercentage * 100);
                    return false;
                }
            }

            return true;
        }
    }
}
