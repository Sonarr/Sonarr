using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using NLog;
using NzbDrone.Core.Helpers;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers
{
    public class RenameProvider : IRenameProvider
    {
        private readonly ISeriesProvider _seriesProvider;
        private readonly ISeasonProvider _seasonProvider;
        private readonly IEpisodeProvider _episodeProvider;
        private readonly IMediaFileProvider _mediaFileProvider;
        private readonly IDiskProvider _diskProvider;
        private readonly IConfigProvider _configProvider;
        private readonly ExternalNotificationProvider _externalNotificationProvider;

        private Thread _renameThread;
        private List<EpisodeRenameModel> _epsToRename = new List<EpisodeRenameModel>();

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public RenameProvider(ISeriesProvider seriesProvider, ISeasonProvider seasonProvider,
            IEpisodeProvider episodeProvider, IMediaFileProvider mediaFileProvider,
            IDiskProvider diskProvider, IConfigProvider configProvider,
            ExternalNotificationProvider extenalNotificationProvider)
        {
            _seriesProvider = seriesProvider;
            _seasonProvider = seasonProvider;
            _episodeProvider = episodeProvider;
            _mediaFileProvider = mediaFileProvider;
            _diskProvider = diskProvider;
            _configProvider = configProvider;
            _externalNotificationProvider = extenalNotificationProvider;
        }

        #region IRenameProvider Members
        public void RenameAll()
        {
            //Get a list of all episode files/episodes and rename them

            foreach (var episodeFile in _mediaFileProvider.GetEpisodeFiles())
            {
                var series = _seriesProvider.GetSeries(episodeFile.SeriesId);
                var erm = new EpisodeRenameModel();
                erm.SeriesName = series.Title;
                erm.Folder = series.Path;

                if (series.SeasonFolder)
                    erm.Folder += Path.DirectorySeparatorChar + EpisodeRenameHelper.GetSeasonFolder(episodeFile.Episodes[0].SeasonNumber, _configProvider.GetValue("Sorting_SeasonFolderFormat", "Season %s", true));

                erm.EpisodeFile = episodeFile;
                _epsToRename.Add(erm);
                StartRename();
            }
        }

        public void RenameSeries(int seriesId)
        {
            //Get a list of all applicable episode files/episodes and rename them

            var series = _seriesProvider.GetSeries(seriesId);

            foreach (var episodeFile in _mediaFileProvider.GetEpisodeFiles().Where(s => s.SeriesId == seriesId))
            {
                var erm = new EpisodeRenameModel();
                erm.SeriesName = series.Title;

                erm.Folder = series.Path;

                if (series.SeasonFolder)
                    erm.Folder += Path.DirectorySeparatorChar + EpisodeRenameHelper.GetSeasonFolder(episodeFile.Episodes[0].SeasonNumber, _configProvider.GetValue("Sorting_SeasonFolderFormat", "Season %s", true));

                erm.EpisodeFile = episodeFile;
                _epsToRename.Add(erm);
                StartRename();
            }
        }

        public void RenameSeason(int seasonId)
        {
            //Get a list of all applicable episode files/episodes and rename them
            var season = _seasonProvider.GetSeason(seasonId);
            var series = _seriesProvider.GetSeries(season.SeriesId);

            foreach (var episodeFile in _mediaFileProvider.GetEpisodeFiles().Where(s => s.Episodes[0].SeasonId == seasonId))
            {
                var erm = new EpisodeRenameModel();
                erm.SeriesName = series.Title;

                erm.Folder = series.Path;

                if (series.SeasonFolder)
                    erm.Folder += Path.DirectorySeparatorChar + EpisodeRenameHelper.GetSeasonFolder(episodeFile.Episodes[0].SeasonNumber, _configProvider.GetValue("Sorting_SeasonFolderFormat", "Season %s", true));

                erm.EpisodeFile = episodeFile;
                _epsToRename.Add(erm);
                StartRename();
            }
        }

        public void RenameEpisode(int episodeId)
        {
            //This will properly rename multi-episode files if asked to rename either of the episode
            var episode = _episodeProvider.GetEpisode(episodeId);
            var series = _seriesProvider.GetSeries(episode.SeriesId);

            var episodeFile = _mediaFileProvider.GetEpisodeFiles().Where(s => s.Episodes.Contains(episode)).FirstOrDefault();

            var erm = new EpisodeRenameModel();
            erm.SeriesName = series.Title;

            erm.Folder = series.Path;

            if (series.SeasonFolder)
                erm.Folder += Path.DirectorySeparatorChar + EpisodeRenameHelper.GetSeasonFolder(episodeFile.Episodes[0].SeasonNumber, _configProvider.GetValue("Sorting_SeasonFolderFormat", "Season %s", true));

            erm.EpisodeFile = episodeFile;
            _epsToRename.Add(erm);
            StartRename();
        }

        public void RenameEpisodeFile(int episodeFileId, bool newDownload)
        {
            //This will properly rename multi-episode files if asked to rename either of the episode
            var episodeFile = _mediaFileProvider.GetEpisodeFile(episodeFileId);
            var series = _seriesProvider.GetSeries(episodeFile.Series.SeriesId);

            var erm = new EpisodeRenameModel();
            erm.SeriesName = series.Title;

            erm.Folder = series.Path;

            if (series.SeasonFolder)
                erm.Folder += Path.DirectorySeparatorChar + EpisodeRenameHelper.GetSeasonFolder(episodeFile.Episodes[0].SeasonNumber, _configProvider.GetValue("Sorting_SeasonFolderFormat", "Season %s", true));

            erm.EpisodeFile = episodeFile;
            _epsToRename.Add(erm);
            StartRename();
        }

        #endregion

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

                if (erm.NewDownload)
                    _externalNotificationProvider.OnDownload(erm);

                else
                    _externalNotificationProvider.OnRename(erm);

            }
            catch (Exception ex)
            {
                Logger.DebugException(ex.Message, ex);
                Logger.Warn("Unable to Rename Episode: {0}", Path.GetFileName(erm.EpisodeFile.Path));
            }
        }
    }
}
