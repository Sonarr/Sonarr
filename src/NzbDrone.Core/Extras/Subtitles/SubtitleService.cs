using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Extras.Subtitles
{
    public class SubtitleService : ExtraFileManager<SubtitleFile>
    {
        private readonly ISubtitleFileService _subtitleFileService;
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        public SubtitleService(IConfigService configService,
                               IDiskTransferService diskTransferService,
                               ISubtitleFileService subtitleFileService,
                               IDiskProvider diskProvider,
                               Logger logger)
            : base(configService, diskTransferService, subtitleFileService)
        {
            _subtitleFileService = subtitleFileService;
            _diskProvider = diskProvider;
            _logger = logger;
        }

        public override int Order => 1;

        public override IEnumerable<ExtraFile> CreateAfterSeriesScan(Series series, List<EpisodeFile> episodeFiles)
        {
            return Enumerable.Empty<SubtitleFile>();
        }

        public override IEnumerable<ExtraFile> CreateAfterEpisodeImport(Series series, EpisodeFile episodeFile)
        {
            return Enumerable.Empty<SubtitleFile>();
        }

        public override IEnumerable<ExtraFile> CreateAfterEpisodeImport(Series series, string seriesFolder, string seasonFolder)
        {
            return Enumerable.Empty<SubtitleFile>();
        }

        public override IEnumerable<ExtraFile> MoveFilesAfterRename(Series series, List<EpisodeFile> episodeFiles)
        {
            // TODO: Remove
            // We don't want to move files after rename yet.

            return Enumerable.Empty<ExtraFile>();

            var subtitleFiles = _subtitleFileService.GetFilesBySeries(series.Id);

            var movedFiles = new List<SubtitleFile>();

            foreach (var episodeFile in episodeFiles)
            {
                var groupedExtraFilesForEpisodeFile = subtitleFiles.Where(m => m.EpisodeFileId == episodeFile.Id)
                                                            .GroupBy(s => s.Language + s.Extension).ToList();

                foreach (var group in groupedExtraFilesForEpisodeFile)
                {
                    var groupCount = group.Count();
                    var copy = 1;

                    if (groupCount > 1)
                    {
                        _logger.Warn("Multiple subtitle files found with the same language and extension for {0}", Path.Combine(series.Path, episodeFile.RelativePath));
                    }

                    foreach (var extraFile in group)
                    {
                        var existingFileName = Path.Combine(series.Path, extraFile.RelativePath);
                        var extension = GetExtension(extraFile, existingFileName, copy, groupCount > 1);
                        var newFileName = Path.ChangeExtension(Path.Combine(series.Path, episodeFile.RelativePath), extension);

                        if (newFileName.PathNotEquals(existingFileName))
                        {
                            try
                            {
                                _diskProvider.MoveFile(existingFileName, newFileName);
                                extraFile.RelativePath = series.Path.GetRelativePath(newFileName);
                                movedFiles.Add(extraFile);
                            }
                            catch (Exception ex)
                            {
                                _logger.Warn(ex, "Unable to move subtitle file: {0}", existingFileName);
                            }
                        }

                        copy++;
                    }
                }
            }

            _subtitleFileService.Upsert(movedFiles);

            return movedFiles;
        }

        public override ExtraFile Import(Series series, EpisodeFile episodeFile, string path, string extension, bool readOnly)
        {
            if (SubtitleFileExtensions.Extensions.Contains(Path.GetExtension(path)))
            {
                var subtitleFile = ImportFile(series, episodeFile, path, extension, readOnly);
                subtitleFile.Language = LanguageParser.ParseSubtitleLanguage(path);

                _subtitleFileService.Upsert(subtitleFile);

                return subtitleFile;
            }

            return null;
        }

        private string GetExtension(SubtitleFile extraFile, string existingFileName, int copy, bool multipleCopies = false)
        {
            var fileExtension = Path.GetExtension(existingFileName);
            var extensionBuilder = new StringBuilder();

            if (multipleCopies)
            {
                extensionBuilder.Append(copy);
                extensionBuilder.Append(".");
            }

            if (extraFile.Language != Language.Unknown)
            {
                extensionBuilder.Append(IsoLanguages.Get(extraFile.Language).TwoLetterCode);
                extensionBuilder.Append(".");
            }

            extensionBuilder.Append(fileExtension.TrimStart('.'));

            return extensionBuilder.ToString();
        }
    }
}
