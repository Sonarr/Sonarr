using System;
using System.Collections.Generic;
using Marr.Data;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Profiles;

namespace NzbDrone.Core.Parser
{
    public abstract class Media : ModelBase
    {
        public string Title { get; set; }
        public string CleanTitle { get; set; }

        public int Year { get; set; }
        public string Overview { get; set; }
        public int Runtime { get; set; }
        public List<MediaCover.MediaCover> Images { get; set; }

        public DateTime? LastInfoSync { get; set; }
        public bool Monitored { get; set; }
        public string RootFolderPath { get; set; }
        public string Path { get; set; }

        public HashSet<Int32> Tags { get; set; }

        public int ProfileId { get; set; }
        public LazyLoaded<Profile> Profile { get; set; }
    }
}
