using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Download;


namespace NzbDrone.Core.MediaFiles.EpisodeImport
{
    public interface IImportApprovedEpisodes
    {
        List<ImportResult> Import(List<ImportDecision> decisions, bool newDownload, DownloadClientItem historyItem = null);
    }

    public class ImportApprovedEpisodes : IImportApprovedEpisodes
    {
        private readonly IUpgradeMediaFiles _episodeFileUpgrader;
        private readonly IMediaFileService _mediaFileService;
        private readonly IDiskProvider _diskProvider;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public ImportApprovedEpisodes(IUpgradeMediaFiles episodeFileUpgrader,
                                      IMediaFileService mediaFileService,
                                      IDiskProvider diskProvider,
                                      IEventAggregator eventAggregator,
                                      Logger logger)
        {
            _episodeFileUpgrader = episodeFileUpgrader;
            _mediaFileService = mediaFileService;
            _diskProvider = diskProvider;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public List<ImportResult> Import(List<ImportDecision> decisions, bool newDownload, DownloadClientItem historyItem = null)
        {
            var qualifiedImports = decisions.Where(c => c.Approved)
               .GroupBy(c => c.LocalEpisode.Series.Id, (i, s) => s
                   .OrderByDescending(c => c.LocalEpisode.Quality, new QualityModelComparer(s.First().LocalEpisode.Series.Profile))
                   .ThenByDescending(c => c.LocalEpisode.Size))
               .SelectMany(c => c)
               .ToList();

            var importResults = new List<ImportResult>();

            foreach (var importDecision in qualifiedImports.OrderByDescending(e => e.LocalEpisode.Size))
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
                    episodeFile.ReleaseGroup = localEpisode.ParsedEpisodeInfo.ReleaseGroup;

                    if (newDownload)
                    {
                        bool copyOnly = historyItem != null && historyItem.IsReadOnly;
                        episodeFile.SceneName = Path.GetFileNameWithoutExtension(localEpisode.Path.CleanFilePath());
                        var moveResult = _episodeFileUpgrader.UpgradeEpisodeFile(episodeFile, localEpisode, copyOnly);
                        oldFiles = moveResult.OldFiles;
                    }

                    else
                    {
                        episodeFile.RelativePath = localEpisode.Series.Path.GetRelativePath(episodeFile.Path);
                    }

                    _mediaFileService.Add(episodeFile);
                    importResults.Add(new ImportResult(importDecision));

                    if (historyItem != null)
                    {
                        _eventAggregator.PublishEvent(new EpisodeImportedEvent(localEpisode, episodeFile, newDownload, historyItem.DownloadClient, historyItem.DownloadClientId));
                    }
                    else
                    {
                        _eventAggregator.PublishEvent(new EpisodeImportedEvent(localEpisode, episodeFile, newDownload));
                    }

                    if (newDownload)
                    {
                        _eventAggregator.PublishEvent(new EpisodeDownloadedEvent(localEpisode, episodeFile, oldFiles));
                    }
                }
                catch (Exception e)
                {
                    _logger.WarnException("Couldn't import episode " + localEpisode, e);
                    importResults.Add(new ImportResult(importDecision, "Failed to import episode"));
                }
            }

            //Adding all the rejected decisions
            importResults.AddRange(decisions.Where(c => !c.Approved)
                                            .Select(d => new ImportResult(d, d.Rejections.ToArray())));

            return importResults;
        }
    }
}
