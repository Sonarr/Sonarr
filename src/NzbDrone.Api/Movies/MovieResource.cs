using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Api.REST;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Movies;

namespace NzbDrone.Api.Movies
{
    public class MoviesResource : RestResource
    {
        public String ImdbId { get; set; }
        public int TmdbId { get; set; }
        
        public string Title { get; set; }
        public String CleanTitle { get; set; }
        public String OriginalTitle { get; set; }

        public int Year { get; set; }
        public string Overview { get; set; }
        public Int32 Runtime { get; set; }
        public string TagLine { get; set; }
        public DateTime ReleaseDate { get; set; }
        public List<MediaCover> Images { get; set; }
        public String RemotePoster { get; set; }

        public DateTime? LastInfoSync { get; set; }

        public Boolean Monitored { get; set; }
        public HashSet<Int32> Tags { get; set; }
        public int ProfileId { get; set; }
        public String RootFolderPath { get; set; }
        public string Path { get; set; }
        public bool AddOptions { get; set; }
        public int MovieFileId { get; set; }

        public Int32 QualityProfileId
        {
            get
            {
                return ProfileId;
            }
            set
            {
                if (value > 0 && ProfileId == 0)
                {
                    ProfileId = value;
                }
            }
        }
    }
}
