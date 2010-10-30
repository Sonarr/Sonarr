using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers
{
    public class MediaFileProvider : IMediaFileProvider
    {
        private readonly IRepository _repository;
        private readonly IConfigProvider _configProvider;
        private readonly IDiskProvider _diskProvider;
        private readonly IEpisodeProvider _episodeProvider;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly string[] MediaExtentions = new[] { "*.mkv", "*.avi", "*.wmv" };

        public MediaFileProvider(IRepository repository, IConfigProvider _configProvider, IDiskProvider diskProvider, IEpisodeProvider episodeProvider)
        {
            _repository = repository;
            this._configProvider = _configProvider;
            _diskProvider = diskProvider;
            _episodeProvider = episodeProvider;
        }

        /// <summary>
        /// Scans the specified series folder for media files
        /// </summary>
        /// <param name="series">The series to be scanned</param>
        public void Scan(Series series)
        {
            var mediaFileList = GetMediaFileList(series.Path);

            foreach (var filePath in mediaFileList)
            {
                ImportFile(series, filePath);
            }
        }

        public EpisodeFile ImportFile(Series series, string filePath)
        {
            Logger.Trace("Importing file to database [{0}]", filePath);

            if (!_repository.Exists<EpisodeFile>(e => e.Path == Parser.NormalizePath(filePath)))
            {
                var episodesInFile = Parser.ParseEpisodeInfo(filePath);

                foreach (var parsedEpisode in episodesInFile)
                {
                    EpisodeParseResult closureEpisode = parsedEpisode;
                    var episode = _episodeProvider.GetEpisode(series.SeriesId, closureEpisode.SeasonNumber, closureEpisode.EpisodeNumber);
                    if (episode != null)
                    {
                        var episodeFile = new EpisodeFile();
                        episodeFile.DateAdded = DateTime.Now;
                        episodeFile.SeriesId = series.SeriesId;
                        episodeFile.EpisodeId = episode.EpisodeId;
                        episodeFile.Path = Parser.NormalizePath(filePath);
                        episodeFile.Size = _diskProvider.GetSize(filePath);
                        episodeFile.Quality = Parser.ParseQuality(filePath);
                        episodeFile.Proper = Parser.ParseProper(filePath);
                        _repository.Add(episodeFile);
                        Logger.Trace("File {0}:{1} attached to '{2}'", episodeFile.FileId, filePath, episode.EpisodeId);

                        return episodeFile;
                    }

                    Logger.Warn("Unable to find Series:{0} Season:{1} Episode:{2} in the database. File:{3}", series.Title, closureEpisode.SeasonNumber, closureEpisode.EpisodeNumber, filePath);
                }
            }
            else
            {
                Logger.Trace("[{0}] already exists in the database. skipping.", filePath);
            }

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
                    _repository.Delete<EpisodeFile>(episodeFile);
                }
            }
        }


        public string GenerateEpisodePath(EpisodeModel episode)
        {
            var episodeNamePattern = _configProvider.EpisodeNameFormat;

            episodeNamePattern = episodeNamePattern.Replace("{series}", "{0}");
            episodeNamePattern = episodeNamePattern.Replace("{episode", "{1");
            episodeNamePattern = episodeNamePattern.Replace("{season", "{2");
            episodeNamePattern = episodeNamePattern.Replace("{title}", "{3}");
            episodeNamePattern = episodeNamePattern.Replace("{quality}", "{4}");

            return String.Format(episodeNamePattern, episode.SeriesTitle, episode.EpisodeNumber, episode.SeasonNumber, episode.EpisodeTitle, episode.Quality);
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
