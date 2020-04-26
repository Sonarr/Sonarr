using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.MediaFiles.EpisodeImport.Aggregation;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Extras.Others
{
    public class ExistingOtherExtraImporter : ImportExistingExtraFilesBase<OtherExtraFile>
    {
        private readonly IExtraFileService<OtherExtraFile> _otherExtraFileService;
        private readonly IAggregationService _aggregationService;
        private readonly Logger _logger;

        public ExistingOtherExtraImporter(IExtraFileService<OtherExtraFile> otherExtraFileService,
                                          IAggregationService aggregationService,
                                          Logger logger)
            : base(otherExtraFileService)
        {
            _otherExtraFileService = otherExtraFileService;
            _aggregationService = aggregationService;
            _logger = logger;
        }

        public override int Order => 2;

        public override IEnumerable<ExtraFile> ProcessFiles(Series series, List<string> filesOnDisk, List<string> importedFiles)
        {
            _logger.Debug("Looking for existing extra files in {0}", series.Path);

            var extraFiles = new List<OtherExtraFile>();
            var filterResult = FilterAndClean(series, filesOnDisk, importedFiles);

            foreach (var possibleExtraFile in filterResult.FilesOnDisk)
            {
                var extension = Path.GetExtension(possibleExtraFile);

                if (extension.IsNullOrWhiteSpace())
                {
                    _logger.Debug("No extension for file: {0}", possibleExtraFile);
                    continue;
                }

                var localEpisode = new LocalEpisode
                                   {
                                       FileEpisodeInfo = Parser.Parser.ParsePath(possibleExtraFile),
                                       Series = series,
                                       Path = possibleExtraFile
                                   };

                try
                {
                    _aggregationService.Augment(localEpisode, null, false);
                }
                catch (AugmentingFailedException)
                {
                    _logger.Debug("Unable to parse extra file: {0}", possibleExtraFile);
                    continue;
                }

                if (localEpisode.Episodes.Empty())
                {
                    _logger.Debug("Cannot find related episodes for: {0}", possibleExtraFile);
                    continue;
                }

                if (localEpisode.Episodes.DistinctBy(e => e.EpisodeFileId).Count() > 1)
                {
                    _logger.Debug("Extra file: {0} does not match existing files.", possibleExtraFile);
                    continue;
                }

                var extraFile = new OtherExtraFile
                {
                    SeriesId = series.Id,
                    SeasonNumber = localEpisode.SeasonNumber,
                    EpisodeFileId = localEpisode.Episodes.First().EpisodeFileId,
                    RelativePath = series.Path.GetRelativePath(possibleExtraFile),
                    Extension = extension
                };

                extraFiles.Add(extraFile);
            }

            _logger.Info("Found {0} existing other extra files", extraFiles.Count);
            _otherExtraFileService.Upsert(extraFiles);

            // Return files that were just imported along with files that were
            // previously imported so previously imported files aren't imported twice

            return extraFiles.Concat(filterResult.PreviouslyImported);
        }
    }
}
