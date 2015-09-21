using NzbDrone.Core.Movies;
namespace NzbDrone.Core.IndexerSearch.Definitions
{
    public class MovieSearchCriteria : MovieSearchCriteriaBase
    {
        public override string ToString()
        {
            return string.Format("[{0}]", Media.Title);
        }
    }
}