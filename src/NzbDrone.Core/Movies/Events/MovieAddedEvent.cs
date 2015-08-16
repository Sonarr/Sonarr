using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.Movies.Events
{
    public class MovieAddedEvent : IEvent
    {
        public Movie Movie { get; private set; }

        public MovieAddedEvent(Movie movie)
        {
            Movie = movie;
        }
    }
}
