using System;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles
{
    public class EpisodeFile : ModelBase
    {
        public EpisodeFile()
        {

        }

        public EpisodeFile(EpisodeFile source)
        {
            Id = source.Id;
            SeriesId = source.SeriesId;
            SeasonNumber = source.SeasonNumber;
            Path = source.Path;
            Size = source.Size;
        }

        public int SeriesId { get; set; }
        public int SeasonNumber { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }
        public DateTime DateAdded { get; set; }
        public string SceneName { get; set; }

        public QualityModel Quality { get; set; }

        public LazyList<Episode> Episodes { get; set; }
    }
}