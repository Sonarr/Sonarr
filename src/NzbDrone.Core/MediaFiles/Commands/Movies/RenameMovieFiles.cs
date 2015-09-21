using System.Collections.Generic;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.MediaFiles.Commands.Movies
{
    public class RenameMovieFilesCommand : Command
    {
        public int MovieId { get; set; }
        public List<int> Files { get; set; }

        public override bool SendUpdatesToClient
        {
            get
            {
                return true;
            }
        }

        public RenameMovieFilesCommand()
        {
        }

        public RenameMovieFilesCommand(int movieId, List<int> files)
        {
            MovieId = movieId;
            Files = files;
        }
    }
}