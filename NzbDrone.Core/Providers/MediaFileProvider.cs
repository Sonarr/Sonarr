using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers
{
    public class MediaFileProvider
    {
        private readonly IRepository _repository;
        private readonly ConfigProvider _configProvider;
        private readonly DiskProvider _diskProvider;
        private readonly EpisodeProvider _episodeProvider;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly string[] MediaExtentions = new[] { "*.mkv", "*.avi", "*.wmv" };

        public MediaFileProvider(IRepository repository, ConfigProvider configProvider, DiskProvider diskProvider, EpisodeProvider episodeProvider)
        {
            _repository = repository;
            _configProvider = configProvider;
            _diskProvider = diskProvider;
            _episodeProvider = episodeProvider;
        }

        public MediaFileProvider()
        {

        }

        /// <summary>
        /// Scans the specified series folder for media files
        /// </summary>
        /// <param name="series">The series to be scanned</param>
        public List<EpisodeFile> Scan(Series series)
        {
            var mediaFileList = GetMediaFileList(series.Path);
            var fileList = new List<EpisodeFile>();

            foreach (var filePath in mediaFileList)
            {
                var file = ImportFile(series, filePath);
                if (file != null)
                    fileList.Add(file);
            }
            return fileList;
        }

        /// <summary>
        /// Scans the specified series folder for media files
        /// </summary>
        /// <param name="series">The series to be scanned</param>
        public List<EpisodeFile> Scan(Series series, string path)
        {
            var mediaFileList = GetMediaFileList(path);
            var fileList = new List<EpisodeFile>();

            foreach (var filePath in mediaFileList)
            {
                var file = ImportFile(series, filePath);
                if (file != null)
                    fileList.Add(file);
            }
            return fileList;
        }

        public EpisodeFile ImportFile(Series series, string filePath)
        {
            Logger.Trace("Importing file to database [{0}]", filePath);

            if (!_repository.Exists<EpisodeFile>(e => e.Path == Parser.NormalizePath(filePath)))
            {
                var episodesInFile = Parser.ParseEpisodeInfo(filePath);

                //Stores the list of episodes to add to the EpisodeFile
                var episodes = new List<Episode>();

                foreach (var episodeNumber in episodesInFile.Episodes)
                {
                    var episode = _episodeProvider.GetEpisode(series.SeriesId, episodesInFile.SeasonNumber, episodeNumber);

                    if (episode != null)
                    {
                        episodes.Add(episode);
                    }

                    else
                        Logger.Warn("Unable to find Series:{0} Season:{1} Episode:{2} in the database. File:{3}", series.Title, episodesInFile.SeasonNumber, episodeNumber, filePath);
                }

                //Return null if no Episodes exist in the DB for the parsed episodes from file
                if (episodes.Count < 1)
                    return null;

                var size = _diskProvider.GetSize(filePath);

                //If Size is less than 50MB and contains sample. Check for Size to ensure its not an episode with sample in the title
                if (size < 50000000 && filePath.ToLower().Contains("sample"))
                {
                    Logger.Trace("[{0}] appears to be a sample... skipping.", filePath);
                    return null;
                }

                var episodeFile = new EpisodeFile();
                episodeFile.DateAdded = DateTime.Now;
                episodeFile.SeriesId = series.SeriesId;
                episodeFile.Path = Parser.NormalizePath(filePath);
                episodeFile.Size = size;
                episodeFile.Quality = episodesInFile.Quality;
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
                Logger.Trace("File {0}:{1} attached to episode(s): '{2}'", episodeFile.EpisodeFileId, filePath, episodeList);

                return episodeFile;
            }

            Logger.Trace("[{0}] already exists in the database. skipping.", filePath);
            return null;
        }

        /// <summary>
        /// Removes files that no longer exist from the database
        /// </summary>
        /// <param name="files">list of files to verify</param>
        public void CleanUp(List<EpisodeFile> files)
        {
            foreach (var episodeFile in files)
            {
                if (!_diskProvider.FileExists(episodeFile.Path))
                {
                    Logger.Trace("File {0} no longer exists on disk. removing from database.", episodeFile.Path);
                    _repository.Delete<EpisodeFile>(episodeFile.EpisodeFileId);
                }
            }
        }


        public void DeleteFromDb(int fileId)
        {
            _repository.Delete<EpisodeFile>(fileId);
        }

        public void DeleteFromDisk(int fileId, string path)
        {
            _diskProvider.DeleteFile(path);
            _repository.Delete<EpisodeFile>(fileId);
        }

        public void Update(EpisodeFile episodeFile)
        {
            _repository.Update(episodeFile);
        }

        public EpisodeFile GetEpisodeFile(int episodeFileId)
        {
            return _repository.Single<EpisodeFile>(episodeFileId);
        }

        public List<EpisodeFile> GetEpisodeFiles()
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
