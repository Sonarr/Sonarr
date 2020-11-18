using System.Collections.Generic;
using System.Linq;
using Nancy;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles.EpisodeImport.Manual;
using NzbDrone.Core.Qualities;
using Sonarr.Api.V3.Episodes;
using Sonarr.Http;
using Sonarr.Http.Extensions;

namespace Sonarr.Api.V3.ManualImport
{
    public class ManualImportModule : SonarrRestModule<ManualImportResource>
    {
        private readonly IManualImportService _manualImportService;

        public ManualImportModule(IManualImportService manualImportService)
            : base("/manualimport")
        {
            _manualImportService = manualImportService;

            GetResourceAll = GetMediaFiles;
            Post("/",  x => ReprocessItems());
        }

        private List<ManualImportResource> GetMediaFiles()
        {
            var folder = (string)Request.Query.folder;
            var downloadId = (string)Request.Query.downloadId;
            var filterExistingFiles = Request.GetBooleanQueryParameter("filterExistingFiles", true);
            var seriesId = Request.GetNullableIntegerQueryParameter("seriesId", null);

            return _manualImportService.GetMediaFiles(folder, downloadId, seriesId, filterExistingFiles).ToResource().Select(AddQualityWeight).ToList();
        }

        private object ReprocessItems()
        {
            var items = Request.Body.FromJson<List<ManualImportReprocessResource>>();

            foreach (var item in items)
            {
                var processedItem = _manualImportService.ReprocessItem(item.Path, item.DownloadId, item.SeriesId, item.SeasonNumber, item.EpisodeIds ?? new List<int>(), item.Quality, item.Language);

                item.SeasonNumber = processedItem.SeasonNumber;
                item.Episodes = processedItem.Episodes.ToResource();
                item.Rejections = processedItem.Rejections;

                // Only set the language/quality if they're unknown
                if (item.Language == Language.Unknown)
                {
                    item.Language = processedItem.Language;
                }

                if (item.Quality?.Quality == Quality.Unknown)
                {
                    item.Quality = processedItem.Quality;
                }

                // Clear episode IDs in favour of the full episode
                item.EpisodeIds = null;
            }

            return items;
        }

        private ManualImportResource AddQualityWeight(ManualImportResource item)
        {
            if (item.Quality != null)
            {
                item.QualityWeight = Quality.DefaultQualityDefinitions.Single(q => q.Quality == item.Quality.Quality).Weight;
                item.QualityWeight += item.Quality.Revision.Real * 10;
                item.QualityWeight += item.Quality.Revision.Version;
            }
           
            return item;
        }
    }
}
