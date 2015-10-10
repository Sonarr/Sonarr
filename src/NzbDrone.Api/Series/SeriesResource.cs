using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Api.REST;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Tv;

namespace NzbDrone.Api.Series
{
    public class SeriesResource : RestResource
    {
        //Todo: Sorters should be done completely on the client
        //Todo: Is there an easy way to keep IgnoreArticlesWhenSorting in sync between, Series, History, Missing?
        //Todo: We should get the entire Profile instead of ID and Name separately

        //View Only
        public string Title { get; set; }
        public List<AlternateTitleResource> AlternateTitles { get; set; }
        public string SortTitle { get; set; }

        public int SeasonCount
        {
            get
            {
                if (Seasons == null) return 0;

                return Seasons.Where(s => s.SeasonNumber > 0).Count();
            }
        }

        public int? TotalEpisodeCount { get; set; }
        public int? EpisodeCount { get; set; }
        public int? EpisodeFileCount { get; set; }
        public long? SizeOnDisk { get; set; }
        public SeriesStatusType Status { get; set; }
        public string ProfileName { get; set; }
        public string Overview { get; set; }
        public DateTime? NextAiring { get; set; }
        public DateTime? PreviousAiring { get; set; }
        public string Network { get; set; }
        public string AirTime { get; set; }
        public List<MediaCover> Images { get; set; }

        public string RemotePoster { get; set; }
        public List<SeasonResource> Seasons { get; set; }
        public int Year { get; set; }

        //View & Edit
        public string Path { get; set; }
        public int ProfileId { get; set; }

        //Editing Only
        public bool SeasonFolder { get; set; }
        public bool Monitored { get; set; }

        public bool UseSceneNumbering { get; set; }
        public int Runtime { get; set; }
        public int TvdbId { get; set; }
        public int TvRageId { get; set; }
        public int TvMazeId { get; set; }
        public DateTime? FirstAired { get; set; }
        public DateTime? LastInfoSync { get; set; }
        public SeriesTypes SeriesType { get; set; }
        public string CleanTitle { get; set; }
        public string ImdbId { get; set; }
        public string TitleSlug { get; set; }
        public string RootFolderPath { get; set; }
        public string Certification { get; set; }
        public List<string> Genres { get; set; }
        public HashSet<int> Tags { get; set; }
        public DateTime Added { get; set; }
        public AddSeriesOptions AddOptions { get; set; }
        public Ratings Ratings { get; set; }

        //TODO: Add series statistics as a property of the series (instead of individual properties)

        //Used to support legacy consumers
        public int QualityProfileId
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
