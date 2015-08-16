using System;
using System.Collections.Generic;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Notifications
{
    public class GrabMovieMessage
    {
        public String Message { get; set; }
        public Movie Movie { get; set; }
        public RemoteMovie RemoteMovie { get; set; }
        public QualityModel Quality { get; set; }

        public override string ToString()
        {
            return Message;
        }
    }
}
