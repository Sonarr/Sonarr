using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Download;
using NzbDrone.Core.Extras;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.MediaFiles.EpisodeImport
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
        private readonly IDiskProvider _diskProvider;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public ImportApprovedEpisodes(IUpgradeMediaFiles episodeFileUpgrader,
                                      IMediaFileService mediaFileService,
                                      IExtraService extraService,
                                      IDiskProvider diskProvider,
                                      IEventAggregator eventAggregator,
                                      Logger logger)
        {
            _episodeFileUpgrader = episodeFileUpgrader;
            _mediaFileService = mediaFileService;
            _extraService = extraService;
            _diskProvider = diskProvider;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public List<ImportResult> Import(List<ImportDecision> decisions, bool newDownload, DownloadClientItem downloadClientItem = null, ImportMode importMode = ImportMode.Auto)
        {
            var qualifiedImports = decisions.Where(c => c.Approved)
               .GroupBy(c => c.LocalEpisode.Series.Id, (i, s) => s
                   .OrderByDescending(c => c.LocalEpisode.Quality, new QualityModelComparer(s.First().LocalEpisode.Series.Profile))
                   .ThenByDescending(c => c.LocalEpisode.Size))
               .SelectMany(c => c)
               .ToList();

            var importResults = new List<ImportResult>();

            foreach (var importDecision in qualifiedImports.OrderBy(e => e.LocalEpisode.Episodes.Select(episode => episode.EpisodeNumber).MinOrDefault())
                                                           .ThenByDescending(e => e.LocalEpisode.Size))
            {
                var localEpisode = importDecision.LocalEpisode;
                var oldFiles = new List<EpisodeFile>();

                try
                {
                    //check if already imported
                    if (importResults.SelectMany(r => r.ImportDecision.LocalEpisode.Episodes)
                                         .Select(e => e.Id)
                                         .Intersect(localEpisode.Episodes.Select(e => e.Id))
                                         .Any())
                    {
                        importResults.Add(new ImportResult(importDecision, "Episode has already been imported"));
                        continue;
                    }

                    var episodeFile = new EpisodeFile();
                    episodeFile.DateAdded = DateTime.UtcNow;
                    episodeFile.SeriesId = localEpisode.Series.Id;
                    episodeFile.Path = localEpisode.Path.CleanFilePath();
                    episodeFile.Size = _diskProvider.GetFileSize(localEpisode.Path);
                    episodeFile.Quality = localEpisode.Quality;
                    episodeFile.MediaInfo = localEpisode.MediaInfo;
                    episodeFile.SeasonNumber = localEpisode.SeasonNumber;
                    episodeFile.Episodes = localEpisode.Episodes;
                    episodeFile.ReleaseGroup = localEpisode.ReleaseGroup;

                    bool copyOnly;
                    switch (importMode)
                    {
                        default:
                        case ImportMode.Auto:
                            copyOnly = downloadClientItem != null && !downloadClientItem.CanMoveFiles;
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
                        episodeFile.SceneName = GetSceneName(downloadClientItem, localEpisode);

                        var moveResult = _episodeFileUpgrader.UpgradeEpisodeFile(episodeFile, localEpisode, copyOnly);
                        oldFiles = moveResult.OldFiles;
                    }
                    else
                    {
                        episodeFile.RelativePath = localEpisode.Series.Path.GetRelativePath(episodeFile.Path);

                        // Delete existing files from the DB mapped to this path
                        var previousFiles = _mediaFileService.GetFilesWithRelativePath(localEpisode.Series.Id, episodeFile.RelativePath);

                        foreach (var previousFile in previousFiles)
                        {
                            _mediaFileService.Delete(previousFile, DeleteMediaFileReason.ManualOverride);
                        }
                    }

                    _mediaFileService.Add(episodeFile);
                    importResults.Add(new ImportResult(importDecision));

                    if (newDownload)
                    {
                        _extraService.ImportEpisode(localEpisode, episodeFile, copyOnly);
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
                }
                catch (Exception e)
                {
                    _logger.Warn(e, "Couldn't import episode " + localEpisode);
                    importResults.Add(new ImportResult(importDecision, "Failed to import episode"));
                }
            }

            //Adding all the rejected decisions
            importResults.AddRange(decisions.Where(c => !c.Approved)
                                            .Select(d => new ImportResult(d, d.Rejections.Select(r => r.Reason).ToArray())));

            return importResults;
        }

        private string GetSceneName(DownloadClientItem downloadClientItem, LocalEpisode localEpisode)
        {
            if (downloadClientItem != null)
            {
                var title = Parser.Parser.RemoveFileExtension(downloadClientItem.Title);

                var parsedTitle = Parser.Parser.ParseTitle(title);

                if (parsedTitle != null && !parsedTitle.FullSeason)
                {
                    return title;
                }
            }

            var fileName = Path.GetFileNameWithoutExtension(localEpisode.Path.CleanFilePath());

            if (SceneChecker.IsSceneTitle(fileName))
            {
                return fileName;
            }

            return null;
        }
    }
}
