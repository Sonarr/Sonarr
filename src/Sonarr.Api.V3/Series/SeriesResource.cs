using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Tv;
using Sonarr.Http.REST;

namespace Sonarr.Api.V3.Series
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

        // V3: replace with Ended
        public SeriesStatusType Status { get; set; }

        public bool Ended => Status == SeriesStatusType.Ended;

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
        public int QualityProfileId { get; set; }
        public int LanguageProfileId { get; set; }

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

        public SeriesStatisticsResource Statistics { get; set; }
    }

    public static class SeriesResourceMapper
    {
        public static SeriesResource ToResource(this NzbDrone.Core.Tv.Series model, bool includeSeasonImages = false)
        {
            if (model == null) return null;

            return new SeriesResource
                   {
                       Id = model.Id,

                       Title = model.Title,
                       //AlternateTitles
                       SortTitle = model.SortTitle,

                       //TotalEpisodeCount
                       //EpisodeCount
                       //EpisodeFileCount
                       //SizeOnDisk
                       Status = model.Status,
                       Overview = model.Overview,
                       //NextAiring
                       //PreviousAiring
                       Network = model.Network,
                       AirTime = model.AirTime,
                       Images = model.Images,

                       Seasons = model.Seasons.ToResource(includeSeasonImages),
                       Year = model.Year,

                       Path = model.Path,
                       QualityProfileId = model.ProfileId,
                       LanguageProfileId = model.LanguageProfileId,

                       SeasonFolder = model.SeasonFolder,
                       Monitored = model.Monitored,

                       UseSceneNumbering = model.UseSceneNumbering,
                       Runtime = model.Runtime,
                       TvdbId = model.TvdbId,
                       TvRageId = model.TvRageId,
                       TvMazeId = model.TvMazeId,
                       FirstAired = model.FirstAired,
                       LastInfoSync = model.LastInfoSync,
                       SeriesType = model.SeriesType,
                       CleanTitle = model.CleanTitle,
                       ImdbId = model.ImdbId,
                       TitleSlug = model.TitleSlug,

                       // Root folder path needs to be calculated from the series path
                       // RootFolderPath = model.RootFolderPath,

                       Certification = model.Certification,
                       Genres = model.Genres,
                       Tags = model.Tags,
                       Added = model.Added,
                       AddOptions = model.AddOptions,
                       Ratings = model.Ratings
                   };
        }

        public static NzbDrone.Core.Tv.Series ToModel(this SeriesResource resource)
        {
            if (resource == null) return null;

            return new NzbDrone.Core.Tv.Series
                   {
                       Id = resource.Id,

                       Title = resource.Title,
                       //AlternateTitles
                       SortTitle = resource.SortTitle,

                       //TotalEpisodeCount
                       //EpisodeCount
                       //EpisodeFileCount
                       //SizeOnDisk
                       Status = resource.Status,
                       Overview = resource.Overview,
                       //NextAiring
                       //PreviousAiring
                       Network = resource.Network,
                       AirTime = resource.AirTime,
                       Images = resource.Images,

                       Seasons = resource.Seasons.ToModel(),
                       Year = resource.Year,

                       Path = resource.Path,
                       ProfileId = resource.QualityProfileId,
                       LanguageProfileId = resource.LanguageProfileId,

                       SeasonFolder = resource.SeasonFolder,
                       Monitored = resource.Monitored,

                       UseSceneNumbering = resource.UseSceneNumbering,
                       Runtime = resource.Runtime,
                       TvdbId = resource.TvdbId,
                       TvRageId = resource.TvRageId,
                       TvMazeId = resource.TvMazeId,
                       FirstAired = resource.FirstAired,
                       LastInfoSync = resource.LastInfoSync,
                       SeriesType = resource.SeriesType,
                       CleanTitle = resource.CleanTitle,
                       ImdbId = resource.ImdbId,
                       TitleSlug = resource.TitleSlug,
                       RootFolderPath = resource.RootFolderPath,
                       Certification = resource.Certification,
                       Genres = resource.Genres,
                       Tags = resource.Tags,
                       Added = resource.Added,
                       AddOptions = resource.AddOptions,
                       Ratings = resource.Ratings
                   };
        }

        public static NzbDrone.Core.Tv.Series ToModel(this SeriesResource resource, NzbDrone.Core.Tv.Series series)
        {
            var updatedSeries = resource.ToModel();

            series.ApplyChanges(updatedSeries);

            return series;
        }

        public static List<SeriesResource> ToResource(this IEnumerable<NzbDrone.Core.Tv.Series> series, bool includeSeasonImages = false)
        {
            return series.Select(s => ToResource(s, includeSeasonImages)).ToList();
        }

        public static List<NzbDrone.Core.Tv.Series> ToModel(this IEnumerable<SeriesResource> resources)
        {
            return resources.Select(ToModel).ToList();
        }
    }
}
