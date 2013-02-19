using System.Linq;
using System;
using NzbDrone.Core.Repository.Quality;
using PetaPoco;

namespace NzbDrone.Core.Tv
{
    public class EpisodeFile
    {
        public EpisodeFile()
        {
            
        }

        public EpisodeFile(EpisodeFile source)
        {
            EpisodeFileId = source.EpisodeFileId;
            SeriesId = source.SeriesId;
            SeasonNumber = source.SeasonNumber;
            Path = source.Path;
            Quality = source.Quality;
            Proper = source.Proper;
            Size = source.Size;
        }

        public int EpisodeFileId { get; set; }

        public int SeriesId { get; set; }
        public int SeasonNumber { get; set; }
        public string Path { get; set; }
        public QualityTypes Quality { get; set; }
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