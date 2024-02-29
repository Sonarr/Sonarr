using System;
using System.Collections.Generic;
using System.IO;
using NzbDrone.Core.CustomFormats;
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
        public int CustomFormatScore { get; set; }
        public int? IndexerFlags { get; set; }
        public int? ReleaseType { get; set; }
        public MediaInfoResource MediaInfo { get; set; }

        public bool QualityCutoffNotMet { get; set; }
    }

    public static class EpisodeFileResourceMapper
    {
        public static EpisodeFileResource ToResource(this EpisodeFile model, NzbDrone.Core.Tv.Series series, IUpgradableSpecification upgradableSpecification, ICustomFormatCalculationService formatCalculationService)
        {
            if (model == null)
            {
                return null;
            }

            model.Series = series;
            var customFormats = formatCalculationService?.ParseCustomFormat(model, model.Series);
            var customFormatScore = series?.QualityProfile?.Value?.CalculateCustomFormatScore(customFormats) ?? 0;

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
                CustomFormats = customFormats.ToResource(false),
                CustomFormatScore = customFormatScore,
                IndexerFlags = (int)model.IndexerFlags,
                ReleaseType = (int)model.ReleaseType,
            };
        }
    }
}
