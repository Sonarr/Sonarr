using System.Collections.Generic;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.MediaFiles.Commands.Movies
{
    public class RenameMovieCommand : Command
    {
        public List<int> MovieIds { get; set; }

        public override bool SendUpdatesToClient
        {
            get
            {
                return true;
            }
        }

        public RenameMovieCommand()
        {
        }
    }
}