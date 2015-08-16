using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.IndexerSearch
{
    public class MissingMovieSearchCommand : Command
    {
        public int? MovieId { get; private set; }

        public override bool SendUpdatesToClient
        {
            get
            {
                return true;
            }
        }

        public MissingMovieSearchCommand()
        {
        }

        public MissingMovieSearchCommand(int movieId)
        {
            MovieId = movieId;
        }
    }
}