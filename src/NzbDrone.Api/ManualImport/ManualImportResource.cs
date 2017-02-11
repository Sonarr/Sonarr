using System.Collections.Generic;
using System.Linq;
using NzbDrone.Api.Episodes;
using Sonarr.Http.REST;
using NzbDrone.Api.Series;
using NzbDrone.Common.Crypto;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Api.ManualImport
{
    public class ManualImportResource : RestResource
    {
        public string Path { get; set; }
        public string RelativePath { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public SeriesResource Series { get; set; }
        public int? SeasonNumber { get; set; }
        public List<EpisodeResource> Episodes { get; set; }
        public QualityModel Quality { get; set; }
        public int QualityWeight { get; set; }
        public string DownloadId { get; set; }
        public IEnumerable<Rejection> Rejections { get; set; }
    }

    public static class ManualImportResourceMapper
    {
        public static ManualImportResource ToResource(this Core.MediaFiles.EpisodeImport.Manual.ManualImportItem model)
        {
            if (model == null) return null;

            return new ManualImportResource
            {
                Id = HashConverter.GetHashInt31(model.Path),

                Path = model.Path,
                RelativePath = model.RelativePath,
                Name = model.Name,
                Size = model.Size,
                Series = model.Series.ToResource(),
                SeasonNumber = model.SeasonNumber,
                Episodes = model.Episodes.ToResource(),
                Quality = model.Quality,
                //QualityWeight
                DownloadId = model.DownloadId,
                Rejections = model.Rejections
            };
        }

        public static List<ManualImportResource> ToResource(this IEnumerable<Core.MediaFiles.EpisodeImport.Manual.ManualImportItem> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
