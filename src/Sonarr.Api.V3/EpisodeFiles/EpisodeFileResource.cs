using System;
using System.IO;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Qualities;
using Sonarr.Http.REST;

namespace Sonarr.Api.V3.EpisodeFiles
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
        public Language Language { get; set; }
        public QualityModel Quality { get; set; }
        public MediaInfoResource MediaInfo { get; set; }

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
                Language = model.Language,
                Quality = model.Quality,
                MediaInfo = model.MediaInfo.ToResource(model.SceneName)
                //QualityCutoffNotMet
            };

        }

        public static EpisodeFileResource ToResource(this EpisodeFile model, NzbDrone.Core.Tv.Series series, IUpgradableSpecification upgradableSpecification)
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
                Language = model.Language,
                Quality = model.Quality,
                MediaInfo = model.MediaInfo.ToResource(model.SceneName),
                
                QualityCutoffNotMet = upgradableSpecification.CutoffNotMet(series.Profile.Value,
                                                                           series.LanguageProfile.Value,
                                                                           model.Quality,
                                                                           model.Language)
            };
        }
    }
}
