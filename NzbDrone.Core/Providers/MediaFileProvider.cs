using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ninject;
using NLog;
using NzbDrone.Core.Helpers;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers
{
    public class MediaFileProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly string[] MediaExtentions = new[] { ".mkv", ".avi", ".wmv", ".mp4" };
        private readonly DiskProvider _diskProvider;
        private readonly EpisodeProvider _episodeProvider;
        private readonly SeriesProvider _seriesProvider;
        private readonly IRepository _repository;
        private readonly ConfigProvider _configProvider;

        [Inject]
        public MediaFileProvider(IRepository repository, DiskProvider diskProvider,
                                 EpisodeProvider episodeProvider, SeriesProvider seriesProvider,
                                 ConfigProvider configProvider)
        {
            _repository = repository;
            _diskProvider = diskProvider;
            _episodeProvider = episodeProvider;
            _seriesProvider = seriesProvider;
            _configProvider = configProvider;
        }

        public MediaFileProvider() { }

        /// <summary>
        ///   Scans the specified series folder for media files
        /// </summary>
        /// <param name = "series">The series to be scanned</param>
        public virtual List<EpisodeFile> Scan(Series series)
        {
            var mediaFileList = GetMediaFileList(series.Path);
            var fileList = new List<EpisodeFile>();

            foreach (var filePath in mediaFileList)
            {
                var file = ImportFile(series, filePath);
                if (file != null)
                    fileList.Add(file);
            }

            series.LastDiskSync = DateTime.Now;
            _seriesProvider.UpdateSeries(series);

            return fileList;
        }

        public virtual EpisodeFile ImportFile(Series series, string filePath)
        {
            Logger.Trace("Importing file to database [{0}]", filePath);

            try
            {
                var size = _diskProvider.GetSize(filePath);

                //If Size is less than 50MB and contains sample. Check for Size to ensure its not an episode with sample in the title
                if (size < 40000000 && filePath.ToLower().Contains("sample"))
                {
                    Logger.Trace("[{0}] appears to be a sample. skipping.", filePath);
                    return null;
                }

                //Check to see if file already exists in the database
                if (!_repository.Exists<EpisodeFile>(e => e.Path == Parser.NormalizePath(filePath)))
                {
                    var parseResult = Parser.ParseEpisodeInfo(filePath);

                    if (parseResult == null)
                        return null;

                    parseResult.CleanTitle = series.Title;//replaces the nasty path as title to help with logging

                    //Stores the list of episodes to add to the EpisodeFile
                    var episodes = new List<Episode>();

                    //Check for daily shows
                    if (parseResult.EpisodeNumbers == null)
                    {
                        var episode = _episodeProvider.GetEpisode(series.SeriesId, parseResult.AirDate.Date);

                        if (episode != null)
                        {
                            episodes.Add(episode);
                        }
                        else
                        {
                            Logger.Warn("Unable to find [{0}] in the database.[{1}]", parseResult, filePath);
                        }
                    }
                    else
                    {
                        foreach (var episodeNumber in parseResult.EpisodeNumbers)
                        {
                            var episode = _episodeProvider.GetEpisode(series.SeriesId, parseResult.SeasonNumber,
                                                                      episodeNumber);

                            if (episode != null)
                            {
                                episodes.Add(episode);
                            }
                            else
                            {
                                Logger.Warn("Unable to find [{0}] in the database.[{1}]", parseResult, filePath);
                            }
                        }
                    }

                    //Return null if no Episodes exist in the DB for the parsed episodes from file
                    if (episodes.Count <= 0)
                        return null;

                    var episodeFile = new EpisodeFile();
                    episodeFile.DateAdded = DateTime.Now;
                    episodeFile.SeriesId = series.SeriesId;
                    episodeFile.Path = Parser.NormalizePath(filePath);
                    episodeFile.Size = size;
                    episodeFile.Quality = parseResult.Quality.QualityType;
                    episodeFile.Proper = parseResult.Quality.Proper;
                    episodeFile.SeasonNumber = parseResult.SeasonNumber;
                    var fileId = (int)_repository.Add(episodeFile);

                    //This is for logging + updating the episodes that are linked to this EpisodeFile
                    string episodeList = String.Empty;
                    foreach (var ep in episodes)
                    {
                        ep.EpisodeFileId = fileId;
                        _episodeProvider.UpdateEpisode(ep);
                        episodeList += String.Format(", {0}", ep.EpisodeId).Trim(' ', ',');
                    }
                    Logger.Trace("File {0}:{1} attached to episode(s): '{2}'", episodeFile.EpisodeFileId, filePath,
                                 episodeList);

                    return episodeFile;
                }

                Logger.Trace("[{0}] already exists in the database. skipping.", filePath);
            }
            catch (Exception ex)
            {
                Logger.ErrorException("An error has occurred while importing file " + filePath, ex);
                throw;
            }
            return null;
        }

        /// <summary>
        ///   Removes files that no longer exist from the database
        /// </summary>
        /// <param name = "files">list of files to verify</param>
        public virtual void CleanUp(List<EpisodeFile> files)
        {
            //TODO: remove orphaned files. in files table but not linked to from episode table.
            foreach (var episodeFile in files)
            {
                if (!_diskProvider.FileExists(episodeFile.Path))
                {
                    Logger.Trace("File {0} no longer exists on disk. removing from database.", episodeFile.Path);

                    //Set the EpisodeFileId for each episode attached to this file to 0
                    foreach (var episode in episodeFile.Episodes)
                    {
                        episode.EpisodeFileId = 0;
                        _episodeProvider.UpdateEpisode(episode);
                    }

                    //Delete it from the DB
                    _repository.Delete<EpisodeFile>(episodeFile.EpisodeFileId);
                }
            }
        }

        public virtual void Update(EpisodeFile episodeFile)
        {
            _repository.Update(episodeFile);
        }

        public virtual EpisodeFile GetEpisodeFile(int episodeFileId)
        {
            return _repository.Single<EpisodeFile>(episodeFileId);
        }

        public virtual List<EpisodeFile> GetEpisodeFiles()
        {
            return _repository.All<EpisodeFile>().ToList();
        }

        public virtual IEnumerable<EpisodeFile> GetSeriesFiles(int seriesId)
        {
            return _repository.All<EpisodeFile>().Where(c => c.SeriesId == seriesId);
        }

        public virtual Tuple<int, int> GetEpisodeFilesCount(int seriesId)
        {
            var allEpisodes = _episodeProvider.GetEpisodeBySeries(seriesId);

            var episodeTotal = allEpisodes.Where(e => !e.Ignored && e.AirDate <= DateTime.Today && e.AirDate.Year > 1900);
            var avilableEpisodes = episodeTotal.Where(e => e.EpisodeFileId > 0);

            return new Tuple<int, int>(avilableEpisodes.Count(), episodeTotal.Count());
        }

        private List<string> GetMediaFileList(string path)
        {
            Logger.Debug("Scanning '{0}' for episodes", path);

            var filesOnDisk = _diskProvider.GetFiles(path, "*.*", SearchOption.AllDirectories);

            var mediaFileList = filesOnDisk.Where(c => MediaExtentions.Contains(Path.GetExtension(c).ToLower())).ToList();

            Logger.Debug("{0} media files were found in {1}", mediaFileList.Count, path);
            return mediaFileList;
        }

        public virtual List<EpisodeFile> ImportNewFiles(string path, Series series)
        {
            var result = new List<EpisodeFile>();

            //Get all the files except those that are considered samples
            var files = GetMediaFileList(path).Where(f => _diskProvider.GetSize(f) > 40000000 || !f.ToLower().Contains("sample")).ToList();

            foreach (var file in files)
            {
                try
                {
                    //Parse the filename
                    var parseResult = Parser.ParseEpisodeInfo(Path.GetFileName(file));
                    parseResult.Series = series;
                    parseResult.Episodes = _episodeProvider.GetEpisodes(parseResult);

                    if (parseResult.Episodes.Count == 0)
                    {
                        Logger.Error("File '{0}' contains invalid episode information, skipping import", file);
                        continue;
                    }

                    var ext = _diskProvider.GetExtension(file);
                    var filename = GetNewFilename(parseResult.Episodes, series.Title, parseResult.Quality.QualityType) + ext;
                    var folder = series.Path + Path.DirectorySeparatorChar;
                    if (_configProvider.UseSeasonFolder)
                        folder += _configProvider.SeasonFolderFormat
                                    .Replace("%0s", parseResult.SeasonNumber.ToString("00"))
                                    .Replace("%s", parseResult.SeasonNumber.ToString())
                                    + Path.DirectorySeparatorChar;

                    _diskProvider.CreateDirectory(folder);

                    //Get a list of episodeFiles that we need to delete and cleanup
                    var episodeFilesToClean = new List<EpisodeFile>();

                    foreach (var episode in parseResult.Episodes)
                    {
                        if (episode.EpisodeFileId > 0)
                            episodeFilesToClean.Add(episode.EpisodeFile);
                    }

                    if (episodeFilesToClean.Count != episodeFilesToClean.Where(e => parseResult.Quality.QualityType >= e.Quality).Count())
                    {
                        Logger.Debug("Episode isn't an upgrade for all episodes in file: [{0}]. Skipping.", file);
                        continue;
                    }

                    //Delete the files and then cleanup!
                    foreach (var e in episodeFilesToClean)
                    {
                        if (_diskProvider.FileExists(e.Path))
                            _diskProvider.DeleteFile(e.Path);
                    }

                    CleanUp(episodeFilesToClean);

                    //Move the file
                    _diskProvider.RenameFile(file, folder + filename);

                    //Import into DB
                    result.Add(ImportFile(series, folder + filename));
                }

                catch (Exception ex)
                {
                    Logger.WarnException("Error importing new download: " + file, ex);
                }
            }

            //If we have imported all the non-sample files, delete the folder, requires a minimum of 1 file to be imported.
            if (files.Count() > 0 && files.Count() == result.Count)
            {
                Logger.Debug("All non-sample files have been processed, deleting folder: {0}", path);
                _diskProvider.DeleteFolder(path, true);
            }

            return result;
        }

        public virtual string GetNewFilename(IList<Episode> episodes, string seriesName, QualityTypes quality)
        {
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
                    title += seriesName;
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
                    title += String.Format(" [{0}]", quality);

                if (replaceSpaces)
                    title = title.Replace(' ', '.');

                Logger.Debug("New File Name is: {0}", title);
                return title;
            }

            var multiEpisodeStyle = EpisodeSortingHelper.GetMultiEpisodeStyle(_configProvider.MultiEpisodeStyle);

            if (useSeriesName)
            {
                title += seriesName;
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
                title += String.Format(" [{0}]", quality);

            if (replaceSpaces)
                title = title.Replace(' ', '.');

            Logger.Debug("New File Name is: {0}", title);
            return title;
        }

        public virtual bool RenameEpisodeFile(int episodeFileId, ProgressNotification notification)
        {
            var episodeFile = GetEpisodeFile(episodeFileId);

            if (episodeFile == null)
                return false;

            try
            {
                notification.CurrentMessage = String.Format("Renaming '{0}'", episodeFile.Path);

                var series = _seriesProvider.GetSeries(episodeFile.SeriesId);
                var folder = new FileInfo(episodeFile.Path).DirectoryName;
                var episodes = _episodeProvider.EpisodesByFileId(episodeFileId);
                var ext = _diskProvider.GetExtension(episodeFile.Path);

                var newFileName = GetNewFilename(episodes, series.Title, episodeFile.Quality);

                var newFile = folder + Path.DirectorySeparatorChar + newFileName + ext;

                //Do the rename
                _diskProvider.RenameFile(episodeFile.Path, newFile);

                //Update the filename in the DB
                episodeFile.Path = newFile;
                Update(episodeFile);

                notification.CurrentMessage = String.Format("Finished Renaming '{0}'", newFile);
            }

            catch (Exception e)
            {
                notification.CurrentMessage = String.Format("Failed to Rename '{0}'", episodeFile.Path);
                Logger.ErrorException("An error has occurred while renaming episode: " + episodeFile.Path, e);
                throw;
            }
            return true;
        }
    }
}