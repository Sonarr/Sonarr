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

            foreach (var episodeFile in _mediaFileProvider.GetEpisodeFiles())
            {
                var series = _seriesProvider.GetSeries(episodeFile.SeriesId);
                var erm = new EpisodeRenameModel();
                erm.SeriesName = series.Title;
                erm.Folder = series.Path + Path.DirectorySeparatorChar + seasonFolder;
                erm.EpisodeFile = episodeFile;
                _epsToRename.Add(erm);
                StartRename();
            }
        }

        public void RenameSeries(int seriesId)
        {
            //Get a list of all applicable episode files/episodes and rename them

            var series = _seriesProvider.GetSeries(seriesId);
            var seasonFolder = _configProvider.GetValue("SeasonFolder", "Season %s", true);

            foreach (var episodeFile in _mediaFileProvider.GetEpisodeFiles().Where(s => s.SeriesId == seriesId))
            {
                var erm = new EpisodeRenameModel();
                erm.SeriesName = series.Title;
                erm.Folder = series.Path + Path.DirectorySeparatorChar + seasonFolder;
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
            var seasonFolder = _configProvider.GetValue("SeasonFolder", "Season %s", true);

            foreach (var episodeFile in _mediaFileProvider.GetEpisodeFiles().Where(s => s.Episodes[0].SeasonId == seasonId))
            {
                var erm = new EpisodeRenameModel();
                erm.SeriesName = series.Title;
                erm.Folder = series.Path + Path.DirectorySeparatorChar + seasonFolder;
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
            var seasonFolder = _configProvider.GetValue("SeasonFolder", "Season %s", true);

            var episodeFile = _mediaFileProvider.GetEpisodeFiles().Where(s => s.Episodes.Contains(episode)).FirstOrDefault();

            var erm = new EpisodeRenameModel();
            erm.SeriesName = series.Title;
            erm.Folder = series.Path + Path.DirectorySeparatorChar + seasonFolder;
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

        private string GetNewName(EpisodeRenameModel erm)
        {
            //Todo: Get the users preferred naming convention instead of hard-coding it

            if (erm.EpisodeFile.Episodes.Count == 1)
            {
                return String.Format("{0} - S{1:00}E{2:00} - {3}", erm.SeriesName,
                                     erm.EpisodeFile.Episodes[0].SeasonNumber, erm.EpisodeFile.Episodes[0].EpisodeNumber,
                                     erm.EpisodeFile.Episodes[0].Title);
            }

            var epNumberString = String.Empty;
            var epNameString = String.Empty;

            foreach (var episode in erm.EpisodeFile.Episodes)
            {
                epNumberString = epNumberString + String.Format("E{0:00}", episode.EpisodeNumber);
                epNameString = epNameString + String.Format("+ {0}", episode.Title).Trim(' ', '+');
            }

            return String.Format("{0} - S{1:00}E{2} - {3}", erm.SeriesName, erm.EpisodeFile.Episodes[0].SeasonNumber,
                                 epNumberString, epNameString);
        }


    }
}
