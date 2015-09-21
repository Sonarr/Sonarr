using System;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Movies.Commands
{
    public class MoveMovieCommand : Command
    {
        public Int32 MovieId { get; set; }
        public String SourcePath { get; set; }
        public String DestinationPath { get; set; }
        public String DestinationRootFolder { get; set; }
    }
}
