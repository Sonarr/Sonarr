using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles
{
    public interface IRenameEpisodeFileService
    {
        List<RenameEpisodeFilePreview> GetRenamePreviews(int seriesId);
        List<RenameEpisodeFilePreview> GetRenamePreviews(int seriesId, int seasonNumber);
        List<RenameEpisodeFilePreview> GetRenamePreviews(List<int> seriesIds);
    }

    public class RenameEpisodeFileService : IRenameEpisodeFileService,
                                            IExecute<RenameFilesCommand>,
                                            IExecute<RenameSeriesCommand>
    {
        private readonly ISeriesService _seriesService;
        private readonly IMediaFileService _mediaFileService;
        private readonly IMoveEpisodeFiles _episodeFileMover;
        private readonly IEventAggregator _eventAggregator;
        private readonly IEpisodeService _episodeService;
        private readonly IBuildFileNames _filenameBuilder;
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        private enum RenameState
        {
            InProgress,
            Resolvable,
            Blocked
        }

        private class EpisodeFileRename
        {
            public EpisodeFile EpisodeFile { get; set; }
            public string PreviousRelativePath { get; set; }
            public string CurrentPath { get; set; }
            public string TargetPath { get; set; }
        }

        private class RenamePlan
        {
            public List<EpisodeFileRename> Moves { get; } = new List<EpisodeFileRename>();
            public List<List<EpisodeFileRename>> Cycles { get; } = new List<List<EpisodeFileRename>>();
            public List<EpisodeFileRename> Blocked { get; } = new List<EpisodeFileRename>();
        }

        public RenameEpisodeFileService(ISeriesService seriesService,
                                        IMediaFileService mediaFileService,
                                        IMoveEpisodeFiles episodeFileMover,
                                        IEventAggregator eventAggregator,
                                        IEpisodeService episodeService,
                                        IBuildFileNames filenameBuilder,
                                        IDiskProvider diskProvider,
                                        Logger logger)
        {
            _seriesService = seriesService;
            _mediaFileService = mediaFileService;
            _episodeFileMover = episodeFileMover;
            _eventAggregator = eventAggregator;
            _episodeService = episodeService;
            _filenameBuilder = filenameBuilder;
            _diskProvider = diskProvider;
            _logger = logger;
        }

        public List<RenameEpisodeFilePreview> GetRenamePreviews(int seriesId)
        {
            var series = _seriesService.GetSeries(seriesId);
            var episodes = _episodeService.GetEpisodeBySeries(seriesId);
            var files = _mediaFileService.GetFilesBySeries(seriesId);

            return GetPreviews(series, episodes, files)
                .OrderByDescending(e => e.SeasonNumber)
                .ThenByDescending(e => e.EpisodeNumbers.First())
                .ToList();
        }

        public List<RenameEpisodeFilePreview> GetRenamePreviews(int seriesId, int seasonNumber)
        {
            var series = _seriesService.GetSeries(seriesId);
            var episodes = _episodeService.GetEpisodesBySeason(seriesId, seasonNumber);
            var files = _mediaFileService.GetFilesBySeason(seriesId, seasonNumber);

            return GetPreviews(series, episodes, files)
                .OrderByDescending(e => e.EpisodeNumbers.First()).ToList();
        }

        public List<RenameEpisodeFilePreview> GetRenamePreviews(List<int> seriesIds)
        {
            var seriesList = _seriesService.GetSeries(seriesIds);
            var episodesList = _episodeService.GetEpisodesBySeries(seriesIds).ToLookup(e => e.SeriesId);
            var filesList = _mediaFileService.GetFilesBySeriesIds(seriesIds).ToLookup(f => f.SeriesId);

            return seriesList.SelectMany(series =>
                {
                    var episodes = episodesList[series.Id].ToList();
                    var files = filesList[series.Id].ToList();

                    return GetPreviews(series, episodes, files);
                })
                .OrderByDescending(e => e.SeriesId)
                .ThenByDescending(e => e.SeasonNumber)
                .ThenByDescending(e => e.EpisodeNumbers.First())
                .ToList();
        }

        private IEnumerable<RenameEpisodeFilePreview> GetPreviews(Series series, List<Episode> episodes, List<EpisodeFile> files)
        {
            foreach (var f in files)
            {
                var file = f;
                var episodesInFile = episodes.Where(e => e.EpisodeFileId == file.Id).ToList();
                var episodeFilePath = Path.Combine(series.Path, file.RelativePath);

                if (!episodesInFile.Any())
                {
                    _logger.Warn("File ({0}) is not linked to any episodes", episodeFilePath);
                    continue;
                }

                var seasonNumber = episodesInFile.First().SeasonNumber;
                var newPath = _filenameBuilder.BuildFilePath(episodesInFile, series, file, Path.GetExtension(episodeFilePath));

                if (!episodeFilePath.PathEquals(newPath, StringComparison.Ordinal))
                {
                    yield return new RenameEpisodeFilePreview
                    {
                        SeriesId = series.Id,
                        SeasonNumber = seasonNumber,
                        EpisodeNumbers = episodesInFile.Select(e => e.EpisodeNumber).ToList(),
                        EpisodeFileId = file.Id,
                        ExistingPath = file.RelativePath,
                        NewPath = series.Path.GetRelativePath(newPath)
                    };
                }
            }
        }

        private List<RenamedEpisodeFile> RenameFiles(List<EpisodeFile> episodeFiles, Series series)
        {
            var renamed = new List<RenamedEpisodeFile>();
            var pending = BuildPendingRenames(episodeFiles, series);
            var plan = BuildRenamePlan(pending);

            foreach (var blocked in plan.Blocked)
            {
                _logger.Warn("File not renamed, there is already a file at the destination: {0}", blocked.TargetPath);
            }

            foreach (var move in plan.Moves)
            {
                ExecuteMove(move, series, renamed);
            }

            foreach (var cycle in plan.Cycles)
            {
                ExecuteCycle(cycle, series, renamed);
            }

            if (renamed.Any())
            {
                _diskProvider.RemoveEmptySubfolders(series.Path);

                _eventAggregator.PublishEvent(new SeriesRenamedEvent(series, renamed));
            }

            return renamed;
        }

        // Computes every file's current and target path up front so conflicts can be planned around
        // instead of discovered mid-move.
        private List<EpisodeFileRename> BuildPendingRenames(List<EpisodeFile> episodeFiles, Series series)
        {
            var pending = new List<EpisodeFileRename>();

            foreach (var episodeFile in episodeFiles)
            {
                var currentPath = Path.Combine(series.Path, episodeFile.RelativePath);
                var episodes = _episodeService.GetEpisodesByFileId(episodeFile.Id);
                var targetPath = _filenameBuilder.BuildFilePath(episodes, series, episodeFile, Path.GetExtension(episodeFile.RelativePath));

                if (currentPath.PathEquals(targetPath))
                {
                    _logger.Debug("File not renamed, source and destination are the same: {0}", currentPath);
                    continue;
                }

                pending.Add(new EpisodeFileRename
                            {
                                EpisodeFile = episodeFile,
                                PreviousRelativePath = episodeFile.RelativePath,
                                CurrentPath = currentPath,
                                TargetPath = targetPath
                            });
            }

            return pending;
        }

        // Follows each file's target to whoever currently occupies it (if anyone) to determine, before any
        // file is touched, whether the whole chain of dependent moves can ultimately complete. A file whose
        // target is occupied by another file in this batch can only move once that file has moved; a chain
        // that bottoms out at a free path can all be moved (last-to-first); a chain that bottoms out at a
        // path occupied by a file outside this batch can never complete and is left untouched entirely; a
        // chain that loops back on itself (e.g. episodes swapping numbers) is a closed rotation that can
        // always be completed using one temporary displacement.
        private RenamePlan BuildRenamePlan(List<EpisodeFileRename> pending)
        {
            var plan = new RenamePlan();
            var byCurrentPath = new Dictionary<string, EpisodeFileRename>(PathEqualityComparer.Instance);

            foreach (var rename in pending)
            {
                byCurrentPath[rename.CurrentPath] = rename;
            }

            EpisodeFileRename NextInBatch(EpisodeFileRename rename)
            {
                return byCurrentPath.TryGetValue(rename.TargetPath, out var occupant) && occupant != rename ? occupant : null;
            }

            var state = new Dictionary<EpisodeFileRename, RenameState>();

            foreach (var start in pending)
            {
                if (state.ContainsKey(start))
                {
                    continue;
                }

                var stack = new List<EpisodeFileRename>();
                var current = start;
                var settled = false;

                // Each step either breaks immediately or consumes one file that was not yet in 'state',
                // and 'state' can hold at most pending.Count distinct files - so the walk cannot take more
                // than pending.Count + 1 steps to either reach a terminal or loop back on itself.
                for (var step = 0; step <= pending.Count; step++)
                {
                    if (state.TryGetValue(current, out var currentState))
                    {
                        if (currentState == RenameState.InProgress)
                        {
                            // Looped back into our own stack: a closed rotation.
                            var cycleStart = stack.IndexOf(current);
                            var cycle = stack.GetRange(cycleStart, stack.Count - cycleStart);
                            var prefix = stack.GetRange(0, cycleStart);

                            MarkResolvable(cycle, state);
                            plan.Cycles.Add(cycle);

                            // A tail leading into a rotation from outside it can't be freed by the rotation alone.
                            MarkBlocked(prefix, state);
                            plan.Blocked.AddRange(prefix);
                        }
                        else if (currentState == RenameState.Resolvable)
                        {
                            MarkResolvableInOrder(stack, state, plan.Moves);
                        }
                        else
                        {
                            MarkBlocked(stack, state);
                            plan.Blocked.AddRange(stack);
                        }

                        settled = true;
                        break;
                    }

                    stack.Add(current);
                    state[current] = RenameState.InProgress;

                    var next = NextInBatch(current);

                    if (next == null)
                    {
                        if (_diskProvider.FileExists(current.TargetPath))
                        {
                            MarkBlocked(stack, state);
                            plan.Blocked.AddRange(stack);
                        }
                        else
                        {
                            MarkResolvableInOrder(stack, state, plan.Moves);
                        }

                        settled = true;
                        break;
                    }

                    current = next;
                }

                if (!settled)
                {
                    // Unreachable unless the traversal above no longer satisfies the bound described
                    // above - fail loudly rather than silently dropping files from the plan.
                    throw new InvalidOperationException("Unable to resolve episode rename dependency chain");
                }
            }

            return plan;
        }

        private static void MarkResolvableInOrder(List<EpisodeFileRename> chain, Dictionary<EpisodeFileRename, RenameState> state, List<EpisodeFileRename> moves)
        {
            // The file at the end of the chain has a free target and must move first, freeing the path
            // needed by the file before it, and so on back to the start of the chain.
            for (var i = chain.Count - 1; i >= 0; i--)
            {
                state[chain[i]] = RenameState.Resolvable;
                moves.Add(chain[i]);
            }
        }

        private static void MarkResolvable(List<EpisodeFileRename> renames, Dictionary<EpisodeFileRename, RenameState> state)
        {
            foreach (var rename in renames)
            {
                state[rename] = RenameState.Resolvable;
            }
        }

        private static void MarkBlocked(List<EpisodeFileRename> renames, Dictionary<EpisodeFileRename, RenameState> state)
        {
            foreach (var rename in renames)
            {
                state[rename] = RenameState.Blocked;
            }
        }

        private void ExecuteMove(EpisodeFileRename move, Series series, List<RenamedEpisodeFile> renamed)
        {
            try
            {
                _logger.Debug("Renaming episode file: {0}", move.EpisodeFile);
                _episodeFileMover.MoveEpisodeFile(move.EpisodeFile, series);

                // Clear the temporary source path override (if any) now that the file lives at its real destination.
                move.EpisodeFile.Path = null;

                _mediaFileService.Update(move.EpisodeFile);

                renamed.Add(new RenamedEpisodeFile
                            {
                                EpisodeFile = move.EpisodeFile,
                                PreviousRelativePath = move.PreviousRelativePath,
                                PreviousPath = move.CurrentPath
                            });

                _logger.Debug("Renamed episode file: {0}", move.EpisodeFile);

                _eventAggregator.PublishEvent(new EpisodeFileRenamedEvent(series, move.EpisodeFile, move.CurrentPath));
            }
            catch (SameFilenameException ex)
            {
                _logger.Debug("File not renamed, source and destination are the same: {0}", ex.Filename);
            }
            catch (FileAlreadyExistsException ex)
            {
                _logger.Warn("File not renamed, there is already a file at the destination: {0}", ex.Filename);
            }
            catch (DestinationAlreadyExistsException)
            {
                _logger.Warn("File not renamed, there is already a file at the destination: {0}", move.TargetPath);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to rename file {0}", move.CurrentPath);
            }
        }

        // A closed rotation (e.g. episode 1 and 2 swapping numbers) has no free slot to start from - every
        // target in the loop is occupied by another member of the loop. One member is displaced to a
        // temporary name just long enough to free up the loop, then moved into its real, final target once
        // the rest of the loop has vacated it; the temporary name is never persisted.
        private void ExecuteCycle(List<EpisodeFileRename> cycle, Series series, List<RenamedEpisodeFile> renamed)
        {
            var displaced = cycle[0];
            var tempPath = GenerateTempPath(displaced.CurrentPath);

            try
            {
                _diskProvider.MoveFile(displaced.CurrentPath, tempPath);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to rename file {0}", displaced.CurrentPath);
                return;
            }

            displaced.EpisodeFile.Path = tempPath;

            for (var i = cycle.Count - 1; i >= 1; i--)
            {
                ExecuteMove(cycle[i], series, renamed);
            }

            ExecuteMove(displaced, series, renamed);
        }

        private string GenerateTempPath(string path)
        {
            var tempPath = path + ".swap~";
            var attempt = 1;

            while (_diskProvider.FileExists(tempPath))
            {
                tempPath = $"{path}.swap{attempt}~";
                attempt++;
            }

            return tempPath;
        }

        public void Execute(RenameFilesCommand message)
        {
            var series = _seriesService.GetSeries(message.SeriesId);
            var episodeFiles = _mediaFileService.Get(message.Files);

            _logger.ProgressInfo("Renaming {0} files for {1}", episodeFiles.Count, series.Title);
            var renamedFiles = RenameFiles(episodeFiles, series);
            _logger.ProgressInfo("{0} selected episode files renamed for {1}", renamedFiles.Count, series.Title);

            _eventAggregator.PublishEvent(new RenameCompletedEvent());
        }

        public void Execute(RenameSeriesCommand message)
        {
            _logger.Debug("Renaming all files for selected series");
            var seriesToRename = _seriesService.GetSeries(message.SeriesIds);

            foreach (var series in seriesToRename)
            {
                var episodeFiles = _mediaFileService.GetFilesBySeries(series.Id);
                _logger.ProgressInfo("Renaming all files in series: {0}", series.Title);
                var renamedFiles = RenameFiles(episodeFiles, series);
                _logger.ProgressInfo("{0} episode files renamed for {1}", renamedFiles.Count, series.Title);
            }

            _eventAggregator.PublishEvent(new RenameCompletedEvent());
        }
    }
}
