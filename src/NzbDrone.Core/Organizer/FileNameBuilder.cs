using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Organizer
{
    public interface IBuildFileNames
    {
        string BuildFilename(IList<Episode> episodes, Series series, EpisodeFile episodeFile);
        string BuildFilename(IList<Episode> episodes, Series series, EpisodeFile episodeFile, NamingConfig namingConfig);
        string BuildFilePath(Series series, int seasonNumber, string fileName, string extension);
    }

    public interface INamingConfigService
    {
        NamingConfig GetConfig();
        NamingConfig Save(NamingConfig namingConfig);
    }

    public class NamingConfigService : INamingConfigService
    {
        private readonly IBasicRepository<NamingConfig> _repository;

        public NamingConfigService(IBasicRepository<NamingConfig> repository)
        {
            _repository = repository;
        }

        public NamingConfig GetConfig()
        {
            var config = _repository.SingleOrDefault();

            if (config == null)
            {
                _repository.Insert(NamingConfig.Default);
                config = _repository.Single();
            }

            return config;
        }

        public NamingConfig Save(NamingConfig namingConfig)
        {
            return _repository.Upsert(namingConfig);
        }
    }

    public class FileNameBuilder : IBuildFileNames
    {
        private readonly IConfigService _configService;
        private readonly INamingConfigService _namingConfigService;
        private readonly Logger _logger;

        public FileNameBuilder(INamingConfigService namingConfigService, IConfigService configService, Logger logger)
        {
            _namingConfigService = namingConfigService;
            _configService = configService;
            _logger = logger;
        }

        public string BuildFilename(IList<Episode> episodes, Series series, EpisodeFile episodeFile)
        {
            var nameSpec = _namingConfigService.GetConfig();

            return BuildFilename(episodes, series, episodeFile, nameSpec);
        }

        public string BuildFilename(IList<Episode> episodes, Series series, EpisodeFile episodeFile, NamingConfig nameSpec)
        {
            if (!nameSpec.RenameEpisodes)
            {
                if (String.IsNullOrWhiteSpace(episodeFile.SceneName))
                {
                    return Path.GetFileNameWithoutExtension(episodeFile.Path);
                }

                return episodeFile.SceneName;
            }

            var sortedEpisodes = episodes.OrderBy(e => e.EpisodeNumber);

            var numberStyle = GetNumberStyle(nameSpec.NumberStyle);

            var episodeNames = new List<string>
                {
                    Parser.Parser.CleanupEpisodeTitle(sortedEpisodes.First().Title)
                };

            var result = String.Empty;

            if (nameSpec.IncludeSeriesTitle)
            {
                result += series.Title + nameSpec.Separator;
            }

            if (series.SeriesType == SeriesTypes.Standard)
            {
                result += numberStyle.Pattern.Replace("%0e",
                                                      String.Format("{0:00}", sortedEpisodes.First().EpisodeNumber));

                if (episodes.Count > 1)
                {
                    var multiEpisodeStyle =
                        GetMultiEpisodeStyle(nameSpec.MultiEpisodeStyle);

                    foreach (var episode in sortedEpisodes.Skip(1))
                    {
                        if (multiEpisodeStyle.Name == "Duplicate")
                        {
                            result += nameSpec.Separator + numberStyle.Pattern;
                        }
                        else
                        {
                            result += multiEpisodeStyle.Pattern;
                        }

                        result = result.Replace("%0e", String.Format("{0:00}", episode.EpisodeNumber));
                        episodeNames.Add(Parser.Parser.CleanupEpisodeTitle(episode.Title));
                    }
                }

                result = result
                    .Replace("%s", String.Format("{0}", episodes.First().SeasonNumber))
                    .Replace("%0s", String.Format("{0:00}", episodes.First().SeasonNumber))
                    .Replace("%x", numberStyle.EpisodeSeparator)
                    .Replace("%p", nameSpec.Separator);
            }

            else
            {
                if (!String.IsNullOrEmpty(episodes.First().AirDate))
                    result += episodes.First().AirDate;

                else
                    result += "Unknown";
            }

            if (nameSpec.IncludeEpisodeTitle)
            {
                if (episodeNames.Distinct().Count() == 1)
                    result += nameSpec.Separator + episodeNames.First();

                else
                    result += nameSpec.Separator + String.Join(" + ", episodeNames.Distinct());
            }

            if (nameSpec.IncludeQuality)
            {
                result += String.Format(" [{0}]", episodeFile.Quality.Quality);

                if (episodeFile.Quality.Proper)
                    result += " [Proper]";
            }

            if (nameSpec.ReplaceSpaces)
                result = result.Replace(' ', '.');

            _logger.Trace("New File Name is: [{0}]", result.Trim());
            return CleanFilename(result.Trim());
        }

        public string BuildFilePath(Series series, int seasonNumber, string fileName, string extension)
        {
            string path = series.Path;
            if (series.SeasonFolder)
            {
                string seasonFolder;

                if (seasonNumber == 0)
                {
                    seasonFolder = "Specials";
                }

                else
                {
                    seasonFolder = _configService.SeasonFolderFormat
                                                 .Replace("%sn", series.Title)
                                                 .Replace("%s.n", series.Title.Replace(' ', '.'))
                                                 .Replace("%s_n", series.Title.Replace(' ', '_'))
                                                 .Replace("%0s", seasonNumber.ToString("00"))
                                                 .Replace("%s", seasonNumber.ToString());
                }

                path = Path.Combine(path, seasonFolder);
            }

            return Path.Combine(path, fileName + extension);
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

        private static readonly List<EpisodeSortingType> NumberStyles = new List<EpisodeSortingType>
                                                                            {
                                                                                new EpisodeSortingType
                                                                                    {
                                                                                        Id = 0,
                                                                                        Name = "1x05",
                                                                                        Pattern = "%sx%0e",
                                                                                        EpisodeSeparator = "x"

                                                                                    },
                                                                                new EpisodeSortingType
                                                                                    {
                                                                                        Id = 1,
                                                                                        Name = "01x05",
                                                                                        Pattern = "%0sx%0e",
                                                                                        EpisodeSeparator = "x"
                                                                                    },
                                                                                new EpisodeSortingType
                                                                                    {
                                                                                        Id = 2,
                                                                                        Name = "S01E05",
                                                                                        Pattern = "S%0sE%0e",
                                                                                        EpisodeSeparator = "E"
                                                                                    },
                                                                                new EpisodeSortingType
                                                                                    {
                                                                                        Id = 3,
                                                                                        Name = "s01e05",
                                                                                        Pattern = "s%0se%0e",
                                                                                        EpisodeSeparator = "e"
                                                                                    }
                                                                            };

        private static readonly List<EpisodeSortingType> MultiEpisodeStyles = new List<EpisodeSortingType>
                                                                                  {
                                                                                      new EpisodeSortingType
                                                                                          {
                                                                                              Id = 0,
                                                                                              Name = "Extend",
                                                                                              Pattern = "-%0e"
                                                                                          },
                                                                                      new EpisodeSortingType
                                                                                          {
                                                                                              Id = 1,
                                                                                              Name = "Duplicate",
                                                                                              Pattern = "%p%0s%x%0e"
                                                                                          },
                                                                                      new EpisodeSortingType
                                                                                          {
                                                                                              Id = 2,
                                                                                              Name = "Repeat",
                                                                                              Pattern = "%x%0e"
                                                                                          },
                                                                                        new EpisodeSortingType
                                                                                          {
                                                                                              Id = 3,
                                                                                              Name = "Scene",
                                                                                              Pattern = "-%x%0e"
                                                                                          }
                                                                                  };


        private static EpisodeSortingType GetNumberStyle(int id)
        {
            return NumberStyles.Single(s => s.Id == id);
        }

        private static EpisodeSortingType GetMultiEpisodeStyle(int id)
        {
            return MultiEpisodeStyles.Single(s => s.Id == id);
        }
    }
}