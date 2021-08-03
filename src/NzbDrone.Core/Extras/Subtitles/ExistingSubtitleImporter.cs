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

namespace NzbDrone.Core.Extras.Subtitles
{
    public class ExistingSubtitleImporter : ImportExistingExtraFilesBase<SubtitleFile>
    {
        private readonly IExtraFileService<SubtitleFile> _subtitleFileService;
        private readonly IAggregationService _aggregationService;
        private readonly Logger _logger;

        public ExistingSubtitleImporter(IExtraFileService<SubtitleFile> subtitleFileService,
                                        IAggregationService aggregationService,
                                        Logger logger)
            : base(subtitleFileService)
        {
            _subtitleFileService = subtitleFileService;
            _aggregationService = aggregationService;
            _logger = logger;
        }

        public override int Order => 1;

        public override IEnumerable<ExtraFile> ProcessFiles(Series series, List<string> filesOnDisk, List<string> importedFiles)
        {
            _logger.Debug("Looking for existing subtitle files in {0}", series.Path);

            var subtitleFiles = new List<SubtitleFile>();
            var filterResult = FilterAndClean(series, filesOnDisk, importedFiles);

            foreach (var possibleSubtitleFile in filterResult.FilesOnDisk)
            {
                var extension = Path.GetExtension(possibleSubtitleFile);

                if (SubtitleFileExtensions.Extensions.Contains(extension))
                {
                    var localEpisode = new LocalEpisode
                    {
                        FileEpisodeInfo = Parser.Parser.ParsePath(possibleSubtitleFile),
                        Series = series,
                        Path = possibleSubtitleFile
                    };

                    try
                    {
                        _aggregationService.Augment(localEpisode, null);
                    }
                    catch (AugmentingFailedException)
                    {
                        _logger.Debug("Unable to parse extra file: {0}", possibleSubtitleFile);
                        continue;
                    }

                    if (localEpisode.Episodes.Empty())
                    {
                        _logger.Debug("Cannot find related episodes for: {0}", possibleSubtitleFile);
                        continue;
                    }

                    if (localEpisode.Episodes.DistinctBy(e => e.EpisodeFileId).Count() > 1)
                    {
                        _logger.Debug("Subtitle file: {0} does not match existing files.", possibleSubtitleFile);
                        continue;
                    }

                    var subtitleFile = new SubtitleFile
                                       {
                                           SeriesId = series.Id,
                                           SeasonNumber = localEpisode.SeasonNumber,
                                           EpisodeFileId = localEpisode.Episodes.First().EpisodeFileId,
                                           RelativePath = series.Path.GetRelativePath(possibleSubtitleFile),
                                           Language = LanguageParser.ParseSubtitleLanguage(possibleSubtitleFile),
                                           Extension = extension
                                       };

                    subtitleFiles.Add(subtitleFile);
                }
            }

            _logger.Info("Found {0} existing subtitle files", subtitleFiles.Count);
            _subtitleFileService.Upsert(subtitleFiles);

            // Return files that were just imported along with files that were
            // previously imported so previously imported files aren't imported twice

            return subtitleFiles.Concat(filterResult.PreviouslyImported);
        }
    }
}
