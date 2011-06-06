using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using NLog;
using NzbDrone.Core.Helpers;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers
{
    public class RenameProvider
    {
        //TODO: Remove some of these dependencies. we shouldn't have a single class with dependency on the whole app!
        //TODO: Also upgrade to a job that can run on background thread.
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly ConfigProvider _configProvider;
        private readonly DiskProvider _diskProvider;
        private readonly EpisodeProvider _episodeProvider;
        private readonly MediaFileProvider _mediaFileProvider;
        private readonly SeriesProvider _seriesProvider;

        private readonly List<EpisodeRenameModel> _epsToRename = new List<EpisodeRenameModel>();

        private Thread _renameThread;

        public RenameProvider(SeriesProvider seriesProvider,EpisodeProvider episodeProvider,
                                MediaFileProvider mediaFileProvider, DiskProvider diskProvider,
                                ConfigProvider configProvider)
        {
            _seriesProvider = seriesProvider;
            _episodeProvider = episodeProvider;
            _mediaFileProvider = mediaFileProvider;
            _diskProvider = diskProvider;
            _configProvider = configProvider;
        }

        public virtual void RenameAll()
        {
            //Get a list of all episode files/episodes and rename them

            foreach (var episodeFile in _mediaFileProvider.GetEpisodeFiles())
            {
                var series = _seriesProvider.GetSeries(episodeFile.SeriesId);
                var erm = new EpisodeRenameModel { SeriesName = series.Title, Folder = series.Path };

                if (series.SeasonFolder)
                    erm.Folder += Path.DirectorySeparatorChar +
                                  EpisodeRenameHelper.GetSeasonFolder(episodeFile.Episodes[0].SeasonNumber,
                                                                      _configProvider.GetValue(
                                                                          "Sorting_SeasonFolderFormat", "Season %s",
                                                                          true));

                erm.EpisodeFile = episodeFile;
                _epsToRename.Add(erm);
                StartRename();
            }
        }

        public virtual void RenameSeries(int seriesId)
        {
            //Get a list of all applicable episode files/episodes and rename them

            var series = _seriesProvider.GetSeries(seriesId);

            foreach (var episodeFile in _mediaFileProvider.GetEpisodeFiles().Where(s => s.SeriesId == seriesId))
            {
                var erm = new EpisodeRenameModel { SeriesName = series.Title, Folder = series.Path };

                if (series.SeasonFolder)
                    erm.Folder += Path.DirectorySeparatorChar +
                                  EpisodeRenameHelper.GetSeasonFolder(episodeFile.Episodes[0].SeasonNumber,
                                                                      _configProvider.GetValue(
                                                                          "Sorting_SeasonFolderFormat", "Season %s",
                                                                          true));

                erm.EpisodeFile = episodeFile;
                _epsToRename.Add(erm);
                StartRename();
            }
        }

        public virtual void RenameSeason(int seasonId)
        {
            throw new NotImplementedException();
        }

        public virtual void RenameEpisode(int episodeId)
        {
            //This will properly rename multi-episode files if asked to rename either of the episode
            var episode = _episodeProvider.GetEpisode(episodeId);
            var series = _seriesProvider.GetSeries(episode.SeriesId);

            var episodeFile =
                _mediaFileProvider.GetEpisodeFiles().Where(s => s.Episodes.Contains(episode)).FirstOrDefault();

            var erm = new EpisodeRenameModel { SeriesName = series.Title, Folder = series.Path };

            if (series.SeasonFolder)
                erm.Folder += Path.DirectorySeparatorChar +
                              EpisodeRenameHelper.GetSeasonFolder(episodeFile.Episodes[0].SeasonNumber,
                                                                  _configProvider.GetValue(
                                                                      "Sorting_SeasonFolderFormat", "Season %s", true));

            erm.EpisodeFile = episodeFile;
            _epsToRename.Add(erm);
            StartRename();
        }

        public virtual void RenameEpisodeFile(int episodeFileId, bool newDownload)
        {
            //This will properly rename multi-episode files if asked to rename either of the episode
            var episodeFile = _mediaFileProvider.GetEpisodeFile(episodeFileId);
            var series = _seriesProvider.GetSeries(episodeFile.Series.SeriesId);

            var erm = new EpisodeRenameModel { SeriesName = series.Title, Folder = series.Path };

            if (series.SeasonFolder)
                erm.Folder += Path.DirectorySeparatorChar +
                              EpisodeRenameHelper.GetSeasonFolder(episodeFile.Episodes[0].SeasonNumber,
                                                                  _configProvider.GetValue(
                                                                      "Sorting_SeasonFolderFormat", "Season %s", true));

            erm.EpisodeFile = episodeFile;
            _epsToRename.Add(erm);
            StartRename();
        }

        private void StartRename()
        {
            Logger.Debug("Episode Rename Starting");
            if (_renameThread == null || !_renameThread.IsAlive)
            {
                Logger.Debug("Initializing background rename of episodes");
                _renameThread = new Thread(RenameProcessor)
                                    {
                                        Name = "RenameEpisodes",
                                        Priority = ThreadPriority.Lowest
                                    };

                _renameThread.Start();
            }
            else
            {
                Logger.Warn("Episode renaming already in progress. Ignoring request.");
            }
        }

        private void RenameProcessor()
        {
            while (_epsToRename.Count > 0)
            {
                var ep = _epsToRename.First();
                _epsToRename.RemoveAt(0);
                RenameFile(ep);
            }
        }

        private void RenameFile(EpisodeRenameModel erm)
        {
            try
            {
                //Update EpisodeFile if successful
                Logger.Debug("Renaming Episode: {0}", Path.GetFileName(erm.EpisodeFile.Path));
                var newName = EpisodeRenameHelper.GetNewName(erm);
                var ext = Path.GetExtension(erm.EpisodeFile.Path);
                var newFilename = erm.Folder + Path.DirectorySeparatorChar + newName + ext;

                if (!_diskProvider.FolderExists(erm.Folder))
                    _diskProvider.CreateDirectory(erm.Folder);

                if (erm.EpisodeFile.Path == newFilename)
                    return;

                _diskProvider.RenameFile(erm.EpisodeFile.Path, newFilename);
                erm.EpisodeFile.Path = newFilename;
                _mediaFileProvider.Update(erm.EpisodeFile);

                throw new NotImplementedException("Rename File");
            }
            catch (Exception ex)
            {
                Logger.DebugException(ex.Message, ex);
                Logger.Warn("Unable to Rename Episode: {0}", Path.GetFileName(erm.EpisodeFile.Path));
            }
        }

        public string GetNewFilename(EpisodeFile episodeFile)
        {
            var episodes = _episodeProvider.EpisodesByFileId(episodeFile.EpisodeFileId);
            var series = _seriesProvider.GetSeries(episodeFile.SeriesId);

            var separatorStyle = EpisodeSortingHelper.GetSeparatorStyle(_configProvider.SeparatorStyle);
            var numberStyle = EpisodeSortingHelper.GetNumberStyle(_configProvider.NumberStyle);
            var useSeriesName = _configProvider.SeriesName;
            var useEpisodeName = _configProvider.EpisodeName;
            var replaceSpaces = _configProvider.ReplaceSpaces;
            var appendQuality = _configProvider.AppendQuality;

            var title = String.Empty;

            if (episodes.Count == 1)
            {
                if (useSeriesName)
                {
                    title += series.Title;
                    title += separatorStyle.Pattern;
                }

                title += numberStyle.Pattern.Replace("%s", String.Format("{0}", episodes[0].SeasonNumber))
                                .Replace("%0s", String.Format("{0:00}", episodes[0].SeasonNumber))
                                .Replace("%0e", String.Format("{0:00}", episodes[0].EpisodeNumber));
                
                if (useEpisodeName)
                {
                    title += separatorStyle.Pattern;
                    title += episodes[0].Title;
                }

                if (appendQuality)
                    title += String.Format(" [{0}]", episodeFile.Quality);

                if (replaceSpaces)
                    title = title.Replace(' ', '.');

                Logger.Debug("New File Name is: {0}", title);
                return title;
            }

            var multiEpisodeStyle = EpisodeSortingHelper.GetMultiEpisodeStyle(_configProvider.MultiEpisodeStyle);

            if (useSeriesName)
            {
                title += series.Title;
                title += separatorStyle.Pattern;
            }

            title += numberStyle.Pattern.Replace("%s", String.Format("{0}", episodes[0].SeasonNumber))
                                .Replace("%0s", String.Format("{0:00}", episodes[0].SeasonNumber))
                                .Replace("%0e", String.Format("{0:00}", episodes[0].EpisodeNumber));

            var numbers = String.Empty;
            var episodeNames = episodes[0].Title;

            for (int i = 1; i < episodes.Count; i++)
            {
                var episode = episodes[i];

                if (multiEpisodeStyle.Name == "Duplicate")
                {
                    numbers += separatorStyle.Pattern + numberStyle.Pattern.Replace("%s", String.Format("{0}", episode.SeasonNumber))
                                .Replace("%0s", String.Format("{0:00}", episode.SeasonNumber))
                                .Replace("%0e", String.Format("{0:00}", episode.EpisodeNumber));
                }
                else
                {
                    numbers += multiEpisodeStyle.Pattern.Replace("%s", String.Format("{0}", episode.SeasonNumber))
                                .Replace("%0s", String.Format("{0:00}", episode.SeasonNumber))
                                .Replace("%0e", String.Format("{0:00}", episode.EpisodeNumber))
                                .Replace("%x", numberStyle.EpisodeSeparator)
                                .Replace("%p", separatorStyle.Pattern);
                }

                episodeNames += String.Format(" + {0}", episode.Title);
            }

            title += numbers;

            if (useEpisodeName)
            {
                episodeNames = episodeNames.TrimEnd(' ', '+');

                title += separatorStyle.Pattern;
                title += episodeNames;
            }

            if (appendQuality)
                title += String.Format(" [{0}]", episodeFile.Quality);

            if (replaceSpaces)
                title = title.Replace(' ', '.');

            Logger.Debug("New File Name is: {0}", title);
            return title;
        }
    }
}