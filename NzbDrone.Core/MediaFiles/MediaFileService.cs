using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Helpers;

namespace NzbDrone.Core.MediaFiles
{
    public interface IMediaFileService
    {
        EpisodeFile Add(EpisodeFile episodeFile);
        void Update(EpisodeFile episodeFile);
        void Delete(int episodeFileId);
        bool Exists(string path);
        EpisodeFile GetFileByPath(string path);
        IList<EpisodeFile> GetFilesBySeries(int seriesId);
        IList<EpisodeFile> GetFilesBySeason(int seriesId, int seasonNumber);
        FileInfo CalculateFilePath(Series series, int seasonNumber, string fileName, string extention);
        string GetNewFilename(IList<Episode> episodes, Series series, Quality quality, bool proper, EpisodeFile episodeFile);
    }

    public class MediaFileService : IMediaFileService
    {
        private readonly IConfigService _configService;
        private readonly IEpisodeService _episodeService;
        private readonly Logger _logger;
        private readonly IMediaFileRepository _mediaFileRepository;


        public MediaFileService(IMediaFileRepository mediaFileRepository, IConfigService configService, IEpisodeService episodeService, Logger logger)
        {
            _mediaFileRepository = mediaFileRepository;
            _configService = configService;
            _episodeService = episodeService;
            _logger = logger;
        }

        public EpisodeFile Add(EpisodeFile episodeFile)
        {
            return _mediaFileRepository.Insert(episodeFile);
        }

        public void Update(EpisodeFile episodeFile)
        {
            _mediaFileRepository.Update(episodeFile);
        }

        public void Delete(int episodeFileId)
        {
            _mediaFileRepository.Delete(episodeFileId);

            var ep = _episodeService.GetEpisodesByFileId(episodeFileId);

            foreach (var episode in ep)
            {
                _episodeService.SetEpisodeIgnore(episode.Id, true);
            }
        }

        public bool Exists(string path)
        {
            return GetFileByPath(path) != null;
        }

        public EpisodeFile GetFileByPath(string path)
        {
            return _mediaFileRepository.GetFileByPath(path.Normalize());
        }

        public IList<EpisodeFile> GetFilesBySeries(int seriesId)
        {
            return _mediaFileRepository.GetFilesBySeries(seriesId);
        }

        public IList<EpisodeFile> GetFilesBySeason(int seriesId, int seasonNumber)
        {
            return _mediaFileRepository.GetFilesBySeason(seriesId, seasonNumber);
        }


        public FileInfo CalculateFilePath(Series series, int seasonNumber, string fileName, string extention)
        {
            string path = series.Path;
            if (series.SeasonFolder)
            {
                var seasonFolder = _configService.SortingSeasonFolderFormat
                    .Replace("%0s", seasonNumber.ToString("00"))
                    .Replace("%s", seasonNumber.ToString());

                path = Path.Combine(path, seasonFolder);
            }

            path = Path.Combine(path, fileName + extention);

            return new FileInfo(path);
        }


        public string GetNewFilename(IList<Episode> episodes, Series series, Quality quality, bool proper, EpisodeFile episodeFile)
        {
            if (_configService.SortingUseSceneName)
            {
                _logger.Trace("Attempting to use scene name");
                if (String.IsNullOrWhiteSpace(episodeFile.SceneName))
                {
                    var name = Path.GetFileNameWithoutExtension(episodeFile.Path);
                    _logger.Trace("Unable to use scene name, because it is null, sticking with current name: {0}", name);

                    return name;
                }

                return episodeFile.SceneName;
            }

            var sortedEpisodes = episodes.OrderBy(e => e.EpisodeNumber);

            var separatorStyle = EpisodeSortingHelper.GetSeparatorStyle(_configService.SortingSeparatorStyle);
            var numberStyle = EpisodeSortingHelper.GetNumberStyle(_configService.SortingNumberStyle);

            var episodeNames = new List<String>();

            episodeNames.Add(Parser.CleanupEpisodeTitle(sortedEpisodes.First().Title));

            string result = String.Empty;

            if (_configService.SortingIncludeSeriesName)
            {
                result += series.Title + separatorStyle.Pattern;
            }

            if (series.SeriesType == SeriesType.Standard)
            {
                result += numberStyle.Pattern.Replace("%0e",
                                                      String.Format("{0:00}", sortedEpisodes.First().EpisodeNumber));

                if (episodes.Count > 1)
                {
                    var multiEpisodeStyle =
                            EpisodeSortingHelper.GetMultiEpisodeStyle(_configService.SortingMultiEpisodeStyle);

                    foreach (var episode in sortedEpisodes.Skip(1))
                    {
                        if (multiEpisodeStyle.Name == "Duplicate")
                        {
                            result += separatorStyle.Pattern + numberStyle.Pattern;
                        }
                        else
                        {
                            result += multiEpisodeStyle.Pattern;
                        }

                        result = result.Replace("%0e", String.Format("{0:00}", episode.EpisodeNumber));
                        episodeNames.Add(Parser.CleanupEpisodeTitle(episode.Title));
                    }
                }

                result = result
                        .Replace("%s", String.Format("{0}", episodes.First().SeasonNumber))
                        .Replace("%0s", String.Format("{0:00}", episodes.First().SeasonNumber))
                        .Replace("%x", numberStyle.EpisodeSeparator)
                        .Replace("%p", separatorStyle.Pattern);
            }

            else
            {
                if (episodes.First().AirDate.HasValue)
                    result += episodes.First().AirDate.Value.ToString("yyyy-MM-dd");

                else
                    result += "Unknown";
            }

            if (_configService.SortingIncludeEpisodeTitle)
            {
                if (episodeNames.Distinct().Count() == 1)
                    result += separatorStyle.Pattern + episodeNames.First();

                else
                    result += separatorStyle.Pattern + String.Join(" + ", episodeNames.Distinct());
            }

            if (_configService.SortingAppendQuality)
            {
                result += String.Format(" [{0}]", quality);

                if (proper)
                    result += " [Proper]";
            }

            if (_configService.SortingReplaceSpaces)
                result = result.Replace(' ', '.');

            _logger.Trace("New File Name is: [{0}]", result.Trim());
            return CleanFilename(result.Trim());
        }


        public static string CleanFilename(string name)
        {
            string result = name;
            string[] badCharacters = { "\\", "/", "<", ">", "?", "*", ":", "|", "\"" };
            string[] goodCharacters = { "+", "+", "{", "}", "!", "@", "-", "#", "`" };

            for (int i = 0; i < badCharacters.Length; i++)
                result = result.Replace(badCharacters[i], goodCharacters[i]);

            return result.Trim();
        }
    }
}