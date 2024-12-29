using NLog;
using Workarr.Common;
using Workarr.Disk;
using Workarr.Download;
using Workarr.Extensions;
using Workarr.Extras;
using Workarr.History;
using Workarr.MediaFiles.Commands;
using Workarr.MediaFiles.Events;
using Workarr.Messaging.Commands;
using Workarr.Messaging.Events;
using Workarr.Parser.Model;
using Workarr.Qualities;

namespace Workarr.MediaFiles.EpisodeImport
{
    public interface IImportApprovedEpisodes
    {
        List<ImportResult> Import(List<ImportDecision> decisions, bool newDownload, DownloadClientItem downloadClientItem = null, ImportMode importMode = ImportMode.Auto);
    }

    public class ImportApprovedEpisodes : IImportApprovedEpisodes
    {
        private readonly IUpgradeMediaFiles _episodeFileUpgrader;
        private readonly IMediaFileService _mediaFileService;
        private readonly IExtraService _extraService;
        private readonly IExistingExtraFiles _existingExtraFiles;
        private readonly IDiskProvider _diskProvider;
        private readonly IHistoryService _historyService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly Logger _logger;

        public ImportApprovedEpisodes(IUpgradeMediaFiles episodeFileUpgrader,
                                      IMediaFileService mediaFileService,
                                      IExtraService extraService,
                                      IExistingExtraFiles existingExtraFiles,
                                      IDiskProvider diskProvider,
                                      IHistoryService historyService,
                                      IEventAggregator eventAggregator,
                                      IManageCommandQueue commandQueueManager,
                                      Logger logger)
        {
            _episodeFileUpgrader = episodeFileUpgrader;
            _mediaFileService = mediaFileService;
            _extraService = extraService;
            _existingExtraFiles = existingExtraFiles;
            _diskProvider = diskProvider;
            _historyService = historyService;
            _eventAggregator = eventAggregator;
            _commandQueueManager = commandQueueManager;
            _logger = logger;
        }

        public List<ImportResult> Import(List<ImportDecision> decisions, bool newDownload, DownloadClientItem downloadClientItem = null, ImportMode importMode = ImportMode.Auto)
        {
            var qualifiedImports = decisions
                .Where(decision => decision.Approved)
                .GroupBy<ImportDecision, int>(decision => decision.LocalEpisode.Series.Id)
                .SelectMany(group => group
                    .OrderByDescending<ImportDecision, QualityModel>(decision => decision.LocalEpisode.Quality, new QualityModelComparer(group.First().LocalEpisode.Series.QualityProfile))
                    .ThenByDescending<ImportDecision, long>(decision => decision.LocalEpisode.Size))
                .ToList();

            var importResults = new List<ImportResult>();

            foreach (var importDecision in qualifiedImports.OrderBy<ImportDecision, int>(e => e.LocalEpisode.Episodes.Select(episode => episode.EpisodeNumber).MinOrDefault())
                                                           .ThenByDescending<ImportDecision, long>(e => e.LocalEpisode.Size))
            {
                var localEpisode = importDecision.LocalEpisode;
                var oldFiles = new List<DeletedEpisodeFile>();

                try
                {
                    // check if already imported
                    if (importResults.SelectMany(r => r.ImportDecision.LocalEpisode.Episodes)
                                         .Select(e => e.Id)
                                         .Intersect<int>(localEpisode.Episodes.Select(e => e.Id))
                                         .Any())
                    {
                        importResults.Add(new ImportResult(importDecision, "Episode has already been imported"));
                        continue;
                    }

                    var episodeFile = new EpisodeFile();
                    episodeFile.DateAdded = DateTime.UtcNow;
                    episodeFile.SeriesId = localEpisode.Series.Id;
                    episodeFile.Path = PathExtensions.CleanFilePath(localEpisode.Path);
                    episodeFile.Size = _diskProvider.GetFileSize(localEpisode.Path);
                    episodeFile.Quality = localEpisode.Quality;
                    episodeFile.MediaInfo = localEpisode.MediaInfo;
                    episodeFile.Series = localEpisode.Series;
                    episodeFile.SeasonNumber = localEpisode.SeasonNumber;
                    episodeFile.Episodes = localEpisode.Episodes;
                    episodeFile.ReleaseGroup = localEpisode.ReleaseGroup;
                    episodeFile.ReleaseHash = localEpisode.ReleaseHash;
                    episodeFile.Languages = localEpisode.Languages;

                    // Prefer the release type from the download client, folder and finally the file so we have the most accurate information.
                    episodeFile.ReleaseType = localEpisode.DownloadClientEpisodeInfo?.ReleaseType ??
                                              localEpisode.FolderEpisodeInfo?.ReleaseType ??
                                              localEpisode.FileEpisodeInfo.ReleaseType;

                    if (StringExtensions.IsNotNullOrWhiteSpace(downloadClientItem?.DownloadId) == true)
                    {
                        var grabHistory = _historyService.FindByDownloadId(downloadClientItem.DownloadId)
                            .OrderByDescending(h => h.Date)
                            .FirstOrDefault(h => h.EventType == EpisodeHistoryEventType.Grabbed);

                        if (Enum.TryParse(CollectionExtensions.GetValueOrDefault<string, string>(grabHistory?.Data, "indexerFlags"), true, out IndexerFlags flags))
                        {
                            episodeFile.IndexerFlags = flags;
                        }

                        // Prefer the release type from the grabbed history
                        if (Enum.TryParse(CollectionExtensions.GetValueOrDefault<string, string>(grabHistory?.Data, "releaseType"), true, out ReleaseType releaseType))
                        {
                            episodeFile.ReleaseType = releaseType;
                        }
                    }
                    else
                    {
                        episodeFile.IndexerFlags = localEpisode.IndexerFlags;
                        episodeFile.ReleaseType = localEpisode.ReleaseType;
                    }

                    // Fall back to parsed information if history is unavailable or missing
                    if (episodeFile.ReleaseType == ReleaseType.Unknown)
                    {
                        // Prefer the release type from the download client, folder and finally the file so we have the most accurate information.
                        episodeFile.ReleaseType = localEpisode.DownloadClientEpisodeInfo?.ReleaseType ??
                                                  localEpisode.FolderEpisodeInfo?.ReleaseType ??
                                                  localEpisode.FileEpisodeInfo.ReleaseType;
                    }

                    bool copyOnly;
                    switch (importMode)
                    {
                        default:
                        case ImportMode.Auto:
                            copyOnly = downloadClientItem is { CanMoveFiles: false };
                            break;
                        case ImportMode.Move:
                            copyOnly = false;
                            break;
                        case ImportMode.Copy:
                            copyOnly = true;
                            break;
                    }

                    if (newDownload)
                    {
                        episodeFile.SceneName = localEpisode.SceneName;
                        episodeFile.OriginalFilePath = GetOriginalFilePath(downloadClientItem, localEpisode);

                        oldFiles = _episodeFileUpgrader.UpgradeEpisodeFile(episodeFile, localEpisode, copyOnly).OldFiles;
                    }
                    else
                    {
                        episodeFile.RelativePath = PathExtensions.GetRelativePath(localEpisode.Series.Path, episodeFile.Path);

                        // Delete existing files from the DB mapped to this path
                        var previousFiles = _mediaFileService.GetFilesWithRelativePath(localEpisode.Series.Id, episodeFile.RelativePath);

                        foreach (var previousFile in previousFiles)
                        {
                            _mediaFileService.Delete(previousFile, DeleteMediaFileReason.ManualOverride);
                        }
                    }

                    episodeFile = _mediaFileService.Add(episodeFile);
                    importResults.Add(new ImportResult(importDecision, episodeFile));

                    if (newDownload)
                    {
                        if (localEpisode.ScriptImported)
                        {
                            _existingExtraFiles.ImportExtraFiles(localEpisode.Series, localEpisode.PossibleExtraFiles, localEpisode.FileNameBeforeRename);

                            if (localEpisode.FileNameBeforeRename != episodeFile.RelativePath)
                            {
                                _extraService.MoveFilesAfterRename(localEpisode.Series, episodeFile);
                            }
                        }

                        if (!localEpisode.ScriptImported || localEpisode.ShouldImportExtras)
                        {
                            _extraService.ImportEpisode(localEpisode, episodeFile, copyOnly);
                        }
                    }

                    _eventAggregator.PublishEvent(new EpisodeImportedEvent(localEpisode, episodeFile, oldFiles, newDownload, downloadClientItem));
                }
                catch (RootFolderNotFoundException e)
                {
                    _logger.Warn(e, "Couldn't import episode " + localEpisode);
                    _eventAggregator.PublishEvent(new EpisodeImportFailedEvent(e, localEpisode, newDownload, downloadClientItem));

                    importResults.Add(new ImportResult(importDecision, "Failed to import episode, Root folder missing."));
                }
                catch (DestinationAlreadyExistsException e)
                {
                    _logger.Warn(e, "Couldn't import episode " + localEpisode);
                    importResults.Add(new ImportResult(importDecision, "Failed to import episode, Destination already exists."));

                    _commandQueueManager.Push(new RescanSeriesCommand(localEpisode.Series.Id));
                }
                catch (RecycleBinException e)
                {
                    _logger.Warn(e, "Couldn't import episode " + localEpisode);
                    _eventAggregator.PublishEvent(new EpisodeImportFailedEvent(e, localEpisode, newDownload, downloadClientItem));

                    importResults.Add(new ImportResult(importDecision, "Failed to import episode, unable to move existing file to the Recycle Bin."));
                }
                catch (Exception e)
                {
                    _logger.Warn(e, "Couldn't import episode " + localEpisode);
                    importResults.Add(new ImportResult(importDecision, "Failed to import episode"));
                }
            }

            // Adding all the rejected decisions
            importResults.AddRange(decisions.Where(c => !c.Approved)
                                            .Select(d => new ImportResult(d, d.Rejections.Select<ImportRejection, string>(r => r.Message).ToArray())));

            return importResults;
        }

        private string GetOriginalFilePath(DownloadClientItem downloadClientItem, LocalEpisode localEpisode)
        {
            var path = localEpisode.Path;

            if (downloadClientItem != null && !downloadClientItem.OutputPath.IsEmpty)
            {
                var outputDirectory = downloadClientItem.OutputPath.Directory.ToString();

                if (PathExtensions.IsParentPath(outputDirectory, path))
                {
                    return PathExtensions.GetRelativePath(outputDirectory, path);
                }
            }

            var folderEpisodeInfo = localEpisode.FolderEpisodeInfo;

            if (folderEpisodeInfo != null)
            {
                var folderPath = PathExtensions.GetAncestorPath(path, folderEpisodeInfo.ReleaseTitle);

                if (folderPath != null)
                {
                    return folderPath.GetParentPath().GetRelativePath(path);
                }
            }

            var parentPath = PathExtensions.GetParentPath(path);
            var grandparentPath = parentPath.GetParentPath();

            if (grandparentPath != null)
            {
                return grandparentPath.GetRelativePath(path);
            }

            return Path.GetFileName((string)path);
        }
    }
}
