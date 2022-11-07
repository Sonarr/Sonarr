using System;
using System.Collections.Generic;
using System.IO;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Qualities;
using Sonarr.Api.V3.CustomFormats;
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
        public string ReleaseGroup { get; set; }
        public List<Language> Languages { get; set; }
        public QualityModel Quality { get; set; }
        public List<CustomFormatResource> CustomFormats { get; set; }
        public MediaInfoResource MediaInfo { get; set; }

        public bool QualityCutoffNotMet { get; set; }
    }

    public static class EpisodeFileResourceMapper
    {
        private static EpisodeFileResource ToResource(this EpisodeFile model)
        {
            if (model == null)
            {
                return null;
            }

            return new EpisodeFileResource
            {
                Id = model.Id,

                SeriesId = model.SeriesId,
                SeasonNumber = model.SeasonNumber,
                RelativePath = model.RelativePath,

                // Path
                Size = model.Size,
                DateAdded = model.DateAdded,
                SceneName = model.SceneName,
                ReleaseGroup = model.ReleaseGroup,
                Languages = model.Languages,
                Quality = model.Quality,
                MediaInfo = model.MediaInfo.ToResource(model.SceneName)

                // QualityCutoffNotMet
            };
        }

        public static EpisodeFileResource ToResource(this EpisodeFile model, NzbDrone.Core.Tv.Series series, IUpgradableSpecification upgradableSpecification)
        {
            if (model == null)
            {
                return null;
            }

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
                ReleaseGroup = model.ReleaseGroup,
                Languages = model.Languages,
                Quality = model.Quality,
                MediaInfo = model.MediaInfo.ToResource(model.SceneName),
                QualityCutoffNotMet = upgradableSpecification.QualityCutoffNotMet(series.QualityProfile.Value, model.Quality),
            };
        }
    }
}
