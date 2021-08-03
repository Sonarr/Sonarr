using System.Collections.Generic;

namespace NzbDrone.Core.MetadataSource.SkyHook.Resource
{
    public class ShowResource
    {
        public ShowResource()
        {
            Actors = new List<ActorResource>();
            Genres = new List<string>();
            Images = new List<ImageResource>();
            Seasons = new List<SeasonResource>();
            Episodes = new List<EpisodeResource>();
        }

        public int TvdbId { get; set; }
        public string Title { get; set; }
        public string Overview { get; set; }

        //public string Language { get; set; }
        public string Slug { get; set; }
        public string FirstAired { get; set; }
        public int? TvRageId { get; set; }
        public int? TvMazeId { get; set; }

        public string Status { get; set; }
        public int? Runtime { get; set; }
        public TimeOfDayResource TimeOfDay { get; set; }

        public string Network { get; set; }
        public string ImdbId { get; set; }

        public List<ActorResource> Actors { get; set; }
        public List<string> Genres { get; set; }

        public string ContentRating { get; set; }

        public RatingResource Rating { get; set; }

        public List<ImageResource> Images { get; set; }
        public List<SeasonResource> Seasons { get; set; }
        public List<EpisodeResource> Episodes { get; set; }
    }
}
