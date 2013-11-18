using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;


namespace NzbDrone.Core.MediaFiles.EpisodeImport
{
    public interface IImportApprovedEpisodes
    {
        List<ImportDecision> Import(List<ImportDecision> decisions, bool newDownloads = false);
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

        public List<ImportDecision> Import(List<ImportDecision> decisions, bool newDownload = false)
        {
            var qualifiedImports = GetQualifiedImports(decisions);
            var imported = new List<ImportDecision>();

            foreach (var importDecision in qualifiedImports.OrderByDescending(e => e.LocalEpisode.Size))
            {
                var localEpisode = importDecision.LocalEpisode;
                var oldFiles = new List<EpisodeFile>();

                try
                {
                    if (imported.SelectMany(r => r.LocalEpisode.Episodes)
                                         .Select(e => e.Id)
                                         .ToList()
                                         .Intersect(localEpisode.Episodes.Select(e => e.Id))
                                         .Any())
                    {
                        continue;
                    }

                    var episodeFile = new EpisodeFile();
                    episodeFile.DateAdded = DateTime.UtcNow;
                    episodeFile.SeriesId = localEpisode.Series.Id;
                    episodeFile.Path = localEpisode.Path.CleanFilePath();
                    episodeFile.Size = _diskProvider.GetFileSize(localEpisode.Path);
                    episodeFile.Quality = localEpisode.Quality;
                    episodeFile.SeasonNumber = localEpisode.SeasonNumber;
                    episodeFile.Episodes = localEpisode.Episodes;

                    if (newDownload)
                    {
                        episodeFile.SceneName = Path.GetFileNameWithoutExtension(localEpisode.Path.CleanFilePath());
                        var moveResult = _episodeFileUpgrader.UpgradeEpisodeFile(episodeFile, localEpisode);
                        episodeFile.Path = moveResult.Path;
                        oldFiles = moveResult.OldFiles;
                    }

                    _mediaFileService.Add(episodeFile);
                    imported.Add(importDecision);

                    if (newDownload)
                    {
                        _eventAggregator.PublishEvent(new EpisodeImportedEvent(localEpisode, episodeFile));
                        _eventAggregator.PublishEvent(new EpisodeDownloadedEvent(localEpisode, episodeFile, oldFiles));
                    }
                }
                catch (Exception e)
                {
                    _logger.WarnException("Couldn't import episode " + localEpisode, e);
                }
            }

            return imported;
        }

        private List<ImportDecision> GetQualifiedImports(List<ImportDecision> decisions)
        {
            return decisions.Where(c => c.Approved)
                            .OrderByDescending(c => c.LocalEpisode.Quality)
                            .ThenByDescending(c => c.LocalEpisode.Size)
                            .ToList();
        }
    }
}
