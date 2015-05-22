using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.MediaFiles.EpisodeImport.Manual;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Api.ManualImport
{
    public class ManualImportModule : NzbDroneRestModule<ManualImportResource>
    {
        private readonly IManualImportService _manualImportService;

        public ManualImportModule(IManualImportService manualImportService)
            : base("/manualimport")
        {
            _manualImportService = manualImportService;

            GetResourceAll = GetMediaFiles;
        }

        private List<ManualImportResource> GetMediaFiles()
        {
            var folderQuery = Request.Query.folder;
            var folder = (string)folderQuery.Value;

            var downloadIdQuery = Request.Query.downloadId;
            var downloadId = (string)downloadIdQuery.Value;

            return ToListResource(_manualImportService.GetMediaFiles(folder, downloadId)).Select(AddQualityWeight).ToList();
        }

        private ManualImportResource AddQualityWeight(ManualImportResource item)
        {
            item.QualityWeight = Quality.DefaultQualityDefinitions.Single(q => q.Quality == item.Quality.Quality).Weight;
            item.QualityWeight += item.Quality.Revision.Real * 10;
            item.QualityWeight += item.Quality.Revision.Version;

            return item;
        }
    }
}