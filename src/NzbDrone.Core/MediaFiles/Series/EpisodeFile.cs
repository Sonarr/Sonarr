using Marr.Data;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Tv;
using System;
using System.Collections.Generic;

namespace NzbDrone.Core.MediaFiles.Series
{
    public class EpisodeFile : MediaModelBase
    {
        // Series
        public Int32 SeriesId { get; set; }
        public Int32 SeasonNumber { get; set; }
        public LazyLoaded<List<Episode>> Episodes { get; set; }
        public LazyLoaded<Tv.Series> Series { get; set; }
    }
}