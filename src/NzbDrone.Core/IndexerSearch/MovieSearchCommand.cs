using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.IndexerSearch
{
    public class MovieSearchCommand : Command
    {
        public int MovieId { get; set; }

        public MovieSearchCommand(int movieId)
        {
            MovieId = movieId;
        }

        public override bool SendUpdatesToClient
        {
            get
            {
                return true;
            }
        }
    }
}