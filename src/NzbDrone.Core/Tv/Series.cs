using System;
using System.Collections.Generic;
using Marr.Data;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Profiles.Languages;

namespace NzbDrone.Core.Tv
{
    public class Series : ModelBase
    {
        public Series()
        {
            Images = new List<MediaCover.MediaCover>();
            Genres = new List<string>();
            Actors = new List<Actor>();
            Seasons = new List<Season>();
            Tags = new HashSet<int>();
        }

        public int TvdbId { get; set; }
        public int TvRageId { get; set; }
        public int TvMazeId { get; set; }
        public string ImdbId { get; set; }
        public string Title { get; set; }
        public string CleanTitle { get; set; }
        public string SortTitle { get; set; }
        public SeriesStatusType Status { get; set; }
        public string Overview { get; set; }
        public string AirTime { get; set; }
        public bool Monitored { get; set; }
        public int ProfileId { get; set; }
        public int LanguageProfileId { get; set; }
        public bool SeasonFolder { get; set; }
        public DateTime? LastInfoSync { get; set; }
        public int Runtime { get; set; }
        public List<MediaCover.MediaCover> Images { get; set; }
        public SeriesTypes SeriesType { get; set; }
        public string Network { get; set; }
        public bool UseSceneNumbering { get; set; }
        public string TitleSlug { get; set; }
        public string Path { get; set; }
        public int Year { get; set; }
        public Ratings Ratings { get; set; }
        public List<string> Genres { get; set; }
        public List<Actor> Actors { get; set; }
        public string Certification { get; set; }
        public string RootFolderPath { get; set; }
        public DateTime Added { get; set; }
        public DateTime? FirstAired { get; set; }
        public LazyLoaded<Profile> Profile { get; set; }
        public LazyLoaded<LanguageProfile> LanguageProfile { get; set; }

        public List<Season> Seasons { get; set; }
        public HashSet<int> Tags { get; set; }
        public AddSeriesOptions AddOptions { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}][{1}]", TvdbId, Title.NullSafe());
        }

        public void ApplyChanges(Series otherSeries)
        {
            TvdbId = otherSeries.TvdbId;

            Seasons = otherSeries.Seasons;
            Path = otherSeries.Path;
            ProfileId = otherSeries.ProfileId;
            LanguageProfileId = otherSeries.LanguageProfileId;

            SeasonFolder = otherSeries.SeasonFolder;
            Monitored = otherSeries.Monitored;

            SeriesType = otherSeries.SeriesType;
            RootFolderPath = otherSeries.RootFolderPath;
            Tags = otherSeries.Tags;
            AddOptions = otherSeries.AddOptions;
        }
    }
}