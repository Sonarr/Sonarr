using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Tv;
using Sonarr.Api.V3.EpisodeFiles;
using Sonarr.Api.V3.Series;
using Sonarr.Http.REST;
using Swashbuckle.AspNetCore.Annotations;

namespace Sonarr.Api.V3.Episodes
{
    public class EpisodeResource : RestResource
    {
        public int SeriesId { get; set; }
        public int TvdbId { get; set; }
        public int EpisodeFileId { get; set; }
        public int SeasonNumber { get; set; }
        public int EpisodeNumber { get; set; }
        public string Title { get; set; }
        public string AirDate { get; set; }
        public DateTime? AirDateUtc { get; set; }
        public DateTime? LastSearchTime { get; set; }
        public int Runtime { get; set; }
        public string FinaleType { get; set; }
        public string Overview { get; set; }
        public EpisodeFileResource EpisodeFile { get; set; }
        public bool HasFile { get; set; }
        public bool Monitored { get; set; }
        public int? AbsoluteEpisodeNumber { get; set; }
        public int? SceneAbsoluteEpisodeNumber { get; set; }
        public int? SceneEpisodeNumber { get; set; }
        public int? SceneSeasonNumber { get; set; }
        public bool UnverifiedSceneNumbering { get; set; }
        public SeriesResource Series { get; set; }
        public List<MediaCover> Images { get; set; }

        // Hiding this so people don't think its usable (only used to set the initial state)
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        [SwaggerIgnore]
        public bool Grabbed { get; set; }
    }

    public static class EpisodeResourceMapper
    {
        public static EpisodeResource ToResource(this Episode model)
        {
            if (model == null)
            {
                return null;
            }

            return new EpisodeResource
            {
                Id = model.Id,

                SeriesId = model.SeriesId,
                TvdbId = model.TvdbId,
                EpisodeFileId = model.EpisodeFileId,
                SeasonNumber = model.SeasonNumber,
                EpisodeNumber = model.EpisodeNumber,
                Title = model.Title,
                AirDate = model.AirDate,
                AirDateUtc = model.AirDateUtc,
                Runtime = model.Runtime,
                FinaleType = model.FinaleType,
                Overview = model.Overview,
                LastSearchTime = model.LastSearchTime,

                // EpisodeFile

                HasFile = model.HasFile,
                Monitored = model.Monitored,
                AbsoluteEpisodeNumber = model.AbsoluteEpisodeNumber,
                SceneAbsoluteEpisodeNumber = model.SceneAbsoluteEpisodeNumber,
                SceneEpisodeNumber = model.SceneEpisodeNumber,
                SceneSeasonNumber = model.SceneSeasonNumber,
                UnverifiedSceneNumbering = model.UnverifiedSceneNumbering,

                // Series = model.Series.MapToResource(),
            };
        }

        public static List<EpisodeResource> ToResource(this IEnumerable<Episode> models)
        {
            if (models == null)
            {
                return null;
            }

            return models.Select(ToResource).ToList();
        }
    }
}
