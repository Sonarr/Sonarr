using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ninject;
using NLog;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using PetaPoco;

namespace NzbDrone.Core.Providers
{
    public class DiskScanProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly string[] MediaExtentions = new[] {".mkv", ".avi", ".wmv", ".mp4"};
        private readonly IDatabase _database;
        private readonly DiskProvider _diskProvider;
        private readonly EpisodeProvider _episodeProvider;
        private readonly MediaFileProvider _mediaFileProvider;
        private readonly SeriesProvider _seriesProvider;

        [Inject]
        public DiskScanProvider(DiskProvider diskProvider, EpisodeProvider episodeProvider,
                                SeriesProvider seriesProvider, MediaFileProvider mediaFileProvider,
                                IDatabase database)
        {
            _diskProvider = diskProvider;
            _episodeProvider = episodeProvider;
            _seriesProvider = seriesProvider;
            _mediaFileProvider = mediaFileProvider;
            _database = database;
        }


        public DiskScanProvider()
        {
        }

        /// <summary>
        ///   Scans the specified series folder for media files
        /// </summary>
        /// <param name = "series">The series to be scanned</param>
        public virtual List<EpisodeFile> Scan(Series series)
        {
            return Scan(series, series.Path);
        }

        /// <summary>
        ///   Scans the specified series folder for media files
        /// </summary>
        /// <param name = "series">The series to be scanned</param>
        /// <param name="path">Path to scan</param>
        public virtual List<EpisodeFile> Scan(Series series, string path)
        {
            if (_episodeProvider.GetEpisodeBySeries(series.SeriesId).Count == 0)
            {
                Logger.Debug("Series {0} has no episodes. skipping", series.Title);
                return new List<EpisodeFile>();
            }

            var mediaFileList = GetVideoFiles(path);
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

            if (_database.Exists<EpisodeFile>("Path =@0", Parser.NormalizePath(filePath)))
            {
                Logger.Trace("[{0}] already exists in the database. skipping.", filePath);
                return null;
            }

            long size = _diskProvider.GetSize(filePath);

            //If Size is less than 50MB and contains sample. Check for Size to ensure its not an episode with sample in the title
            if (size < 40000000 && filePath.ToLower().Contains("sample"))
            {
                Logger.Trace("[{0}] appears to be a sample. skipping.", filePath);
                return null;
            }

            var parseResult = Parser.ParseEpisodeInfo(filePath);

            if (parseResult == null)
                return null;

            parseResult.CleanTitle = series.Title; //replaces the nasty path as title to help with logging

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
            int fileId = Convert.ToInt32(_database.Insert(episodeFile));

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


        public virtual bool RenameEpisodeFile(EpisodeFile episodeFile)
        {
            if (episodeFile == null)
                throw new ArgumentNullException("episodeFile");

            var series = _seriesProvider.GetSeries(episodeFile.SeriesId);
            string ext = _diskProvider.GetExtension(episodeFile.Path);
            var episodes = _episodeProvider.GetEpisodesByFileId(episodeFile.EpisodeFileId);
            string newFileName = _mediaFileProvider.GetNewFilename(episodes, series.Title, episodeFile.Quality);

            var newFile = _mediaFileProvider.CalculateFilePath(series, episodes.First().SeasonNumber, newFileName, ext);

            //Do the rename
            _diskProvider.RenameFile(episodeFile.Path, newFile.FullName);

            //Update the filename in the DB
            episodeFile.Path = newFile.FullName;
            _mediaFileProvider.Update(episodeFile);


            return true;
        }


        /// <summary>
        ///   Removes files that no longer exist on disk from the database
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
                    _database.Delete<EpisodeFile>(episodeFile.EpisodeFileId);
                }
            }
        }


        private List<string> GetVideoFiles(string path)
        {
            Logger.Debug("Scanning '{0}' for episodes", path);

            var filesOnDisk = _diskProvider.GetFiles(path, "*.*", SearchOption.AllDirectories);

            var mediaFileList = filesOnDisk.Where(c => MediaExtentions.Contains(Path.GetExtension(c).ToLower())).ToList();

            Logger.Debug("{0} media files were found in {1}", mediaFileList.Count, path);
            return mediaFileList;
        }
    }
}