using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers
{
    public class MediaFileProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly string[] MediaExtentions = new[] { "*.mkv", "*.avi", "*.wmv", "*.mp4" };
        private readonly DiskProvider _diskProvider;
        private readonly EpisodeProvider _episodeProvider;
        private readonly SeriesProvider _seriesProvider;
        private readonly IRepository _repository;

        public MediaFileProvider(IRepository repository, DiskProvider diskProvider,
                                 EpisodeProvider episodeProvider, SeriesProvider seriesProvider)
        {
            _repository = repository;
            _diskProvider = diskProvider;
            _episodeProvider = episodeProvider;
            _seriesProvider = seriesProvider;
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

                    //Stores the list of episodes to add to the EpisodeFile
                    var episodes = new List<Episode>();

                    //Check for daily shows
                    if (parseResult.Episodes == null)
                    {
                        var episode = _episodeProvider.GetEpisode(series.SeriesId, parseResult.AirDate.Date);

                        if (episode != null)
                        {
                            episodes.Add(episode);
                        }
                        else
                        {
                            Logger.Warn("Unable to find '{0}' in the database. File:{1}", parseResult, filePath);
                        }
                    }
                    else
                    {
                        foreach (var episodeNumber in parseResult.Episodes)
                        {
                            var episode = _episodeProvider.GetEpisode(series.SeriesId, parseResult.SeasonNumber,
                                                                      episodeNumber);

                            if (episode != null)
                            {
                                episodes.Add(episode);
                            }
                            else
                            {
                                Logger.Warn("Unable to find '{0}' in the database. File:{1}", parseResult, filePath);
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
                    episodeFile.Quality = parseResult.Quality;
                    episodeFile.Proper = Parser.ParseProper(filePath);
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

        private List<string> GetMediaFileList(string path)
        {
            Logger.Debug("Scanning '{0}' for episodes", path);

            var mediaFileList = new List<string>();

            foreach (var ext in MediaExtentions)
            {
                mediaFileList.AddRange(_diskProvider.GetFiles(path, ext, SearchOption.AllDirectories));
            }

            Logger.Trace("{0} media files were found in {1}", mediaFileList.Count, path);
            return mediaFileList;
        }
    }
}