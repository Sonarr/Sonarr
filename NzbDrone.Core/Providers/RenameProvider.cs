using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using NLog;
using NzbDrone.Core.Model;
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
        private Thread _renameThread;
        private List<EpisodeRenameModel> _epsToRename = new List<EpisodeRenameModel>();

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public RenameProvider(ISeriesProvider seriesProvider, ISeasonProvider seasonProvider,
            IEpisodeProvider episodeProvider, IMediaFileProvider mediaFileProvider,
            IDiskProvider diskProvider, IConfigProvider configProvider)
        {
            _seriesProvider = seriesProvider;
            _seasonProvider = seasonProvider;
            _episodeProvider = episodeProvider;
            _mediaFileProvider = mediaFileProvider;
            _diskProvider = diskProvider;
            _configProvider = configProvider;
        }

        #region IRenameProvider Members
        public void RenameAll()
        {
            //Get a list of all episode files/episodes and rename them

            var seasonFolder = _configProvider.GetValue("SeasonFolder", "Season %s", true);

            foreach (var series in _seriesProvider.GetAllSeries())
            {
                foreach (var episode in series.Episodes)
                {
                    var episodeRenameModel = new EpisodeRenameModel();
                    episodeRenameModel.SeriesName = series.Title;
                    episodeRenameModel.SeasonNumber = episode.SeasonNumber;
                    episodeRenameModel.EpisodeNumber = episode.EpisodeNumber;
                    episodeRenameModel.EpisodeName = episode.Title;
                    episodeRenameModel.Folder = series.Path + Path.DirectorySeparatorChar + seasonFolder;
                    episodeRenameModel.EpisodeFile = episode.EpisodeFile;

                    _epsToRename.Add(episodeRenameModel);
                    StartRename();
                }
            }
        }

        public void RenameSeries(int seriesId)
        {
            //Get a list of all applicable episode files/episodes and rename them

            var series = _seriesProvider.GetSeries(seriesId);
            var seasonFolder = _configProvider.GetValue("SeasonFolder", "Season %s", true);

            foreach (var episode in series.Episodes)
            {
                var episodeRenameModel = new EpisodeRenameModel();
                episodeRenameModel.SeriesName = series.Title;
                episodeRenameModel.SeasonNumber = episode.SeasonNumber;
                episodeRenameModel.EpisodeNumber = episode.EpisodeNumber;
                episodeRenameModel.EpisodeName = episode.Title;
                episodeRenameModel.Folder = series.Path + Path.DirectorySeparatorChar + seasonFolder;
                episodeRenameModel.EpisodeFile = episode.EpisodeFile;

                _epsToRename.Add(episodeRenameModel);
                StartRename();
            }
        }

        public void RenameSeason(int seasonId)
        {
            //Get a list of all applicable episode files/episodes and rename them
            var season = _seasonProvider.GetSeason(seasonId);
            var series = _seriesProvider.GetSeries(season.SeriesId);
            var seasonFolder = _configProvider.GetValue("SeasonFolder", "Season %s", true);

            foreach (var episode in season.Episodes)
            {
                var episodeRenameModel = new EpisodeRenameModel();
                episodeRenameModel.SeriesName = series.Title;
                episodeRenameModel.SeasonNumber = episode.SeasonNumber;
                episodeRenameModel.EpisodeNumber = episode.EpisodeNumber;
                episodeRenameModel.EpisodeName = episode.Title;
                episodeRenameModel.Folder = series.Path + Path.DirectorySeparatorChar + seasonFolder;
                episodeRenameModel.EpisodeFile = episode.EpisodeFile;

                _epsToRename.Add(episodeRenameModel);
                StartRename();
            }
        }

        public void RenameEpisode(int episodeId)
        {
            var episode = _episodeProvider.GetEpisode(episodeId);
            var series = _seriesProvider.GetSeries(episode.SeriesId);
            var seasonFolder = _configProvider.GetValue("SeasonFolder", "Season %s", true);

            var episodeRenameModel = new EpisodeRenameModel();
            episodeRenameModel.SeriesName = series.Title;
            episodeRenameModel.SeasonNumber = episode.SeasonNumber;
            episodeRenameModel.EpisodeNumber = episode.EpisodeNumber;
            episodeRenameModel.EpisodeName = episode.Title;
            episodeRenameModel.Folder = series.Path + Path.DirectorySeparatorChar + seasonFolder;
            episodeRenameModel.EpisodeFile = episode.EpisodeFile;

            _epsToRename.Add(episodeRenameModel);
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

        private void RenameFile(EpisodeRenameModel episodeRenameModel)
        {
            try
            {
                //Update EpisodeFile if successful
                Logger.Debug("Renaming Episode: {0}", Path.GetFileName(episodeRenameModel.EpisodeFile.Path));
                var newName = GetNewName(episodeRenameModel);
                var newFilename = episodeRenameModel.Folder + Path.DirectorySeparatorChar + newName;

                if (!_diskProvider.FolderExists(episodeRenameModel.Folder))
                    _diskProvider.CreateDirectory(episodeRenameModel.Folder);

                _diskProvider.RenameFile(episodeRenameModel.EpisodeFile.Path, newFilename);
                episodeRenameModel.EpisodeFile.Path = newFilename;
                _mediaFileProvider.Update(episodeRenameModel.EpisodeFile);

            }
            catch (Exception ex)
            {
                Logger.DebugException(ex.Message, ex);
                Logger.Warn("Unable to Rename Episode: {0}", Path.GetFileName(episodeRenameModel.EpisodeFile.Path));
            }
        }

        private string GetNewName(EpisodeRenameModel episodeRenameModel)
        {
            //Todo: Get the users preferred naming convention instead of hard-coding it
            return String.Format("{0} - S{1:00}E{2:00} - {3}", episodeRenameModel.SeriesName,
                                 episodeRenameModel.SeasonNumber, episodeRenameModel.EpisodeNumber,
                                 episodeRenameModel.EpisodeName);
            //var fileString = _configProvider.GetValue("")
        }
    }
}
