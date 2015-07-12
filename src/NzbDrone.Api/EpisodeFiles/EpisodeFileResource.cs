using System;
using System.IO;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.MediaFiles;
using Sonarr.Http.REST;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Languages;

namespace NzbDrone.Api.EpisodeFiles
{
    public class EpisodeFileResource : RestResource
    {
        public int SeriesId { get; set; }
        public int SeasonNumber { get; set; }
        public string RelativePath { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }
        public DateTime DateAdded { get; set; }
        public string SceneName { get; set; }
        public QualityModel Quality { get; set; }
        public Language Language { get; set; }

        public bool QualityCutoffNotMet { get; set; }
    }

    public static class EpisodeFileResourceMapper
    {
        private static EpisodeFileResource ToResource(this EpisodeFile model)
        {
            if (model == null) return null;

            return new EpisodeFileResource
            {
                Id = model.Id,

                SeriesId = model.SeriesId,
                SeasonNumber = model.SeasonNumber,
                RelativePath = model.RelativePath,
                //Path
                Size = model.Size,
                DateAdded = model.DateAdded,
                SceneName = model.SceneName,
                Quality = model.Quality,
                //QualityCutoffNotMet
            };
        }

        public static EpisodeFileResource ToResource(this EpisodeFile model, Core.Tv.Series series, IUpgradableSpecification upgradableSpecification)
        {
            if (model == null) return null;

            return new EpisodeFileResource
            {
                Id = model.Id,

                SeriesId = model.SeriesId,
                SeasonNumber = model.SeasonNumber,
                RelativePath = model.RelativePath,
                Path = Path.Combine(series.Path, model.RelativePath),
                Size = model.Size,
                DateAdded = model.DateAdded,
                SceneName = model.SceneName,
                Quality = model.Quality,
                Language = model.Language,
                QualityCutoffNotMet = upgradableSpecification.CutoffNotMet(series.Profile.Value, series.LanguageProfile.Value, model.Quality, model.Language)
            };
        }
    }
}
