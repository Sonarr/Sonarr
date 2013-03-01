using System.Linq;
using System;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Qualities;
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
            Quality = source.Quality;
            Proper = source.Proper;
            Size = source.Size;
        }

        public int SeriesId { get; set; }
        public int SeasonNumber { get; set; }
        public string Path { get; set; }
        public Quality Quality { get; set; }
        public bool Proper { get; set; }
        public long Size { get; set; }
        public DateTime DateAdded { get; set; }
        public string SceneName { get; set; }
        public string ReleaseGroup { get; set; }

        public QualityModel QualityWrapper
        {
            get
            {
                return new QualityModel(Quality, Proper);
            }
            set
            {
                Quality = value.Quality;
                Proper = value.Proper;
            }
        }
    }
}