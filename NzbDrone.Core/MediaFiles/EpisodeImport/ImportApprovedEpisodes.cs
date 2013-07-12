using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.MediaFiles.Events;

namespace NzbDrone.Core.MediaFiles.EpisodeImport
{
    public interface IImportApprovedEpisodes
    {
        List<ImportDecision> Import(List<ImportDecision> decisions, bool newDownloads = false);
    }
    
    public class ImportApprovedEpisodes : IImportApprovedEpisodes
    {
        private readonly IMoveEpisodeFiles _episodeFileMover;
        private readonly IMediaFileService _mediaFileService;
        private readonly IDiskProvider _diskProvider;
        private readonly IMessageAggregator _messageAggregator;
        private readonly Logger _logger;

        public ImportApprovedEpisodes(IMoveEpisodeFiles episodeFileMover,
                                      IMediaFileService mediaFileService,
                                      IDiskProvider diskProvider,
                                      IMessageAggregator messageAggregator,
                                      Logger logger)
        {
            _episodeFileMover = episodeFileMover;
            _mediaFileService = mediaFileService;
            _diskProvider = diskProvider;
            _messageAggregator = messageAggregator;
            _logger = logger;
        }

        public List<ImportDecision> Import(List<ImportDecision> decisions, bool newDownload = false)
        {
            var qualifiedImports = GetQualifiedImports(decisions);
            var imported = new List<ImportDecision>();

            foreach (var importDecision in qualifiedImports)
            {
                var localEpisode = importDecision.LocalEpisode;

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
                    episodeFile.Path = localEpisode.Path.CleanPath();
                    episodeFile.Size = _diskProvider.GetFileSize(localEpisode.Path);
                    episodeFile.Quality = localEpisode.Quality;
                    episodeFile.SeasonNumber = localEpisode.SeasonNumber;
                    episodeFile.SceneName = Path.GetFileNameWithoutExtension(localEpisode.Path.CleanPath());
                    episodeFile.Episodes = localEpisode.Episodes;

                    if (newDownload)
                    {
                        episodeFile = _episodeFileMover.MoveEpisodeFile(episodeFile, localEpisode);
                        _messageAggregator.PublishEvent(new EpisodeImportedEvent(episodeFile));
                    }
                    
                    _mediaFileService.Add(episodeFile);
                    imported.Add(importDecision);
                }
                catch (Exception e)
                {
                    _logger.WarnException("Couldn't add report to download queue. " + localEpisode, e);
                }
            }

            return imported;
        }

        private List<ImportDecision> GetQualifiedImports(List<ImportDecision> decisions)
        {
            return decisions.Where(c => c.Approved)
                            .OrderByDescending(c => c.LocalEpisode.Quality)
                            .ToList();
        }
    }
}
