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
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.EpisodeImport;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Extras.Subtitles
{
    public class SubtitleService : ExtraFileManager<SubtitleFile>
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IDetectSample _detectSample;
        private readonly ISubtitleFileService _subtitleFileService;
        private readonly IMediaFileAttributeService _mediaFileAttributeService;
        private readonly Logger _logger;

        public SubtitleService(IConfigService configService,
                               IDiskProvider diskProvider,
                               IDiskTransferService diskTransferService,
                               IDetectSample detectSample,
                               ISubtitleFileService subtitleFileService,
                               IMediaFileAttributeService mediaFileAttributeService,
                               Logger logger)
            : base(configService, diskProvider, diskTransferService, logger)
        {
            _diskProvider = diskProvider;
            _detectSample = detectSample;
            _subtitleFileService = subtitleFileService;
            _mediaFileAttributeService = mediaFileAttributeService;
            _logger = logger;
        }

        public override int Order => 1;

        public override IEnumerable<ExtraFile> CreateAfterMediaCoverUpdate(Series series)
        {
            return Enumerable.Empty<SubtitleFile>();
        }

        public override IEnumerable<ExtraFile> CreateAfterSeriesScan(Series series, List<EpisodeFile> episodeFiles)
        {
            return Enumerable.Empty<SubtitleFile>();
        }

        public override IEnumerable<ExtraFile> CreateAfterEpisodeImport(Series series, EpisodeFile episodeFile)
        {
            return Enumerable.Empty<SubtitleFile>();
        }

        public override IEnumerable<ExtraFile> CreateAfterEpisodeFolder(Series series, string seriesFolder, string seasonFolder)
        {
            return Enumerable.Empty<SubtitleFile>();
        }

        public override IEnumerable<ExtraFile> MoveFilesAfterRename(Series series, List<EpisodeFile> episodeFiles)
        {
            var subtitleFiles = _subtitleFileService.GetFilesBySeries(series.Id);

            var movedFiles = new List<SubtitleFile>();

            foreach (var episodeFile in episodeFiles)
            {
                var groupedExtraFilesForEpisodeFile = subtitleFiles.Where(m => m.EpisodeFileId == episodeFile.Id)
                                                            .GroupBy(s => s.AggregateString).ToList();

                foreach (var group in groupedExtraFilesForEpisodeFile)
                {
                    var groupCount = group.Count();
                    var copy = 1;

                    foreach (var subtitleFile in group)
                    {
                        var suffix = GetSuffix(subtitleFile.Language, copy, subtitleFile.LanguageTags, groupCount > 1);
                        movedFiles.AddIfNotNull(MoveFile(series, episodeFile, subtitleFile, suffix));

                        copy++;
                    }
                }
            }

            _subtitleFileService.Upsert(movedFiles);

            return movedFiles;
        }

        public override bool CanImportFile(LocalEpisode localEpisode, EpisodeFile episodeFile, string path, string extension, bool readOnly)
        {
            return SubtitleFileExtensions.Extensions.Contains(extension.ToLowerInvariant());
        }

        public override IEnumerable<ExtraFile> ImportFiles(LocalEpisode localEpisode, EpisodeFile episodeFile, List<string> files, bool isReadOnly)
        {
            var importedFiles = new List<SubtitleFile>();

            var filteredFiles = files.Where(f => CanImportFile(localEpisode, episodeFile, f, Path.GetExtension(f), isReadOnly)).ToList();

            var sourcePath = localEpisode.Path;
            var sourceFolder = _diskProvider.GetParentFolder(sourcePath);
            var sourceFileName = Path.GetFileNameWithoutExtension(sourcePath);

            var matchingFiles = new List<string>();

            foreach (var file in filteredFiles)
            {
                try
                {
                    // Filename match
                    if (Path.GetFileNameWithoutExtension(file).StartsWithIgnoreCase(sourceFileName))
                    {
                        matchingFiles.Add(file);
                        continue;
                    }

                    // Season and episode match
                    var fileEpisodeInfo = Parser.Parser.ParsePath(file) ?? new ParsedEpisodeInfo();

                    if (fileEpisodeInfo.EpisodeNumbers.Length == 0)
                    {
                        continue;
                    }

                    if (fileEpisodeInfo.SeasonNumber == localEpisode.FileEpisodeInfo.SeasonNumber &&
                        fileEpisodeInfo.EpisodeNumbers.SequenceEqual(localEpisode.FileEpisodeInfo.EpisodeNumbers))
                    {
                        matchingFiles.Add(file);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Failed to import subtitle file: {0}", file);
                }
            }

            // Use any sub if only episode in folder
            if (matchingFiles.Count == 0 && filteredFiles.Count > 0)
            {
                var videoFiles = _diskProvider.GetFiles(sourceFolder, SearchOption.AllDirectories)
                                              .Where(file => MediaFileExtensions.Extensions.Contains(Path.GetExtension(file)))
                                              .ToList();

                if (videoFiles.Count > 2)
                {
                    return importedFiles;
                }

                // Filter out samples
                videoFiles = videoFiles.Where(file =>
                {
                    var sample = _detectSample.IsSample(localEpisode.Series, file, false);

                    if (sample == DetectSampleResult.Sample)
                    {
                        return false;
                    }

                    return true;
                }).ToList();

                if (videoFiles.Count == 1)
                {
                    matchingFiles.AddRange(filteredFiles);

                    _logger.Warn("Imported any available subtitle file for episode: {0}", localEpisode);
                }
            }

            var subtitleFiles = new List<SubtitleFile>();

            foreach (string file in matchingFiles)
            {
                var language = LanguageParser.ParseSubtitleLanguage(file);
                var extension = Path.GetExtension(file);
                var languageTags = LanguageParser.ParseLanguageTags(file);
                var subFile = new SubtitleFile
                {
                    Language = language,
                    Extension = extension
                };
                subFile.LanguageTags = languageTags.ToList();
                subFile.RelativePath = PathExtensions.GetRelativePath(sourceFolder, file);
                subtitleFiles.Add(subFile);
            }

            var groupedSubtitleFiles = subtitleFiles.GroupBy(s => s.AggregateString).ToList();

            foreach (var group in groupedSubtitleFiles)
            {
                var groupCount = group.Count();
                var copy = 1;

                foreach (var file in group)
                {
                    var path = Path.Combine(sourceFolder, file.RelativePath);
                    var language = file.Language;
                    var extension = file.Extension;
                    var suffix = GetSuffix(language, copy, file.LanguageTags, groupCount > 1);
                    try
                    {
                        var subtitleFile = ImportFile(localEpisode.Series, episodeFile, path, isReadOnly, extension, suffix);
                        subtitleFile.Language = language;
                        subtitleFile.LanguageTags = file.LanguageTags;

                        _mediaFileAttributeService.SetFilePermissions(path);
                        _subtitleFileService.Upsert(subtitleFile);

                        importedFiles.Add(subtitleFile);

                        copy++;
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn(ex, "Failed to import subtitle file: {0}", path);
                    }
                }
            }

            return importedFiles;
        }

        private string GetSuffix(Language language, int copy, List<string> languageTags, bool multipleCopies = false)
        {
            var suffixBuilder = new StringBuilder();

            if (multipleCopies)
            {
                suffixBuilder.Append('.');
                suffixBuilder.Append(copy);
            }

            if (language != Language.Unknown)
            {
                suffixBuilder.Append('.');
                suffixBuilder.Append(IsoLanguages.Get(language).TwoLetterCode);
            }

            if (languageTags.Any())
            {
                suffixBuilder.Append('.');
                suffixBuilder.Append(string.Join(".", languageTags));
            }

            return suffixBuilder.ToString();
        }
    }
}
